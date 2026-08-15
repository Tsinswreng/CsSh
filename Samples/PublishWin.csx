// Cssh 目標 API 展示稿：只依賴 Cssh 的宣告層，尚無實現，故暫不能執行。
// dotnet-script 使用時預期引用已發布的 Tsinswreng.Cssh 程序集：
// #r "path/to/Tsinswreng.Cssh.dll"

using static Tsinswreng.Cssh.Sh;

var Root = Pwd();

// 先保留上次發布結果；第一次執行時 publish 不存在，直接略過即可。
Cd("Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows");
Rm("publishOld", Force: true);
if (Exists("publish"))
	Mv("publish", "publishOld");

X(
	"dotnet publish -c Release -r win-x64 " +
	"-p:AllowMissingPrunePackageData=true");

// 此命令只在根目錄執行，不需要為切換目錄建立或恢復 scope。
X("sh ./CpAssets.sh", Cwd: Root);

var ReleaseDir = "bin/Release/net10.0/win-x64";
var PublishDir = "bin/Release/net10.0/win-x64/publish";
var PublishNoPdbDir = "bin/Release/net10.0/win-x64/publishNoPdb";

// 生成一份可分發副本，再刪除符號檔。
Rm(PublishNoPdbDir, Force: true);
Cp(PublishDir, PublishNoPdbDir);
foreach (var Pdb in Find("*.pdb", Under: PublishNoPdbDir))
	Rm(Pdb, Force: true);

// 壓縮檔暫存在來源目錄外，以免 tar 在掃描來源時把自身打進去。
var ArchivePath = Root + "/Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows/bin/Release/net10.0/win-x64/Ngan.Dict.Windows.tar.gz";
Rm(ArchivePath, Force: true);
X($"tar -czf {ArchivePath} .", Cwd: PublishNoPdbDir);
Mv(ArchivePath, "bin/Release/net10.0/win-x64/publishNoPdb/Ngan.Dict.Windows.tar.gz");

Console.WriteLine("Windows publish completed.");
