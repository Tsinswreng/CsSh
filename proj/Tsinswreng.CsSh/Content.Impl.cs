using System.Text;

namespace Tsinswreng.CsSh;

/// Implements Content's ownership, conversion and text-decoding behavior.
public sealed partial class Content{
	public partial Content(Stream Stream, ContentOptions? Options) {
		this.Stream = Stream ?? throw new ArgumentNullException(nameof(Stream));
		this.Options = Options ?? new();
	}

	public partial str Text() {
		EnsureReadable();
		using var Reader = new StreamReader(Stream, Options.Encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 81920, leaveOpen: true);
		return Reader.ReadToEnd();
	}

	public partial async Task<str> Text(CT Ct) {
		EnsureReadable();
		using var Reader = new StreamReader(Stream, Options.Encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 81920, leaveOpen: true);
		return await Reader.ReadToEndAsync(Ct).ConfigureAwait(false);
	}

	public partial void Dispose() {
		if (!Options.LeaveOpen)
			Stream.Dispose();
	}

	public partial async ValueTask DisposeAsync() {
		if (!Options.LeaveOpen)
			await Stream.DisposeAsync().ConfigureAwait(false);
	}

	/// Rejects text conversion of a write-only target before StreamReader produces a less useful exception.
	private void EnsureReadable() {
		if (!Stream.CanRead)
			throw new NotSupportedException("Content stream is not readable.");
	}
}
