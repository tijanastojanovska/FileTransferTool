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