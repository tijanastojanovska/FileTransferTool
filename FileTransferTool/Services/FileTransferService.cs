using FileTransferTool.Models;
using System.Security.Cryptography;

namespace FileTransferTool.Services
{
	/// <summary>
	/// Handles file transfer operations including chunked copying and verification
	/// </summary>
	public class FileTransferService
	{
		public void CopyFileInChunks(string sourceFilePath, string destinationFilePath, FileTransferConfig config)
		{
			using FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

			byte[] buffer = new byte[config.ChunkSize];
			byte[] verifyBuffer = new byte[config.ChunkSize];

			long position = 0;
			int blockNumber = 1;
			int bytesRead;

			// Calculate total number of chunks needed(rounding up for partial last chunk)
			long totalChunks = (sourceStream.Length + config.ChunkSize - 1) / config.ChunkSize;

			while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
			{
				Console.WriteLine($"Copying chunk {blockNumber} of {totalChunks}...");

				TransferChunk chunk = new TransferChunk()
				{
					Buffer = buffer,
					VerifyBuffer = verifyBuffer,
					BytesRead = bytesRead,
					Position = position,
					BlockNumber = blockNumber
				};

				ProcessChunk(destinationStream, chunk, config.MaxRetries);

				position += bytesRead;
				blockNumber++;
			}
		}

		private void ProcessChunk(FileStream destinationStream, TransferChunk chunk, int maxRetries)
		{
			string sourceHash = GenerateMD5Hash(chunk.Buffer, chunk.BytesRead);

			for (int attempt = 1; attempt <= maxRetries; attempt++)
			{
				destinationStream.Position = chunk.Position;
				destinationStream.Write(chunk.Buffer, 0, chunk.BytesRead);
				destinationStream.Flush(); // Make sure data is written before verifying the chunk

				destinationStream.Position = chunk.Position; // Reset position so we read back the same chunk we just wrote
				int readBack = destinationStream.Read(chunk.VerifyBuffer, 0, chunk.BytesRead);

				if (readBack != chunk.BytesRead)
				{
					Console.WriteLine($"Block {chunk.BlockNumber}: read mismatch (attempt {attempt})");
					continue;
				}

				string destinationHash = GenerateMD5Hash(chunk.VerifyBuffer, readBack);

				if (destinationHash == sourceHash)
				{
					Console.WriteLine($"{chunk.BlockNumber}) position = {chunk.Position}, hash = {sourceHash}");
					return;
				}

				Console.WriteLine($"Block {chunk.BlockNumber}: hash mismatch (attempt {attempt})");
			}

			throw new IOException($"Failed to copy block {chunk.BlockNumber} at position {chunk.Position} after {maxRetries}");
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
