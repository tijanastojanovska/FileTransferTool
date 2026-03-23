using FileTransferTool.Models;
using FileTransferTool.Services;

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

FileTransferConfig  config = new FileTransferConfig();

FileTransferService fileTransferService = new FileTransferService();
fileTransferService.CopyFileInChunks(sourcePath, destinationPath, config);

Console.WriteLine();
Console.WriteLine("Chunk transfer complete");

Console.WriteLine();
Console.WriteLine("Calculating final file checksums...");

string sourceFileHash = fileTransferService.GenerateFileSha256(sourcePath);
string destinationFileHash = fileTransferService.GenerateFileSha256(destinationPath);

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

#region Input Helpers
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

#endregion
