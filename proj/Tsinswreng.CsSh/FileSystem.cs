namespace Tsinswreng.CsSh;

/// Cp 的可选行为。
public sealed record CpOptions(
	bool Overwrite = false);

/// Mv 的可选行为。
public sealed record MvOptions(
	bool Overwrite = false);

/// Ls 的可选行为。
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
	
	/// 取得路徑的最後一段檔案或目錄名稱，等價於 Bash basename。
	public partial Pth BaseName(Pth Path);

	/// 取得路徑的父目錄部分，等價於 Bash dirname。
	public partial Pth DirName(Pth Path);

	/// 依此 Sh 的目前工作目錄展開為絕對、規範化路徑。
	/// 不要求目標存在，且不解析符號連結；等價於 System.IO.Path.GetFullPath。
	public partial Pth FullPath(Pth Path);

/// 惰性查找匹配的文件系统项目；这是递归枚举加 glob 过滤，不是 Bash find 条件表达式。
	/// Pattern 是跨平台 glob 路径，支持 /、*、? 与 **；例如 Glob("src/**/*.csproj")。
	/// 返回 .NET 的 FileSystemInfo；文件为 FileInfo、目录为 DirectoryInfo。
/// 可直接读取 Attributes、LastWriteTimeUtc 等 BCL 属性；文件大小使用 FileInfo.Length。
public partial IEnumerable<FileSystemInfo> Glob(Pth Pattern);

	/// 异步查找匹配的文件系统项目；Ct 必须作为最后一个位置参数传入。
public partial IAsyncEnumerable<FileSystemInfo> Glob(Pth Pattern, CT Ct);

	/// 惰性列出目录中的子项，等价于 Bash 的 ls。
	/// 返回 .NET 的 FileSystemInfo；Path 为 null 时列出当前目录；Recursive 为 false 时仅列出直接子项。
	public partial IEnumerable<FileSystemInfo> Ls(Pth? Path = null, LsOptions? Options = null);

	/// 异步列出目录中的子项；Ct 必须作为最后一个位置参数传入。
	public partial IAsyncEnumerable<FileSystemInfo> Ls(Pth? Path, CT Ct);

	/// 异步列出目录中的子项；需要递归列出时传入 Options；Ct 必须作为最后一个位置参数传入。
	public partial IAsyncEnumerable<FileSystemInfo> Ls(Pth? Path, LsOptions? Options, CT Ct);

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
}
