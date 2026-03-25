using FileTransferTool.Models;
using System.Security.Cryptography;

namespace FileTransferTool.Services
{
	/// <summary>
	/// Handles file transfer operations including chunked copying and verification
	/// </summary>
	public class FileTransferService
	{
		public async Task CopyFileInChunksAsync(string sourceFilePath, string destinationFilePath, FileTransferConfig config)
		{
			using FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using (File.Create(destinationFilePath)) { } // Create the destination file that later parallel chunks can open and write into

			long position = 0;
			int blockNumber = 1;
			int bytesRead;

			// Calculate total number of chunks needed(rounding up for partial last chunk)
			long totalChunks = (sourceStream.Length + config.ChunkSize - 1) / config.ChunkSize;

			const int batchSize = 8;

			while (true)
			{
				List<TransferChunk> batch = new List<TransferChunk>();

				for (int i = 0; i < batchSize; i++)
				{
					byte[] buffer = new byte[config.ChunkSize];
					bytesRead = sourceStream.Read(buffer, 0, buffer.Length);

					if (bytesRead == 0)
					{
						break;
					}

					byte[] actualBuffer;

					if (bytesRead == config.ChunkSize)
					{
						actualBuffer = buffer; // BytesRead is the same as chunkSize so we don't have empty unused part of the chunk
					}
					else
					{
						actualBuffer = new byte[bytesRead];
						Array.Copy(buffer, actualBuffer, bytesRead); // If the chunk is smaller create new buffer with actual read size (usually for last chunk)
					}

					TransferChunk chunk = new TransferChunk
					{
						Buffer = actualBuffer,
						VerifyBuffer = new byte[bytesRead],
						BytesRead = bytesRead,
						Position = position,
						BlockNumber = blockNumber
					};

					Console.WriteLine($"Preparing chunk {blockNumber} of {totalChunks}...");
					batch.Add(chunk);

					position += bytesRead;
					blockNumber++;
				}

				if (batch.Count == 0)
				{
					break;
				}

				await Parallel.ForEachAsync(batch, async (chunk, _) =>
				{
					using FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, chunk.BytesRead, FileOptions.Asynchronous);

					string hash = await ProcessChunkAsync(destinationStream, chunk, config.MaxRetries);
					Console.WriteLine($"{chunk.BlockNumber}) position = {chunk.Position}, hash = {hash}");
				});
			}
		}

		private async Task<string> ProcessChunkAsync(FileStream destinationStream, TransferChunk chunk, int maxRetries)
		{
			string sourceHash = GenerateMD5Hash(chunk.Buffer, chunk.BytesRead);

			for (int attempt = 1; attempt <= maxRetries; attempt++)
			{
				destinationStream.Position = chunk.Position;
				await destinationStream.WriteAsync(chunk.Buffer, 0, chunk.BytesRead);
				await destinationStream.FlushAsync(); // Make sure data is written before verifying the chunk

				destinationStream.Position = chunk.Position; // Reset position so we read back the same chunk we just wrote
				int readBack = await destinationStream.ReadAsync(chunk.VerifyBuffer, 0, chunk.BytesRead);

				if (readBack != chunk.BytesRead)
				{
					Console.WriteLine($"Block {chunk.BlockNumber}: read mismatch (attempt {attempt})");
					continue;
				}

				string destinationHash = GenerateMD5Hash(chunk.VerifyBuffer, readBack);

				if (destinationHash == sourceHash)
				{
					return destinationHash;
				}

				Console.WriteLine($"Block {chunk.BlockNumber}: hash mismatch (attempt {attempt})");
			}

			throw new IOException($"Failed to copy block {chunk.BlockNumber} at position {chunk.Position} after {maxRetries} attempts");
		}

		private string GenerateMD5Hash(byte[] buffer, int bytesToHash)
		{
			using MD5 md5 = MD5.Create();
			byte[] hashBytes = md5.ComputeHash(buffer, 0, bytesToHash);
			return Convert.ToHexString(hashBytes);
		}

		public string GenerateFileSha256(string filePath)
		{
			using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using SHA256 sha256 = SHA256.Create();

			byte[] hashBytes = sha256.ComputeHash(fileStream);
			return Convert.ToHexString(hashBytes);
		}
	}
}
