// CsSh 完整 API 樣本。
// 以 dotnet-script 執行時，引用已發布的 Tsinswreng.CsSh 程序集：
// #r "path/to/Tsinswreng.CsSh.dll"

using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

using var CtSource = new CancellationTokenSource();
var Ct = CtSource.Token;
var StartDir = Pwd();
var Root = ScriptDir();
var SampleDir = Root / "artifacts" / "cssh-sample";

// 每個範例均在樣本目錄內操作，避免修改腳本以外的檔案。
await Rm(SampleDir, Ct);
await Mkdir(SampleDir, Ct);
Cd(SampleDir);

// Pwd、ScriptDir、Args 與 Echo 是腳本的基本環境 API。
await Echo("pwd: " + Pwd(), Ct);
await Echo("script dir: " + ScriptDir(), Ct);
await Echo("argument count: " + Args().Count, Ct);

// 環境變數由當前腳本進程持有；之後才啟動的 X 命令也會繼承它。
SetEnv("CSSH_SAMPLE", "1");
await Echo("CSSH_SAMPLE=" + GetEnv("CSSH_SAMPLE"), Ct);
UnsetEnv("CSSH_SAMPLE");

// Mkdir、Write、Read、Cp、Mv、Rm 均為非同步檔案操作。
await Mkdir("input", Ct);
await Write("input/message.txt", "CsSh Content input\n", Ct);
await using (Content Message = await Read("input/message.txt", Ct)) {
	await Echo("read: " + (await Message.Text(Ct)).Trim(), Ct);
}
await Cp("input/message.txt", "input/copy.txt", Ct);
await Mv("input/copy.txt", "input/moved.txt", Ct);
await Rm("input/moved.txt", Ct);

// Ls 直接回傳 .NET FileSystemInfo；以 FileInfo/DirectoryInfo 型別與 Attributes 判斷項目性質。
await foreach (var Item in Ls("input", Ct))
	await Echo($"ls: {Item.Name}; file={Item is FileInfo}; dir={Item is DirectoryInfo}; link={Item.Attributes.HasFlag(FileAttributes.ReparsePoint)}; modified={Item.LastWriteTimeUtc:O}", Ct);

// Find 接收 Bash 風格的 glob 路徑，並以 IAsyncEnumerable 惰性輸出結果。
await foreach (var Item in Find("input/**/*.txt", Ct))
	await Echo("find: " + Item.FullName.Replace('\\', '/'), Ct);

// X 只建立惰性 Command DTO；Out 開始消費 Result 的兩條 Stream 後才啟動命令。
await using (var Version = X("dotnet --version", Ct))
	await Version.Out(Ct);

// TryX 對非零退出碼不丟例外，仍透過 Done 非同步取得結構化退出結果。
await using var GitProbeCommand = TryX("git rev-parse --is-inside-work-tree", Ct);
var GitProbe = await GitProbeCommand.Out(Ct);
await Echo("git probe success: " + GitProbe.IsSuccess, Ct);

// Write 對應 >、Append 對應 >>；命令結果本身就是 Content，可直接成為來源。
await using (var Status = TryX("git status --short", Ct)) {
	await Write("git-status.log", Status.Result.Stdout, Ct);
	await Append("git-status.log", Status.Result.Stderr, Ct);
	await Status.Done;
}

await using (var History = TryX("git log -1 --oneline", Ct)) {
	await Write("history.log", History.Result.Stdout, Ct);
	await Write(Stderr, History.Result.Stderr, Ct);
	await History.Done;
}

// Read 回傳 Content，既可直接作 CommandOptions.Input，也可隱式取出普通 Stream。
await using (Content Input = await Read("input/message.txt", Ct)) {
	await using var Hash = X("git hash-object --stdin", new(Input), Ct);
	await Hash.Out(Ct);
}

// 命令管道不解析 |：下游命令的 Input 直接指向上游 Command 的 stdout Content。
await using var Log = X("git log --oneline", Ct);
await using var LogHash = X("git hash-object --stdin", new(Log.Result.Stdout), Ct);
await Task.WhenAll(
	Write(Stdout, LogHash.Result.Stdout, Ct),
	Write(Stderr, Log.Result.Stderr, Ct),
	Write(Stderr, LogHash.Result.Stderr, Ct),
	Log.Done,
	LogHash.Done);

// Null 是跨平台 /dev/null / NUL；此處只丟棄 stderr，stdout 仍顯示在終端。
await using (var Version = X("dotnet --info", Ct)) {
	await Version.Out(Stdout, Null, Ct);
}

Cd(StartDir);
await Echo("CsSh complete sample finished.", Ct);

