// CsSh 完整 API 樣本。
// 以 dotnet-script 執行時，引用已發布的 Tsinswreng.CsSh 程序集：
// #r "path/to/Tsinswreng.CsSh.dll"

using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

using var CtSource = new CancellationTokenSource();
var Ct = CtSource.Token;
var StartDir = Pwd();
Pth Root = CsxDir();
var SampleDir = Root / "artifacts" / "cssh-sample";

// 每個範例均在樣本目錄內操作，避免修改腳本以外的檔案。
await Rm(SampleDir, Ct);
await Mkdir(SampleDir, Ct);
Cd(SampleDir);

// Pwd、CsxDir 與 Echo 是 CsSh 提供的腳本基本環境 API。
// Args 是 dotnet-script 注入的全域腳本參數，不由 CsSh 包裝。
await Echo("pwd: " + Pwd(), Ct);
await Echo("script dir: " + CsxDir(), Ct);
await Echo("argument count: " + Args.Count, Ct);

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

// Ls 直接回傳完整 Pth；需要種類時明確呼叫 IsFile、IsDir，不配置 FileSystemInfo。
foreach (var Item in Ls("input"))
	await Echo($"ls: {Item}; file={await IsFile(Item, Ct)}; dir={await IsDir(Item, Ct)}", Ct);

// FsInfo 回傳 BCL 的 FileInfo 或 DirectoryInfo；IsFile 與 IsDir 則對應 Bash 的 -f、-d 常用判斷。
var MessageInfo = await FsInfo("input/message.txt", Ct);
await Echo("info: file=" + (MessageInfo is FileInfo) + "; dir=" + (MessageInfo is DirectoryInfo), Ct);
await Echo("is file=" + await IsFile("input/message.txt", Ct) + "; is dir=" + await IsDir("input", Ct), Ct);

// BaseName、DirName、RealPath 都依目前 Shell 的 Cwd 工作；Pth 可直接和 string 隱式互轉。
var MessagePath = (Pth)"input/message.txt";
await Echo("base=" + BaseName(MessagePath) + "; dir=" + DirName(MessagePath), Ct);
await Echo("absolute=" + RealPath(MessagePath), Ct);

// Glob 接收第三方庫支援的 glob 路徑，並以 IEnumerable 惰性輸出結果。
foreach (var Item in Glob("input/**/*.txt"))
	await Echo("find: " + Item, Ct);

// Exe 是一般命令入口：立即執行並把兩條輸出流寫回終端。
await Exe("dotnet", ["--version"], Ct);

// Q 只用於另一個程式要自行解析命令字串時；CsSh 本身的 Exe/Cmd 不需要它。
var QuotedPath = Q("a path with spaces");
await Echo("quoted external-command fragment: " + QuotedPath, Ct);

// TryExe 對非零退出碼不丟例外，仍非同步取得結構化退出結果。
var GitProbe = await TryExe("git", ["rev-parse", "--is-inside-work-tree"], Ct);
await Echo("git probe success: " + GitProbe.IsSuccess, Ct);

// TryCmd 適合要讀取失敗輸出後自行決定如何處理的場景。
await using (var Probe = TryCmd("dotnet", ["--definitely-invalid-option"], Ct)) {
	var ProbeResult = await Probe.Text(Ct);
	await Echo("try cmd success: " + ProbeResult.Exit.IsSuccess, Ct);
	await Echo("try cmd stderr: " + ProbeResult.Stderr.Trim(), Ct);
}

// Write 對應 >、Append 對應 >>；命令結果本身就是 Content，可直接成為來源。
await using (var Status = TryCmd("git", ["status", "--short"], Ct)) {
	await Write("git-status.log", Status.Result.Stdout, Ct);
	await Append("git-status.log", Status.Result.Stderr, Ct);
	await Status.Done;
}

await using (var History = TryCmd("git", ["log", "-1", "--oneline"], Ct)) {
	await Write("history.log", History.Result.Stdout, Ct);
	await Write(Stderr, History.Result.Stderr, Ct);
	await History.Done;
}

// Read 回傳 Content，既可直接作 CommandOptions.Input，也可隱式取出普通 Stream。
await using (Content Input = await Read("input/message.txt", Ct)) {
	await using var Hash = Cmd("git", ["hash-object", "--stdin"], new CommandOptions(Input), Ct);
	await Hash.Out(Ct);
}

// 命令管道不解析 |：下游命令的 Input 直接指向上游 Command 的 stdout Content。
await using var Log = Cmd("git", ["log", "--oneline"], Ct);
await using var LogHash = Cmd("git", ["hash-object", "--stdin"], new(Log.Result.Stdout), Ct);
await Task.WhenAll(
	Write(Stdout, LogHash.Result.Stdout, Ct),
	Write(Stderr, Log.Result.Stderr, Ct),
	Write(Stderr, LogHash.Result.Stderr, Ct),
	Log.Done,
	LogHash.Done);

// Null 是跨平台 /dev/null / NUL；此處只丟棄 stderr，stdout 仍顯示在終端。
await using (var Version = Cmd("dotnet", ["--info"], Ct)) {
	await Version.Out(Stdout, Null, Ct);
}

Cd(StartDir);
await Echo("CsSh complete sample finished.", Ct);

