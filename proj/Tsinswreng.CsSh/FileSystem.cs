namespace Tsinswreng.CsSh;

/// Cp 的可选行为。
public sealed record CpOptions(
	bool Overwrite = false);

/// Mv 的可选行为。
public sealed record MvOptions(
	bool Overwrite = false);

/// Ls、LsDir 與 LsFile 的可选行为。
/// Recursive 為 false 時只列出直接子項；為 true 時遞迴列出所有後代項目。
public sealed record LsOptions(
	bool Recursive = false);

/// 供 csx 构建脚本直接静态导入的文件系统动词。
/// 每个路径均由 .NET 负责平台适配，不依赖 Bash、PowerShell 或外部 coreutils。
/// 调用方可统一使用正斜杠，例如 Rm("artifacts/publish")；无需调用 Path.Combine。
public partial class Sh{
	/// 判断路径是否存在，可以是文件或目录。
	public partial bool Exists(Pth Path);

	/// 异步判断路径是否存在；Ct 必须作为最后一个位置参数传入。
	public partial Task<bool> Exists(Pth Path, CT Ct);

	/// 取得文件或目录的 .NET 文件系统信息；路径不存在时返回 null。
	/// 文件返回 FileInfo，目录返回 DirectoryInfo；其余属性直接使用 BCL 成员读取。
	public partial FileSystemInfo? FsInfo(Pth Path);

	/// 异步取得文件或目录的 .NET 文件系统信息；路径不存在时返回 null。
	public partial Task<FileSystemInfo?> FsInfo(Pth Path, CT Ct);

	/// 判断路径是否为普通文件；不存在时返回 false。
	/// 与 FsInfo 一样，遇到真实的文件系统访问错误会抛出异常。
	public partial bool IsFile(Pth Path);

	/// 异步判断路径是否为普通文件；不存在时返回 false。
	public partial Task<bool> IsFile(Pth Path, CT Ct);

	/// 判断路径是否为目录；不存在时返回 false。
	/// 与 FsInfo 一样，遇到真实的文件系统访问错误会抛出异常。
	public partial bool IsDir(Pth Path);

	/// 异步判断路径是否为目录；不存在时返回 false。
	public partial Task<bool> IsDir(Pth Path, CT Ct);

	/// 建立目录及所有缺失的父目录；已存在时不报错。
	public partial void Mkdir(Pth Path);

	/// 异步建立目录；Ct 必须作为最后一个位置参数传入。
	public partial Task<nil> Mkdir(Pth Path, CT Ct);

	/// 删除文件，或递归删除目录。
	/// 固定等价于 Bash 的 rm -rf：目标不存在时忽略，绝不交互确认；权限不足等真实文件系统错误仍会抛出。
	public partial void Rm(Pth Path);

	/// 异步删除文件或目录；语义与同步 Rm 相同，Ct 必须作为最后一个位置参数传入。
	public partial Task<nil> Rm(Pth Path, CT Ct);

	/// 复制文件，或递归复制整个目录树。
	/// Source 也可为 glob；例如 Cp("ExternalRsrc/*", "output") 等价于 Bash 的 cp -r ExternalRsrc/* output。
	public partial void Cp(Pth Source, Pth Destination, CpOptions? Options = null);

	/// 异步复制文件、目录或 glob 匹配项；Ct 必须作为最后一个位置参数传入。
	public partial Task<nil> Cp(Pth Source, Pth Destination, CT Ct);

	/// 异步复制文件或目录；需要覆写既有目标时传入 Options；Ct 必须作为最后一个位置参数传入。
	public partial Task<nil> Cp(Pth Source, Pth Destination, CpOptions? Options, CT Ct);

	/// 移动文件或目录；Overwrite 控制目标已存在时是否替换。
	public partial void Mv(Pth Source, Pth Destination, MvOptions? Options = null);

	/// 异步移动文件或目录；Ct 必须作为最后一个位置参数传入。
	public partial Task<nil> Mv(Pth Source, Pth Destination, CT Ct);

	/// 异步移动文件或目录；需要覆写既有目标时传入 Options；Ct 必须作为最后一个位置参数传入。
	public partial Task<nil> Mv(Pth Source, Pth Destination, MvOptions? Options, CT Ct);
	
	/// 取得路徑的最後一段檔案或目錄名稱。
	/// 採用 System.IO.Path 的元件擷取語義；根路徑會回傳空字串。
	public partial Pth BaseName(Pth Path);

	/// 取得路徑的父目錄部分。
	/// 採用 System.IO.Path 的元件擷取語義；沒有父目錄或僅為根路徑時回傳空字串。
	public partial Pth DirName(Pth Path);

	/// 依此 Sh 的目前工作目錄展開為絕對、規範化路徑。
	/// 不要求目標存在，且不解析符號連結；等價於 System.IO.Path.GetFullPath。
	public partial Pth FullPath(Pth Path);

	/// 惰性查找匹配的文件系统路径；相对 Pattern 按当前 Sh 的工作目录解析。
	/// 语法与匹配语义由 Meziantou.Framework.Globbing 的 Standard dialect 定义，支持 *、?、[]、{} 与 **。
	/// 不以 / 结尾的模式只匹配文件；以 / 结尾的模式只匹配目录，且字面量目录如 Glob("src/assets/") 会返回该目录本身。
	/// 返回完整 Pth，不建立 FileSystemInfo；需要类型或 metadata 时，显式调用 IsFile、IsDir 或 FsInfo。
	/// 列举在 foreach 时才访问文件系统，所以目录不存在、权限不足等错误也会在 foreach 时抛出。
	/// <example>
	/// foreach (var FilePath in Glob("src/**/*.cs")) {
	/// 	Console.WriteLine(FilePath);
	/// }
	/// foreach (var DirectoryPath in Glob("src/*/")) {
	/// 	Console.WriteLine(DirectoryPath);
	/// }
	/// </example>
	public partial IEnumerable<Pth> Glob(Pth Pattern);

