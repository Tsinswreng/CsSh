## 定位

`Tsinswreng.CsSh` 為 C# 與 `.csx` 腳本提供接近 Bash 使用體驗的基礎 API，主要服務於構建、發布、同步與工具鏈腳本。

它不重新實現 Bash，也不解析完整 Shell 語法。外部工具仍由 .NET `Process` 啟動；Cssh 負責提供一致的命令、流、文件系統、工作目錄與環境配置接口。

主要特點：

- 支持 Windows、Linux 與 macOS 的正斜槓路徑。
- 支持 NativeAOT 兼容的普通 C# 程序與 `.csx` 腳本。
- 命令參數使用 `IList<string>` 分隔，避免呼叫方手動引號與跳脫。
- stdout、stderr、stdin 統一使用 `Content` 流載體。
- 文件操作提供同步與以 `CancellationToken` 結尾的異步入口。

## 安裝與引用

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

## 工作目錄與路徑

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

`Pth` 是 Cssh 的路徑值類型，與 `string` 隱式互轉；它承擔路徑拼接與未來路徑值成員，不把這些成員污染到所有 `string`。通用 BCL 操作仍直接使用 `System.IO.Path`。

Bash 風格的路徑函數掛在 `Sh`，因此可直接配合 `using static ShGlobal` 使用：

```cs
BaseName("src/app/config.json"); // config.json
DirName("src/app/config.json");  // src/app
RealPath("src/../README.typ");   // 依目前 Cd 展開的絕對路徑
```

`ShGlobal.RealPath` 是預設 Shell 的靜態便利名；持有 `Sh` 實例時，對應成員名為 `Sh.FullPath`：

```cs
var Sh = new Tsinswreng.CsSh.Sh();
Sh.Cd("build");
var Absolute = Sh.FullPath("../README.typ");
```

二者都對應 `System.IO.Path.GetFullPath`：不要求路徑存在，也不解析符號連結。

## 外部命令

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

`Q(string)` 保留給「把一個值嵌入另一個程式所接收的命令字符串」的場景，例如傳給 `cmd.exe /c` 或其他自行解析命令列的外部工具。它不是 Cssh 命令呼叫的必需步驟；正常 `Cmd`／`Exe`／`TryCmd`／`TryExe` 一律優先使用參數列表。

## 輸入、輸出與管道

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

## Content

`Content` 是 Cssh 的統一資料載體，包裝一條 .NET `Stream`，並支持文字與普通 Stream 的隱式轉換：

```cs
Content FromText = "hello";
Content FromStream = SomeStream;

string Text = FromText;
Stream Stream = FromStream;
```

`Text()` 與 `Text(Ct)` 第一次調用時消費底層流並緩存結果；後續文本轉換會重用同一結果。直接操作 `Content.Stream` 仍然是流式、一次性消費，不能在任意外部讀取後自動恢復內容。

`ContentOptions.LeaveOpen` 控制釋放 `Content` 時是否關閉呼叫方提供的底層流。

## 文件系統

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

`Glob` 直接使用 Meziantou.Framework.Globbing 的 Standard dialect，惰性返回完整 `Pth` 路徑，不建立 `FileSystemInfo`。相對模式按目前 `Sh` 的工作目錄解析；支持 `*`、`?`、`[]`、`{}` 和 `**`。不以 `/` 結尾的模式只匹配檔案，以 `/` 結尾的模式只匹配目錄；字面量目錄模式也會回傳該目錄本身：

```cs
foreach (var Entry in Glob("src/**/*.csproj")) {
	Console.WriteLine(Entry);
}

foreach (var Directory in Glob("src/*/")) {
	Console.WriteLine(Directory);
}

foreach (var Assets in Glob("src/assets/")) {
	Console.WriteLine(Assets); // src/assets
}
```

`Ls` 以相同的 `IEnumerable<Pth>` 方式列出檔案與目錄；`LsDir` 和 `LsFile` 分別只列出目錄或檔案。三者直接轉發 .NET `Directory.Enumerate*`，不預先讀取大小、時間等 metadata。`LsOptions.Recursive` 控制是否遞迴：

