namespace FileTransferTool.Models
{
	/// <summary>
	/// Configuration settings for file transfer
	/// </summary>
	public class FileTransferConfig
	{
		/// <summary>
		/// 1MB is small enough for a chunk to transfer, but also large enough so that we don't have too many chunks when working with larger files
		/// </summary>
		public int ChunkSize { get; set; } = 1024 * 1024;

		/// <summary>
		/// Number of retry attempts if source and destination hashes do not match
		/// </summary>
		public int MaxRetries { get; set; } = 3;
	}
}
