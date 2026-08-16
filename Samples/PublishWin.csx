// CsSh 版 Windows 發布腳本。
// dotnet-script 使用時引用已發布的 Tsinswreng.CsSh 程序集：
// #r "path/to/Tsinswreng.CsSh.dll"

using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

var Root = Pwd();

// 以終端作為預設的 stdout/stderr 接收端。
// 命令本身仍是惰性的；只有三個非同步工作開始消費其結果後才會啟動。
void XTerm(string Text, string? Cwd = null){
	using var Command = Cwd is null ? X(Text) : X(Text, new(Cwd: Cwd));
	Task.WhenAll(
		Task.Run(() => Write(Stdout, Command.Result.Stdout)),
		Task.Run(() => Write(Stderr, Command.Result.Stderr)),
		Command.Done).GetAwaiter().GetResult();
}

// 先保留上次發布結果；第一次執行時 publish 不存在，直接略過即可。
Cd("Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows");
Rm("publishOld");
if (Exists("publish"))
	Mv("publish", "publishOld");

XTerm(
	"dotnet publish -c Release -r win-x64 " +
	"-p:AllowMissingPrunePackageData=true");

// 此命令只在根目錄執行，不需要為切換目錄建立或恢復 scope。
XTerm("sh ./CpAssets.sh", Root);

var ReleaseDir = "bin/Release/net10.0/win-x64";
var PublishDir = "bin/Release/net10.0/win-x64/publish";
var PublishNoPdbDir = "bin/Release/net10.0/win-x64/publishNoPdb";

// 生成一份可分發副本，再刪除符號檔。
Rm(PublishNoPdbDir);
Cp(PublishDir, PublishNoPdbDir);
foreach (var Pdb in Find(PublishNoPdbDir / "**/*.pdb"))
	Rm(Pdb.Path);

// 壓縮檔暫存在來源目錄外，以免 tar 在掃描來源時把自身打進去。
var ArchivePath = Root / "Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows/bin/Release/net10.0/win-x64/Ngan.Dict.Windows.tar.gz";
Rm(ArchivePath);
XTerm($"tar -czf \"{ArchivePath}\" .", PublishNoPdbDir);
Mv(ArchivePath, "bin/Release/net10.0/win-x64/publishNoPdb/Ngan.Dict.Windows.tar.gz");

Echo("Windows publish completed.");

