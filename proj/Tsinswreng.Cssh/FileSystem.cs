namespace Tsinswreng.Cssh;

/// 由 Ls 或 Find 列出的一个文件系统项目。
/// Path 是可直接传回 Cssh 文件操作的完整路径；Name 是最后一段名称。
/// IsFile、IsDir 与 IsLink 保留项目类型，脚本无需重新访问文件系统或从路径文本猜测类型。
public sealed record FileSystemEntry(
	str Path,
	str Name,
	bool IsFile,
	bool IsDir,
	bool IsLink);

/// 供 csx 构建脚本直接静态导入的文件系统动词。
/// 每个路径均由 .NET 负责平台适配，不依赖 Bash、PowerShell 或外部 coreutils。
/// 调用方可统一使用正斜杠，例如 Rm("artifacts/publish")；无需调用 Path.Combine。
public static partial class Sh{
	/// 异步判断路径是否存在，可以是文件或目录。
	public static partial Task<bool> Exists(str Path, CT Ct = default);

	/// 建立目录及所有缺失的父目录；已存在时不报错。
	public static partial Task<nil> Mkdir(str Path, CT Ct = default);

	/// 删除文件，或递归删除目录。
	/// Force 为 true 时，目标不存在也视为成功，适合清理构建产物。
	public static partial Task<nil> Rm(str Path, bool Force = false, CT Ct = default);

	/// 复制文件，或递归复制整个目录树。
	public static partial Task<nil> Cp(str Source, str Destination, bool Overwrite = false, CT Ct = default);

	/// 移动文件或目录。
	/// Force 为 true 时，源路径不存在也视为成功；Overwrite 控制目标已存在时是否替换。
	public static partial Task<nil> Mv(str Source, str Destination, bool Overwrite = false, bool Force = false, CT Ct = default);

	/// 惰性查找匹配的文件系统项目，等价于 Bash 的 find。
	/// Pattern 是跨平台 glob 路径，支持 /、*、? 与 **；例如 Find("src/**/*.csproj")。
	/// 返回 FileSystemEntry，并保留每个项目的路径、名称与类型。
	public static partial IAsyncEnumerable<FileSystemEntry> Find(str Pattern, CT Ct = default);

	/// 惰性列出目录中的子项，等价于 Bash 的 ls。
	/// 返回 FileSystemEntry；Path 为 null 时列出当前目录；Recursive 为 false 时仅列出直接子项。
	public static partial IAsyncEnumerable<FileSystemEntry> Ls(str? Path = null, bool Recursive = false, CT Ct = default);

	/// 异步打开一个只读文件流。
	public static partial Task<Stream> OpenRead(str Path, CT Ct = default);

	/// 异步打开或新建一个可覆写的文件流，等价于 > Path。
	public static partial Task<Stream> OpenWrite(str Path, CT Ct = default);

	/// 异步打开或新建一个可追加的文件流，等价于 >> Path。
	public static partial Task<Stream> OpenAppend(str Path, CT Ct = default);

	/// 以文本方式读取文件。
	public static partial Task<str> Read(str Path, Encoding? Encoding = null, CT Ct = default);

	/// 写入文本；CreateParentDirectory 为 true 时自动建立父目录。
	public static partial Task<nil> Write(str Path, str Content, Encoding? Encoding = null, bool CreateParentDirectory = true, CT Ct = default);
}
