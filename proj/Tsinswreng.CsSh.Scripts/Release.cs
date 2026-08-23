namespace Tsinswreng.CsSh.Scripts;

/// 執行 CsSh 的完整 CI/CD 流程。
/// 流程總是先驗證；只有在 GitHub tag 環境中才會繼續打包、推送與建立 Release。
internal static partial class Release{
	/// 執行唯一發布入口；版本由 GitHub tag 決定。
	internal static partial Task Main(str Root, CT Ct);

	/// 驗證依賴、Release 編譯與現有測試。
	private static partial Task<nil> Verify(str Root, CT Ct);

	/// 依發布版本建立 NuGet 套件並回傳產物路徑。
	private static partial Task<IReadOnlyList<Pth>> Pack(str Root, str Version, CT Ct);

	/// 將 NuGet 套件推送至 nuget.org。
	private static partial Task<nil> PushNuGet(IReadOnlyList<Pth> Packages, CT Ct);

	/// 建立或更新 GitHub Release，並把 NuGet 與符號產物附加上去。
	private static partial Task<nil> PublishGitHubRelease(str Tag, IReadOnlyList<Pth> Packages, CT Ct);

	/// 從 GitHub Actions 環境取得發布 tag；非 tag 執行時只做驗證。
	private static partial str? ResolveTag();

	/// 驗證 tag 格式並去掉 NuGet 不使用的 v 前綴。
	private static partial str ParseVersion(str Tag);
}
