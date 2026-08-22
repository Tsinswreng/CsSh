namespace Tsinswreng.CsSh;

internal sealed partial class CommandReadStream{
	internal partial CommandReadStream(Stream Inner, Action Start) {
		this.Inner = Inner;
		this.Start = Start;
	}

	public override partial void Flush() {
		throw new NotSupportedException();
	}

	public override partial Task FlushAsync(CT Ct) {
		throw new NotSupportedException();
	}

	public override partial i32 Read(byte[] Buffer, i32 Offset, i32 Count) {
		Start();
		return Inner.Read(Buffer, Offset, Count);
	}

	public override partial i32 Read(Span<byte> Buffer) {
		Start();
		return Inner.Read(Buffer);
	}

	public override partial Task<i32> ReadAsync(byte[] Buffer, i32 Offset, i32 Count, CT Ct) {
		Start();
		return Inner.ReadAsync(Buffer, Offset, Count, Ct);
	}

	public override partial ValueTask<i32> ReadAsync(Memory<byte> Buffer, CT Ct) {
		Start();
		return Inner.ReadAsync(Buffer, Ct);
	}

	public override partial long Seek(long Offset, SeekOrigin Origin) {
		throw new NotSupportedException();
	}

	public override partial void SetLength(long Value) {
		throw new NotSupportedException();
	}

	public override partial void Write(byte[] Buffer, i32 Offset, i32 Count) {
		throw new NotSupportedException();
	}
}
