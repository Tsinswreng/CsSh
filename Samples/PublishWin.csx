// Cssh 目標 API 展示稿：只依賴 Cssh 的宣告層，尚無實現，故暫不能執行。
// dotnet-script 使用時預期引用已發布的 Tsinswreng.Cssh 程序集：
// #r "path/to/Tsinswreng.Cssh.dll"

using static Tsinswreng.Cssh.Sh;

using var CtSource = new CancellationTokenSource();
var Ct = CtSource.Token;
var Root = Pwd();

// 以終端作為預設的 stdout/stderr 接收端。
// 命令本身仍是惰性的；只有三個非同步工作開始消費其結果後才會啟動。
async Task XTerm(string Text, string? Cwd = null){
	await using var Command = X(Text, Cwd: Cwd, Ct: Ct);
	await Task.WhenAll(
		Write(Stdout, Command.Result.Stdout, Ct),
		Write(Stderr, Command.Result.Stderr, Ct),
		Command.Done);
}

// 先保留上次發布結果；第一次執行時 publish 不存在，直接略過即可。
Cd("Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows");
await Rm("publishOld", Force: true, Ct);
if (await Exists("publish", Ct))
	await Mv("publish", "publishOld", Ct: Ct);

await XTerm(
	"dotnet publish -c Release -r win-x64 " +
	"-p:AllowMissingPrunePackageData=true");

// 此命令只在根目錄執行，不需要為切換目錄建立或恢復 scope。
await XTerm("sh ./CpAssets.sh", Root);

var ReleaseDir = "bin/Release/net10.0/win-x64";
var PublishDir = "bin/Release/net10.0/win-x64/publish";
var PublishNoPdbDir = "bin/Release/net10.0/win-x64/publishNoPdb";

// 生成一份可分發副本，再刪除符號檔。
await Rm(PublishNoPdbDir, Force: true, Ct);
await Cp(PublishDir, PublishNoPdbDir, Ct: Ct);
await foreach (var Pdb in Find(PublishNoPdbDir + "/**/*.pdb", Ct))
	await Rm(Pdb.Path, Force: true, Ct);

// 壓縮檔暫存在來源目錄外，以免 tar 在掃描來源時把自身打進去。
var ArchivePath = Root + "/Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows/bin/Release/net10.0/win-x64/Ngan.Dict.Windows.tar.gz";
await Rm(ArchivePath, Force: true, Ct);
await XTerm($"tar -czf \"{ArchivePath}\" .", PublishNoPdbDir);
await Mv(ArchivePath, "bin/Release/net10.0/win-x64/publishNoPdb/Ngan.Dict.Windows.tar.gz", Ct: Ct);

await Echo("Windows publish completed.", Ct);
