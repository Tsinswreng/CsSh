// Cssh 完整 API 樣本。
// 以 dotnet-script 執行時，引用已發布的 Tsinswreng.Cssh 程序集：
// #r "path/to/Tsinswreng.Cssh.dll"

using static Tsinswreng.Cssh.Sh;

using var CtSource = new CancellationTokenSource();
var Ct = CtSource.Token;
var StartDir = Pwd();
var Root = ScriptDir();
var SampleDir = Root + "/artifacts/cssh-sample";

// 統一將命令的兩條結果流接回目前終端，並等待該命令的非同步退出結果。
async Task<CommandExit> XTerm(Command Command){
	await using var OwnedCommand = Command;
	await Task.WhenAll(
		Write(Stdout, Command.Result.Stdout, Ct),
		Write(Stderr, Command.Result.Stderr, Ct),
		Command.Done);
	return await Command.Done;
}

// 每個範例均在樣本目錄內操作，避免修改腳本以外的檔案。
await Rm(SampleDir, Force: true, Ct);
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
await Write("input/message.txt", "Cssh stream input\n", Ct: Ct);
var Message = await Read("input/message.txt", Ct: Ct);
await Echo("read: " + Message.Trim(), Ct);
await Cp("input/message.txt", "input/copy.txt", Ct: Ct);
await Mv("input/copy.txt", "input/moved.txt", Ct: Ct);
await Rm("input/moved.txt", Ct: Ct);

// Ls 保留檔案、目錄與連結的類型資訊。
await foreach (var Item in Ls("input", Ct: Ct))
	await Echo($"ls: {Item.Name}; file={Item.IsFile}; dir={Item.IsDir}; link={Item.IsLink}", Ct);

// Find 接收 Bash 風格的 glob 路徑，並以 IAsyncEnumerable 惰性輸出結果。
await foreach (var Item in Find("input/**/*.txt", Ct))
	await Echo("find: " + Item.Path, Ct);

// X 只建立惰性 Command DTO；XTerm 開始消費 Result 的兩條 Stream 後才啟動命令。
await XTerm(X("dotnet --version", Ct: Ct));

// TryX 對非零退出碼不丟例外，仍透過 Done 非同步取得結構化退出結果。
var GitProbe = await XTerm(TryX("git rev-parse --is-inside-work-tree", Ct: Ct));
await Echo("git probe success: " + GitProbe.IsSuccess, Ct);

// OpenWrite 對應 >：stdout 和 stderr 安全合併寫入同一檔案。
await using (var StatusLog = await OpenWrite("git-status.log", Ct)) {
	await using var Status = TryX("git status --short", Ct: Ct);
	await Task.WhenAll(
		Write(StatusLog, [Status.Result.Stdout, Status.Result.Stderr], Ct),
		Status.Done);
}

// OpenAppend 對應 >>：輸出附加到既有檔案。
await using (var HistoryLog = await OpenAppend("history.log", Ct)) {
	await using var History = TryX("git log -1 --oneline", Ct: Ct);
	await Task.WhenAll(
		Write(HistoryLog, History.Result.Stdout, Ct),
		Write(Stderr, History.Result.Stderr, Ct),
		History.Done);
}

// OpenRead 提供外部 stdin；git hash-object --stdin 讀取檔案 Stream 後產生 hash。
await using (var Input = await OpenRead("input/message.txt", Ct)) {
	await using var Hash = X("git hash-object --stdin", Input: Input, Ct: Ct);
	await Task.WhenAll(
		Write(Stdout, Hash.Result.Stdout, Ct),
		Write(Stderr, Hash.Result.Stderr, Ct),
		Hash.Done);
}

// 命令管道不解析 |：下游命令的 Input 直接指向上游 Command 的 stdout Stream。
await using var Log = X("git log --oneline", Ct: Ct);
await using var LogHash = X("git hash-object --stdin", Input: Log.Result.Stdout, Ct: Ct);
await Task.WhenAll(
	Write(Stdout, LogHash.Result.Stdout, Ct),
	Write(Stderr, Log.Result.Stderr, Ct),
	Write(Stderr, LogHash.Result.Stderr, Ct),
	Log.Done,
	LogHash.Done);

// Null 是跨平台 /dev/null / NUL；此處只丟棄 stderr，stdout 仍顯示在終端。
await using (var Version = X("dotnet --info", Ct: Ct)) {
	await Task.WhenAll(
		Write(Stdout, Version.Result.Stdout, Ct),
		Write(Null, Version.Result.Stderr, Ct),
		Version.Done);
}

Cd(StartDir);
await Echo("Cssh complete sample finished.", Ct);
