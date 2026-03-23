namespace FileTransferTool.Models
{
	/// <summary>
	/// Represents a single chunk of a file being transferred.
	/// </summary>
	public class TransferChunk
	{
		public required byte[] Buffer { get; set; }
		public required byte[] VerifyBuffer { get; set; }
		public int BytesRead { get; set; }
		public long Position { get; set; }
		public int BlockNumber { get; set; }
	}
}
