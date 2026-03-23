using System.Security.Cryptography;

Console.WriteLine("===== File Transfer Tool =====");
Console.WriteLine();

string sourcePath = GetValidSourcePath();
string destinationDirectory = GetValidDestinationDirectory();

string fileName = Path.GetFileName(sourcePath);
string destinationPath = Path.Combine(destinationDirectory, fileName);

if (string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
{
	Console.WriteLine("Source and destination cannot be the same!");
	return;
}

Console.WriteLine();
Console.WriteLine($"Source File: {sourcePath}");
Console.WriteLine($"Destination File: {destinationPath}");
Console.WriteLine();

Console.WriteLine("Setup complete. Ready to begin transfer...");

//1MB is small enough for a chunk to transfer, but also large enough so that I don't have too many chunks when working with larger files
const int chunkSize = 1024 * 1024;
const int maxRetries = 3; //retries if source and destination hashes do not match

CopyFileInChunks(sourcePath, destinationPath, chunkSize, maxRetries);

Console.WriteLine();
Console.WriteLine("Chunk transfer complete");

Console.WriteLine();
Console.WriteLine("Calculating final file checksums...");

string sourceFileHash = GenerateFileSha256(sourcePath);
string destinationFileHash = GenerateFileSha256(destinationPath);

Console.WriteLine($"Source SHA-256: {sourceFileHash}");
Console.WriteLine($"Destination SHA-256: {destinationFileHash}");

if (sourceFileHash == destinationFileHash)
{
	Console.WriteLine("Final file verification successful");
}
else
{
	Console.WriteLine("Final file verification failed");
}
string GetValidSourcePath()
{
	while (true)
	{
		Console.Write("Enter source file path: ");
		string? input = Console.ReadLine()?.Trim();

		if (string.IsNullOrWhiteSpace(input))
		{
			Console.WriteLine("Path cannot be empty. Please try again!");
			continue;
		}

		if (!File.Exists(input))
		{
			Console.WriteLine("File does not exist. Please try again!");
			continue;
		}

		return input;
	}
}

string GetValidDestinationDirectory()
{
	while (true)
	{
		Console.Write("Enter destination directory: ");
		string? input = Console.ReadLine()?.Trim();

		if (string.IsNullOrWhiteSpace(input))
		{
			Console.WriteLine("Path cannot be empty. Please try again!");
			continue;
		}

		try
		{
			if (!Directory.Exists(input))
			{
				Directory.CreateDirectory(input);
				Console.WriteLine("Destination directory created");
			}

			return input;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"An error occurred: {ex.Message}");
		}
	}
}

void CopyFileInChunks(string sourceFilePath, string destinationFilePath, int chunkSize, int maxRetries)
{
	using FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
	using FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

	byte[] buffer = new byte[chunkSize];
	byte[] verifyBuffer = new byte[chunkSize];

	long position = 0;
	int blockNumber = 1;
	int bytesRead;
	long totalChunks = (sourceStream.Length + chunkSize - 1) / chunkSize;

	while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
	{
		Console.WriteLine($"Copying chunk {blockNumber} of {totalChunks}...");

		string sourceHash = GenerateMD5Hash(buffer, bytesRead);
		bool success = false;

		for (int attempt = 1; attempt <= maxRetries; attempt++)
		{
			destinationStream.Position = position;
			destinationStream.Write(buffer, 0, bytesRead);
			destinationStream.Flush(); //make sure data is written before verifying the chunk

			destinationStream.Position = position; //reset position so we read back the same chunk we just wrote


			int readBack = destinationStream.Read(verifyBuffer, 0, bytesRead);

			if (readBack != bytesRead)
			{
				Console.WriteLine($"Block {blockNumber}: couldn't read back properly (attempt {attempt})");
				continue;
			}

			string destinationHash = GenerateMD5Hash(verifyBuffer, readBack);

			if (destinationHash == sourceHash)
			{
				Console.WriteLine($"{blockNumber}) position = {position}, hash = {sourceHash}");
				success = true;
				break;
			}

			Console.WriteLine($"Block {blockNumber}: hash mismatch (attempt {attempt})");
		}

		if (!success)
		{
			throw new IOException($"Failed to copy block at position {position}");
		}

		position += bytesRead;
		blockNumber++;
	}
}

string GenerateMD5Hash(byte[] buffer, int bytesToHash)
{
	using MD5 md5 = MD5.Create();
	byte[] hashBytes = md5.ComputeHash(buffer, 0, bytesToHash);
	return Convert.ToHexString(hashBytes);
}

string GenerateFileSha256(string filePath)
{
	using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
	using SHA256 sha256 = SHA256.Create();

	byte[] hashBytes = sha256.ComputeHash(fileStream);
	return Convert.ToHexString(hashBytes);
}