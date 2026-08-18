#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading

#set page(
	margin: (x: 2.4cm, y: 2.2cm),
)
#set text(size: 10.5pt)

#align(center)[
	#text(size: 24pt, weight: "bold")[Tsinswreng.CsSh]
	#v(0.4em)
	跨平台、AOT 兼容的 C# Shell 基础设施库
]

#H[定位][
	`Tsinswreng.CsSh` 為 C# 與 `.csx` 腳本提供接近 Bash 使用體驗的基礎 API，主要服務於構建、發布、同步與工具鏈腳本。

	它不重新實現 Bash，也不解析完整 Shell 語法。外部工具仍由 .NET `Process` 啟動；Cssh 負責提供一致的命令、流、文件系統、工作目錄與環境配置接口。

	主要特點：

	- 支持 Windows、Linux 與 macOS 的正斜槓路徑。
	- 支持 NativeAOT 兼容的普通 C# 程序與 `.csx` 腳本。
	- 命令參數使用 `IList<string>` 分隔，避免呼叫方手動引號與跳脫。
	- stdout、stderr、stdin 統一使用 `Content` 流載體。
	- 文件操作提供同步與以 `CancellationToken` 結尾的異步入口。
]

#H[安裝與引用][
	CsSh 是普通 .NET 程序集。建置腳本可以引用已建置的 DLL：

```cs
#r "path/to/Tsinswreng.CsSh.dll"

using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;
```

	本地源碼建置：

```sh
dotnet build proj/Tsinswreng.CsSh/Tsinswreng.CsSh.csproj
```

	腳本通常使用 `ShGlobal` 的靜態 facade；需要獨立工作目錄、標準流或命令狀態時，直接建立 `Sh` 實例：

```cs
var Sh = new Tsinswreng.CsSh.Sh();
Sh.Cd("build");
await Sh.Exe("dotnet", ["--version"], Ct);
```
]

#H[工作目錄與路徑][
	`Sh` 實例保存自己的工作目錄。`Cd` 只改變該實例後續相對路徑和命令的工作目錄，不改變進程的全局當前目錄。

```cs
using static Tsinswreng.CsSh.ShGlobal;

var Ct = CancellationToken.None;
var Root = CsxDir();
Cd(Root / "artifacts");
await Mkdir("logs", Ct);
await Write("logs/version.txt", "created by Cssh\n", Ct);
```

	字符串的 `/` 操作符用於路徑拼接，會統一輸出正斜槓：

```cs
var Project = Root / "src" / "MyProject";
```

	`Path` 是 Cssh 的路徑值類型，與 `string` 隱式互轉；它承擔路徑拼接與未來路徑值成員，不把這些成員污染到所有 `string`。通用 BCL 操作仍直接使用 `System.IO.Path`。

	Bash 風格的路徑函數掛在 `Sh`，因此可直接配合 `using static ShGlobal` 使用：

```cs
BaseName("src/app/config.json"); // config.json
DirName("src/app/config.json");  // src/app
RealPath("src/../README.typ");   // 依目前 Cd 展開的絕對路徑
```

	`RealPath` 對應 `System.IO.Path.GetFullPath`：它不要求路徑存在，也不解析符號連結。
]

#H[外部命令][
	命令 API 強制分離可執行檔與參數。參數列表中的每一項都是一個完整參數，不需要手動加引號或處理空格、引號與反斜槓。

```cs
var Exit = await Exe(
	"dotnet",
	["build", "My Project/MyProject.csproj", "-c", "Release"],
	Ct);
```

	可用入口：

	- `Cmd`：建立延遲命令，不會因建立對象而立即啟動。
	- `TryCmd`：建立不因非零退出碼拋異常的延遲命令。
	- `Exe`：立即消費命令輸出到 `Sh.Stdout` 與 `Sh.Stderr`，成功時返回 `CommandExit`。
	- `TryExe`：與 `Exe` 相同，但非零退出碼返回 `CommandExit`。

	命令對象的常用消費方式：

```cs
await using var Command = Cmd("git", ["status", "--short"], Ct);
var Exit = await Command.Done;
var Text = await Command.Result.Stdout.Text(Ct);
```

	有限文本輸出應使用 `Text` 一次取得兩條輸出和退出結果，避免手動建立多個讀取任務：

```cs
await using var Command = TryCmd("typst", ["eval", "document.typ"], Ct);
var Result = await Command.Text(Ct);

Console.WriteLine(Result.Stdout);
Console.Error.WriteLine(Result.Stderr);
if (!Result.Exit.IsSuccess) {
	throw new Exception($"exit code: {Result.Exit.ExitCode}");
}
```

	Cssh 不提供完整命令字符串重載，因此不解析 `|`、`>`、`&&`、Shell 變量展開等語法。`IList<string>` 只解決參數邊界，不等價於 Shell。

	`Q(string)` 保留作為命令字符串或其他外部格式需要的安全參數引用工具；正常命令調用優先使用參數列表。
]

#H[輸入、輸出與管道][
	`CommandOptions.Input` 將一個 `Content` 作為子命令 stdin。命令輸出的 `Result.Stdout` 和 `Result.Stderr` 也是 `Content`，因此可直接連接到下一個命令：

```cs
await using var Log = Cmd("git", ["log", "--oneline"], Ct);
await using var Hash = Cmd(
	"git",
	["hash-object", "--stdin"],
	new CommandOptions(Input: Log.Result.Stdout),
	Ct);

var HashResult = await Hash.Text(Ct);
```

	文件重定向使用 `Out` 或 `Write`：

```cs
await using var Build = Cmd("dotnet", ["build"], Ct);
await Build.Out("build.log", Ct);
```

	stdout 與 stderr 分別指定目標：