	/// 惰性列出 Path 的文件和子目录路径，等价于 Bash 的 ls；Path 为 null 时列出当前 Sh 的工作目录。
	/// 直接转发 Directory.EnumerateFileSystemEntries，因此只产生路径，不读取大小、时间等 metadata。
	/// <example>
	/// foreach (var EntryPath in Ls("artifacts")) {
	/// 	Console.WriteLine(EntryPath);
	/// }
	/// </example>
	public partial IEnumerable<Pth> Ls(Pth? Path = null, LsOptions? Options = null);

	/// 惰性列出 Path 的子目录路径；Path 为 null 时列出当前 Sh 的工作目录。
	/// 直接转发 Directory.EnumerateDirectories；使用 new LsOptions(Recursive: true) 可递迴列出所有子目录。
	/// <example>
	/// foreach (var DirectoryPath in LsDir("src", new LsOptions(Recursive: true))) {
	/// 	Console.WriteLine(DirectoryPath);
	/// }
	/// </example>
	public partial IEnumerable<Pth> LsDir(Pth? Path = null, LsOptions? Options = null);

	/// 惰性列出 Path 的文件路径；Path 为 null 时列出当前 Sh 的工作目录。
	/// 直接转发 Directory.EnumerateFiles；使用 new LsOptions(Recursive: true) 可遞迴列出所有檔案。
	/// <example>
	/// foreach (var FilePath in LsFile("src")) {
	/// 	Console.WriteLine(FilePath);
	/// }
	/// </example>
	public partial IEnumerable<Pth> LsFile(Pth? Path = null, LsOptions? Options = null);

	/// 讀取檔案為 Content；呼叫方可將結果隱式視為 string 或普通 Stream。
	public partial Content Read(Pth Path);

	/// 非同步讀取檔案為 Content；Ct 必須作為最後一個位置參數傳入。
	public partial Task<Content> Read(Pth Path, CT Ct);

	/// 覆寫檔案內容，等價於 > Path。
	/// Source 接受 Content；string 與普通 Stream 均可隱式轉入 Content。
	public partial void Write(Pth Path, Content Source);

	/// 以字串路徑覆寫檔案內容。
	/// 此重載優先保留 Write("file", "text") 的腳本寫法，避免與 Content 目標寫入重載二義。
	public partial void Write(str Path, Content Source);

	/// 非同步覆寫檔案內容；Ct 必須作為最後一個位置參數傳入。
	public partial Task<nil> Write(Pth Path, Content Source, CT Ct);

	/// 以字串路徑非同步覆寫檔案內容；Ct 必須作為最後一個位置參數傳入。
	public partial Task<nil> Write(str Path, Content Source, CT Ct);

	/// 追加檔案內容，等價於 >> Path。
	/// Source 接受 Content；string 與普通 Stream 均可隱式轉入 Content。
	public partial void Append(Pth Path, Content Source);

	/// 以字串路徑追加檔案內容。
	public partial void Append(str Path, Content Source);

	/// 非同步追加檔案內容；Ct 必須作為最後一個位置參數傳入。
	public partial Task<nil> Append(Pth Path, Content Source, CT Ct);

	/// 以字串路徑非同步追加檔案內容；Ct 必須作為最後一個位置參數傳入。
	public partial Task<nil> Append(str Path, Content Source, CT Ct);

	/// 將一個檔案複製至既有目的地規則所解析出的目標位置。
	private partial Task CopyFile(str Source, str Destination, bool Overwrite, CT Ct);

	/// 遞迴複製一個目錄樹，並依 Overwrite 決定衝突處理方式。
	private partial Task CopyDirectory(str Source, str Destination, bool Overwrite, CT Ct);

	/// 複製 glob 匹配的每個項目至目的地。
	private partial Task CopyMatches(str Source, str Destination, bool Overwrite, CT Ct);

	/// 將來源目錄內容合併至目的目錄。
	private partial Task CopyDirectoryMerge(str Source, str Destination, CT Ct);

	/// 判斷路徑是否含有 Cssh 支援的 glob 萬用字元。
	private partial bool HasGlob(str Path);

	/// 取得傳給第三方 Glob 的搜尋根目錄，使相對模式不會以 .. 開頭。
	/// IsDirectoryPattern 保留原始輸入的尾隨分隔符語義，因為 Path.GetRelativePath 會將其移除。
	private partial str GetGlobSearchRoot(str FullPattern, bool IsDirectoryPattern);

	/// 按目的地是否為目錄，解析來源項目的最終目的路徑。
	private partial str ResolveDestinationPath(str SourcePath, str DestinationPath);

	/// 建立所有 Ls 系列共用的 BCL 列舉選項，保留可列舉的隱藏和系統項目。
	private partial EnumerationOptions MkLsEnumerationOptions(LsOptions? Options);

	/// 在建立檔案前確保其父目錄存在。
	private partial void EnsureParentDirectory(str FileSystemPath);
}
