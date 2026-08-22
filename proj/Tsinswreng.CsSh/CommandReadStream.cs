namespace Tsinswreng.CsSh;

/// 讀取前啟動所屬 Command 的只讀 Stream 包裝。
/// 包裝不緩衝資料，讀取會直接進入 PipeReader 對應的非同步 Stream。
internal sealed partial class CommandReadStream:Stream{
	/// 實際承載命令輸出資料的 PipeReader Stream。
	private readonly Stream Inner;
	/// 首次讀取時啟動所屬命令的回呼。
	private readonly Action Start;

	/// 建立資料流包裝；不會立即啟動命令。
	internal partial CommandReadStream(Stream Inner, Action Start);

	public override bool CanRead => Inner.CanRead;
	public override bool CanSeek => false;
	public override bool CanWrite => false;
	public override long Length => throw new NotSupportedException();
	public override long Position{get => throw new NotSupportedException(); set => throw new NotSupportedException();}
	/// 此唯讀資料流不支援同步 Flush。
	public override partial void Flush();
	/// 此唯讀資料流不支援非同步 Flush。
	public override partial Task FlushAsync(CT Ct);
	/// 啟動命令後讀取同步資料。
	public override partial i32 Read(byte[] Buffer, i32 Offset, i32 Count);
	/// 啟動命令後讀取同步資料。
	public override partial i32 Read(Span<byte> Buffer);
	/// 啟動命令後讀取非同步資料。
	public override partial Task<i32> ReadAsync(byte[] Buffer, i32 Offset, i32 Count, CT Ct);
	/// 啟動命令後讀取非同步資料。
	public override partial ValueTask<i32> ReadAsync(Memory<byte> Buffer, CT Ct = default);
	/// 此資料流不支援定位。
	public override partial long Seek(long Offset, SeekOrigin Origin);
	/// 此資料流不支援改變長度。
	public override partial void SetLength(long Value);
	/// 此資料流不支援寫入。
	public override partial void Write(byte[] Buffer, i32 Offset, i32 Count);
}