```cs
foreach (var Entry in Ls("artifacts")) {
	Console.WriteLine(Entry);
}

foreach (var Directory in LsDir("src", new LsOptions(Recursive: true))) {
	Console.WriteLine(Directory);
}

foreach (var File in LsFile("src")) {
	Console.WriteLine(File);
}
```

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

## 環境變量

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

## 同步與異步

最後一個參數為 `CancellationToken Ct` 的入口是異步 API；沒有 `Ct` 的同名入口是同步 API。

```cs
// 異步
await Write("message.txt", "hello", Ct);
await Exe("dotnet", ["--version"], Ct);

// 同步
Write("message.txt", "hello");
Exe("dotnet", ["--version"]);
```

異步 API 支持取消。命令流以 `Stream` 惰性、流式方式消費；`Glob`、`Ls`、`LsDir` 與 `LsFile` 則是底層 .NET／Glob 函式庫提供的同步 `IEnumerable<Pth>` 列舉，沒有假的非同步包裝。

## 測試與開發

CsSh 使用 `Tsinswreng.CsTreeTest` 測試。測試資料應建立在測試程序集輸出目錄附近，測試結束後清理。

```sh
dotnet build proj/Tsinswreng.CsSh.Test/Tsinswreng.CsSh.Test.csproj --no-restore
dotnet run --project proj/Tsinswreng.CsSh.Test/Tsinswreng.CsSh.Test.csproj --no-build
```

測試應按聲明與實現分離：測試聲明放在 `TestXxx.cs`，實現與註冊放在 `TestXxx.Impl.cs`。

## CI/CD、NuGet 發布與版本升級

本專案使用 GitHub Actions 執行 CI/CD。定義 `CI` 爲每次提交後自動建置、測試與打包的驗證流程；定義 `CD` 爲在驗證成功後，把指定版本發布至 nuget.org 的流程。

目前套件的預設版本爲 `0.1.0-alpha`，套件 ID 爲 `Tsinswreng.CsSh`，授權爲 MIT。NuGet 套件同時產生 `.nupkg` 與供除錯使用的 `.snupkg` 符號套件。

### 工作流與觸發條件

專案有兩個工作流：

- `.github/workflows/verify.yml` 是驗證工作流。推送至 `master`、對 `master` 建立 Pull Request，或在 GitHub Actions 手動執行時觸發。它會還原依賴、以 Release 組態建置、執行測試，並以 `0.1.0-alpha.ci.<執行編號>` 打包驗證。驗證產物會作為 Actions artifact 上傳，但不會發布至 nuget.org。
- `.github/workflows/publish.yml` 是發布工作流。推送符合 `v*` 的 Git tag 時觸發；也可以從 GitHub Actions 手動執行並輸入版本。tag 觸發時，tag 名稱必須是 `v` 加上版本號，例如 `v0.1.0-alpha`；工作流會移除開頭的 `v`，得到 NuGet 套件版本 `0.1.0-alpha`。

發布工作流的步驟固定如下：

1. 還原依賴、Release 建置、執行全部測試。
2. 以 tag 中的版本覆蓋打包時的 `PackageVersion`，產生 NuGet 與符號套件。
3. 使用 GitHub environment 中的 `NUGET_API_KEY` 推送 `.nupkg` 至 nuget.org。
4. tag 觸發時，推送成功後建立同名的 GitHub Release，並自動產生 Release notes。

手動執行發布工作流不會建立 GitHub Release；它適合在已有 tag 或需要重試 NuGet 推送時使用。正常發版應使用 tag。

### 首次設定

發布者需要在 GitHub repository 的 Settings -\> Environments 新增名為 `nuget` 的 environment，再在該 environment 的 Secrets 新增 `NUGET_API_KEY`。

`NUGET_API_KEY` 是 nuget.org 產生的推送金鑰。應限制它僅能推送套件 ID `Tsinswreng.CsSh`，並且只保存於 GitHub Secret；不得寫入專案檔、工作流檔、Git commit 或本機設定檔。

若為 environment 設定 required reviewers，發布工作流會在推送前等待核准；若未設定，工作流會直接使用該 environment 的 Secret 執行。

### 本地發布前驗證

先在準備發布的 commit 上執行下列命令。它們與 CI/CD 使用相同的 Release 建置、測試與打包順序，但不會推送任何套件：

