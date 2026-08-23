namespace Tsinswreng.CsSh.Scripts;

/// 發布並執行 CsSh 測試專案的 NativeAOT 產物。
internal static partial class TestAot{
	/// 使用 AOT 發布測試專案後執行產物；失敗時以非零退出碼結束。
	internal static partial Task Main(str Root, CT Ct);
}
