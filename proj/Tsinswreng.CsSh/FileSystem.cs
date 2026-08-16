using System.Text;

namespace Tsinswreng.CsSh;

/// 由 Ls 或 Find 列出的一个文件系统项目。
/// Path 是可直接传回 Cssh 文件操作的完整路径；Name 是最后一段名称。
/// IsFile、IsDir 与 IsLink 保留项目类型，脚本无需重新访问文件系统或从路径文本猜测类型。
public sealed record FileSystemEntry(
	str Path,
	str Name,
	bool IsFile,
	bool IsDir,
	bool IsLink);

/// Cp 的可选行为。
public sealed record CpOptions(
	bool Overwrite = false);

/// Mv 的可选行为。
public sealed record MvOptions(
	bool Overwrite = false);

/// Ls 的可选行为。
public sealed record LsOptions(
	bool Recursive = false);

/// 文本写入的可选行为。
public sealed record TextWriteOptions(
	Encoding? Encoding = null,
	bool CreateParentDirectory = true);

/// 供 csx 构建脚本直接静态导入的文件系统动词。
/// 每个路径均由 .NET 负责平台适配，不依赖 Bash、PowerShell 或外部 coreutils。
/// 调用方可统一使用正斜杠，例如 Rm("artifacts/publish")；无需调用 Path.Combine。
public static partial class Sh{
	/// 判断路径是否存在，可以是文件或目录。
	public static partial bool Exists(str Path);

	/// 异步判断路径是否存在；Ct 必须作为最后一个位置参数传入。
	public static partial Task<bool> Exists(str Path, CT Ct);

	/// 建立目录及所有缺失的父目录；已存在时不报错。
	public static partial void Mkdir(str Path);

	/// 异步建立目录；Ct 必须作为最后一个位置参数传入。
	public static partial Task<nil> Mkdir(str Path, CT Ct);

	/// 删除文件，或递归删除目录。
	/// 固定等价于 Bash 的 rm -rf：目标不存在时忽略，绝不交互确认；权限不足等真实文件系统错误仍会抛出。
	public static partial void Rm(str Path);

	/// 异步删除文件或目录；语义与同步 Rm 相同，Ct 必须作为最后一个位置参数传入。
	public static partial Task<nil> Rm(str Path, CT Ct);

	/// 复制文件，或递归复制整个目录树。
	public static partial void Cp(str Source, str Destination, CpOptions? Options = null);

	/// 异步复制文件或目录；Ct 必须作为最后一个位置参数传入。
	public static partial Task<nil> Cp(str Source, str Destination, CT Ct);

	/// 异步复制文件或目录；需要覆写既有目标时传入 Options；Ct 必须作为最后一个位置参数传入。
	public static partial Task<nil> Cp(str Source, str Destination, CpOptions? Options, CT Ct);

	/// 移动文件或目录；Overwrite 控制目标已存在时是否替换。
	public static partial void Mv(str Source, str Destination, MvOptions? Options = null);

	/// 异步移动文件或目录；Ct 必须作为最后一个位置参数传入。
	public static partial Task<nil> Mv(str Source, str Destination, CT Ct);

	/// 异步移动文件或目录；需要覆写既有目标时传入 Options；Ct 必须作为最后一个位置参数传入。
	public static partial Task<nil> Mv(str Source, str Destination, MvOptions? Options, CT Ct);

	/// 惰性查找匹配的文件系统项目，等价于 Bash 的 find。
	/// Pattern 是跨平台 glob 路径，支持 /、*、? 与 **；例如 Find("src/**/*.csproj")。
	/// 返回 FileSystemEntry，并保留每个项目的路径、名称与类型。
	public static partial IEnumerable<FileSystemEntry> Find(str Pattern);

	/// 异步查找匹配的文件系统项目；Ct 必须作为最后一个位置参数传入。
	public static partial IAsyncEnumerable<FileSystemEntry> Find(str Pattern, CT Ct);

	/// 惰性列出目录中的子项，等价于 Bash 的 ls。
	/// 返回 FileSystemEntry；Path 为 null 时列出当前目录；Recursive 为 false 时仅列出直接子项。
	public static partial IEnumerable<FileSystemEntry> Ls(str? Path = null, LsOptions? Options = null);

	/// 异步列出目录中的子项；Ct 必须作为最后一个位置参数传入。
	public static partial IAsyncEnumerable<FileSystemEntry> Ls(str? Path, CT Ct);

	/// 异步列出目录中的子项；需要递归列出时传入 Options；Ct 必须作为最后一个位置参数传入。
	public static partial IAsyncEnumerable<FileSystemEntry> Ls(str? Path, LsOptions? Options, CT Ct);

	/// 异步打开一个只读文件流。
	public static partial Stream OpenRead(str Path);

	/// 异步打开只读文件流；Ct 必须作为最后一个位置参数传入。
	public static partial Task<Stream> OpenRead(str Path, CT Ct);

	/// 异步打开或新建一个可覆写的文件流，等价于 > Path。
	public static partial Stream OpenWrite(str Path);

	/// 异步打开可覆写文件流；Ct 必须作为最后一个位置参数传入。
	public static partial Task<Stream> OpenWrite(str Path, CT Ct);

	/// 异步打开或新建一个可追加的文件流，等价于 >> Path。
	public static partial Stream OpenAppend(str Path);

	/// 异步打开可追加文件流；Ct 必须作为最后一个位置参数传入。
	public static partial Task<Stream> OpenAppend(str Path, CT Ct);

	/// 以文本方式读取文件。
	public static partial str Read(str Path, Encoding? Encoding = null);

	/// 异步读取文本文件；Ct 必须作为最后一个位置参数传入。
	public static partial Task<str> Read(str Path, CT Ct);

	/// 异步读取文本文件；需要指定编码时传入 Encoding；Ct 必须作为最后一个位置参数传入。
	public static partial Task<str> Read(str Path, Encoding? Encoding, CT Ct);

	/// 写入文本；CreateParentDirectory 为 true 时自动建立父目录。
	public static partial void Write(str Path, str Content, TextWriteOptions? Options = null);

	/// 异步写入文本文件；Ct 必须作为最后一个位置参数传入。
	public static partial Task<nil> Write(str Path, str Content, CT Ct);

	/// 异步写入文本文件；需要控制编码或父目录创建时传入 Options；Ct 必须作为最后一个位置参数传入。
	public static partial Task<nil> Write(str Path, str Content, TextWriteOptions? Options, CT Ct);
}
