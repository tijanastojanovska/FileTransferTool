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

const int chunkSize = 1024 * 1024;

CopyFileInChunks(sourcePath, destinationPath, chunkSize);

Console.WriteLine();
Console.WriteLine("Transfer complete");

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

void CopyFileInChunks(string sourceFilePath, string destinationFilePath, int chunkSize)
{
	using FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

	using FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

	byte[] buffer = new byte[chunkSize];
	long position = 0;
	int blockNumber = 1;
	int bytesRead;

	while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
	{
		destinationStream.Write(buffer, 0, bytesRead);

		Console.WriteLine($"{blockNumber}) position = {position}, bytes = {bytesRead}");

		position += bytesRead;
		blockNumber++;
	}
}