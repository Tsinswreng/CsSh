// CsSh 版 Windows 發布腳本。
// dotnet-script 使用時引用已發布的 Tsinswreng.CsSh 程序集：
// #r "path/to/Tsinswreng.CsSh.dll"

using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

using var CtSource = new CancellationTokenSource();
var Ct = CtSource.Token;
var Root = System.IO.Path.GetFullPath(CsxDir() / "../..").Replace('\\', '/');
var ProjectDir = Root / "Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows";

// 先保留上次發布結果；所有路徑都從腳本位置推得，不依賴啟動 cwd。
var PublishDir = ProjectDir / "bin/Release/net10.0/win-x64/publish";
var PublishNoPdbDir = ProjectDir / "bin/Release/net10.0/win-x64/publishNoPdb";
var OldPublishDir = ProjectDir / "publishOld";
Cd(ProjectDir);
await Rm(OldPublishDir, Ct);
if (await Exists(PublishDir, Ct))
	await Mv(PublishDir, OldPublishDir, Ct);

await Exe("dotnet", ["publish", "-c", "Release", "-r", "win-x64", "-p:AllowMissingPrunePackageData=true"], Ct);
await Rm(OldPublishDir, Ct);

// 資源同步入口在 Ngan.Dict.Scripts 中；用 Cd 切到根目錄後直接呼叫，不建立 Cwd DTO。
Cd(Root);
await Exe("dotnet", ["run", "--project", Root / "Ngan.Dict/Ngan.Dict.Scripts/Ngan.Dict.Scripts.csproj", "--", "CpAssets"], Ct);
Cd(ProjectDir);

// 生成一份可分發副本，再刪除符號檔。
await Rm(PublishNoPdbDir, Ct);
await Cp(PublishDir / "*", PublishNoPdbDir, Ct);
foreach (var Pdb in Glob(PublishNoPdbDir / "**/*.pdb"))
	await Rm(Pdb, Ct);

// 壓縮檔暫存在來源目錄外，以免 tar 在掃描來源時把自身打進去。
var ArchivePath = ProjectDir / "bin/Release/net10.0/win-x64/Ngan.Dict.Windows.tar.gz";
await Rm(ArchivePath, Ct);
Cd(PublishNoPdbDir);
await Exe("tar", ["-czf", ArchivePath, "."], Ct);
await Mv(ArchivePath, PublishNoPdbDir / "Ngan.Dict.Windows.tar.gz", Ct);

await Echo("Windows publish completed.", Ct);

