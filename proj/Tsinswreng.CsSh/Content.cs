using System.Text;

namespace Tsinswreng.CsSh;

/// CsSh 的統一資料載體。
/// 一個 Content 包裝一條可讀或可寫的資料流；它可由文字或普通 Stream 隱式建立，
/// 因此檔案、命令與標準輸入輸出 API 都只需要處理 Content。
/// Content 是一次性資料：轉成 string、讀取其 Stream，或作為 Write 的來源都會推進底層位置。
public sealed partial class Content:IDisposable,IAsyncDisposable{
	/// 此 Content 所包裝的普通 .NET Stream。
	/// 取得它不複製資料，也不重設目前讀寫位置。
	public Stream Stream{get;}

	/// 建立時的文字編碼與底層 Stream 所有權設定。
	public ContentOptions Options{get;}

	/// First text consumption is shared by synchronous and asynchronous callers.
	private Task<str>? TextTask;

	/// 從普通 .NET Stream 建立 Content。
	public partial Content(Stream Stream, ContentOptions? Options = null);

	/// 將文字編碼為 Content；預設 UTF-8。
	public static implicit operator Content(str Text) {
		ArgumentNullException.ThrowIfNull(Text);
		var Bytes = Encoding.UTF8.GetBytes(Text);
		return new(new MemoryStream(Bytes, writable: false), new(LeaveOpen: false));
	}

	/// 將普通 .NET Stream 包裝為 Content，不預先讀取或複製資料。
	public static implicit operator Content(Stream Stream) {
		return new(Stream);
	}

	/// 同步讀完 Content 並按設定編碼解碼為文字。
	/// 這會消費 Content；需要非同步讀取時使用 Text(Ct)。
	public static implicit operator str(Content Value) {
		ArgumentNullException.ThrowIfNull(Value);
		return Value.Text();
	}

	/// 取出 Content 的底層普通 Stream，不複製資料。
	public static implicit operator Stream(Content Value) {
		ArgumentNullException.ThrowIfNull(Value);
		return Value.Stream;
	}

	/// 同步讀完並解碼為文字。
	public partial str Text();

	/// 非同步讀完並解碼為文字；Ct 必須作為最後一個位置參數傳入。
	public partial Task<str> Text(CT Ct);

	/// 釋放 Content 擁有的底層資源；不擁有的外部 Stream 不會被關閉。
	public partial void Dispose();

	/// 非同步釋放 Content 擁有的底層資源；不擁有的外部 Stream 不會被關閉。
	public partial ValueTask DisposeAsync();
}

/// Content 的建立選項。
/// Encoding 控制 string 與 Content 的互轉；LeaveOpen 為 true 時 Content 釋放後保留呼叫方提供的 Stream。
public sealed record ContentOptions(
	Encoding? Encoding = null,
	bool LeaveOpen = true);