```cs
await using var Command = Cmd("dotnet", ["build"], Ct);
await Command.Out(Stdout, Stderr, Ct);
```

	這裡分別寫入當前 Shell 的標準輸出與標準錯誤；如果只需要一個合併文件，直接使用 `Out("build.log", Ct)` 即可。

	大量輸出應使用流式 `Result.Stdout`／`Result.Stderr`，不要先全部轉成字符串。`Text` 適合 JSON、版本號與錯誤摘要等有限輸出。
]

#H[Content][
	`Content` 是 Cssh 的統一資料載體，包裝一條 .NET `Stream`，並支持文字與普通 Stream 的隱式轉換：

```cs
Content FromText = "hello";
Content FromStream = SomeStream;

string Text = FromText;
Stream Stream = FromStream;
```

	`Text()` 與 `Text(Ct)` 第一次調用時消費底層流並緩存結果；後續文本轉換會重用同一結果。直接操作 `Content.Stream` 仍然是流式、一次性消費，不能在任意外部讀取後自動恢復內容。

	`ContentOptions.LeaveOpen` 控制釋放 `Content` 時是否關閉呼叫方提供的底層流。
]

#H[文件系統][
	文件 API 使用 .NET 文件系統，路徑可直接寫正斜槓：

```cs
await Mkdir("input/nested", Ct);
await Write("input/message.txt", "hello\n", Ct);
await Cp("input/message.txt", "input/copy.txt", Ct);
await Mv("input/copy.txt", "input/moved.txt", Ct);
await Rm("input/moved.txt", Ct);
```

	`Rm` 固定採用 `rm -rf` 語義：目標不存在時忽略，不交互確認；真實權限或文件系統錯誤仍會拋出。

	`Cp` 與 `Mv` 遵循常見 Bash 目的地規則。當目的地是已存在目錄時，源名稱會追加到目的地：

```cs
await Cp("source.txt", "output", Ct); // output/source.txt
await Mv("source-dir", "output", Ct);  // output/source-dir/...
```

	覆寫行為放在選項 DTO：

```cs
await Cp("source.txt", "output", new CpOptions(Overwrite: true), Ct);
```

	`Glob` 是「遞歸枚舉 + glob 過濾」API，不是 Bash `find` 條件表達式。支持 `*`、`?` 和 `**`：

```cs
await foreach (var Entry in Glob("src/**/*.csproj", Ct)) {
	Console.WriteLine(Entry.FullName);
}
```

	返回值是 .NET `FileSystemInfo`；文件可轉為 `FileInfo`，目錄可轉為 `DirectoryInfo`，屬性、時間和大小直接使用 BCL 成員。

	單一路徑的類型與屬性查詢使用 `FsInfo`；不存在時返回 `null`：

```cs
var Entry = FsInfo(Path);
if (Entry is FileInfo File) {
	Console.WriteLine(File.Length);
}
if (Entry is DirectoryInfo) {
	Console.WriteLine("directory");
}
```

	最常用的檔案與目錄分支使用 `IsFile`、`IsDir`；二者在路徑不存在時都返回 `false`：

```cs
if (IsFile("settings.json")) {
	// Bash: if -f settings.json
}
if (IsDir("ExternalRsrc")) {
	// Bash: if -d ExternalRsrc
}
```
]

#H[環境變量][
	`GetEnv`、`SetEnv`、`UnsetEnv` 操作當前腳本進程的環境變量：

```cs
SetEnv("CSSH_MODE", "build");
var Mode = GetEnv("CSSH_MODE");
UnsetEnv("CSSH_MODE");
```

	單個子命令的環境配置應放入 `CommandOptions.Env`，不修改宿主進程，也不影響其他命令：

```cs
var Options = new CommandOptions(
	Env: new Dictionary<string, string?> {
		["MODE"] = "release",
		["REMOVE_ME"] = null,
	});

await Exe("tool", [], Options, Ct);
```

	子命令先繼承父進程環境，再套用 `Env`：非空值覆蓋，`null` 移除。環境在建立 `Cmd`／呼叫 `Exe` 時快照。
]

#H[同步與異步][
	最後一個參數為 `CancellationToken Ct` 的入口是異步 API；沒有 `Ct` 的同名入口是同步 API。

```cs
// 異步
await Write("message.txt", "hello", Ct);
await Exe("dotnet", ["--version"], Ct);

// 同步
Write("message.txt", "hello");
Exe("dotnet", ["--version"]);
```

	異步 API 支持取消。文件枚舉與命令流應優先使用異步入口；`IAsyncEnumerable` 和 `Stream` 都按惰性、流式方式消費。
]

#H[測試與開發][
	CsSh 使用 `Tsinswreng.CsTreeTest` 測試。測試資料應建立在測試程序集輸出目錄附近，測試結束後清理。

```sh
dotnet build proj/Tsinswreng.CsSh.Test/Tsinswreng.CsSh.Test.csproj --no-restore
dotnet run --project proj/Tsinswreng.CsSh.Test/Tsinswreng.CsSh.Test.csproj --no-build
```

	測試應按聲明與實現分離：測試聲明放在 `TestXxx.cs`，實現與註冊放在 `TestXxx.Impl.cs`。
]

#H[設計邊界][
	Cssh 有意不提供以下 Shell 語法的隱式解析：

	- 管道符 `|`。
	- 重定向符 `>`、`>>`、`<`。
	- `&&`、`||` 與命令替換。
	- Shell glob 自動展開到外部命令參數。
	- Shell 變量展開與引號解析。

	這些能力分別由 `Content` 管道、`Out`／`Write` 重定向、C# 控制流、`Glob` 文件枚舉、`CommandOptions.Env` 與參數列表取代。這樣腳本使用的是可檢查、可組合的 C# API，而不是另一套未完整實現的 Shell 解析器。
]