```sh
dotnet restore proj/Tsinswreng.CsSh.Test/Tsinswreng.CsSh.Test.csproj --ignore-failed-sources --no-cache
dotnet build proj/Tsinswreng.CsSh.Test/Tsinswreng.CsSh.Test.csproj --configuration Release --no-restore
dotnet run --project proj/Tsinswreng.CsSh.Test/Tsinswreng.CsSh.Test.csproj --configuration Release --no-build --no-restore
dotnet pack proj/Tsinswreng.CsSh/Tsinswreng.CsSh.csproj --configuration Release --no-build --no-restore --output artifacts/nuget-validation -p:PackageVersion=0.1.0-alpha
```

`--ignore-failed-sources` 只適合本機已有依賴快取、但暫時無法連線 NuGet 來源時使用。若本機沒有快取，應移除該參數並先排除網路問題。`artifacts/` 已被 Git 忽略。

### 發布首版

先提交準備發布的原始碼、README、套件設定與工作流。確認 `master` 上的 CI 已通過後，在同一個 commit 建立並推送 tag：

```sh
git switch master
git pull --ff-only origin master
git tag v0.1.0-alpha
git push origin v0.1.0-alpha
```

推送 tag 後，在 GitHub Actions 查看 `Publish NuGet package`。它成功完成才代表 NuGet 套件已發布；GitHub tag 已推送不代表套件必然已上架。

### 版本升級規則

本專案以 `MAJOR.MINOR.PATCH` 表示穩定版版本號：`MAJOR` 表示不相容的公開 API 變更，`MINOR` 表示向後相容的新功能，`PATCH` 表示向後相容的修正。`-alpha` 是預發版後綴，預發版使用者必須明確選取預發版本。

版本選擇與 tag 對照如下：

- 修正預發版問題：`0.1.0-alpha.1`，tag 為 `v0.1.0-alpha.1`。
- 在 0.x 階段加入相容功能：`0.2.0-alpha`，tag 為 `v0.2.0-alpha`。
- API 已穩定時的第一個正式版：`0.1.0`，tag 為 `v0.1.0`。
- 正式版的相容修正：`0.1.1`，tag 為 `v0.1.1`。
- 正式版的相容功能：`0.2.0`，tag 為 `v0.2.0`。
- 有破壞性變更：`1.0.0` 或後續的下一個主版本，tag 例如 `v1.0.0`。

`Tsinswreng.CsSh.csproj` 的 `<Version>` 是本地建置與未覆蓋打包的預設版本。tag 發布時，以 tag 解析出的版本為準。因此升級發布時必須同時更新 `<Version>` 與建立同版本 tag，避免本地建置顯示的版本和已發布版本不一致。

升級流程為：修改程式與文件 -\> 本地驗證 -\> 提交並推送 `master` -\> 確認 Verify 成功 -\> 更新 `<Version>` 至下一版並提交 -\> 再次確認 Verify 成功 -\> 建立並推送相同版本的 `v...` tag。

### 失敗處理與不可變版本

nuget.org 中已成功發布的相同版本不可覆蓋。若發布後發現程式、套件內容或 metadata 有問題，必須修正後發布一個更高版本；不可刪除 tag 後重新使用原版本。

發布工作流的 NuGet 推送使用 `--skip-duplicate`。因此「套件已推送成功，但建立 GitHub Release 失敗」時，可以重新執行工作流：既有套件會被略過，工作流會再次嘗試建立 Release。若 workflow 在測試或打包階段失敗，修正問題後應建立新的版本與 tag，不應把修正提交到既有發版 tag 所指向的 commit。

## 設計邊界

Cssh 有意不提供以下 Shell 語法的隱式解析：

- 管道符 `|`。
- 重定向符 `>`、`>>`、`<`。
- `&&`、`||` 與命令替換。
- Shell glob 自動展開到外部命令參數。
- Shell 變量展開與引號解析。

這些能力分別由 `Content` 管道、`Out`／`Write` 重定向、C# 控制流、`Glob` 文件枚舉、`CommandOptions.Env` 與參數列表取代。這樣腳本使用的是可檢查、可組合的 C# API，而不是另一套未完整實現的 Shell 解析器。
