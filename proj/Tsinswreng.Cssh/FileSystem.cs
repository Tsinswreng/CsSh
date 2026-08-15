namespace Tsinswreng.Cssh;

/// 供 csx 构建脚本直接静态导入的文件系统动词。
/// 每个路径均由 .NET 负责平台适配，不依赖 Bash、PowerShell 或外部 coreutils。
public static partial class Sh{
	/// 判断路径是否存在，可以是文件或目录。
	public static partial bool Exists(str Path);

	/// 建立目录及所有缺失的父目录；已存在时不报错。
	public static partial void Mkdir(params str[] Paths);

	/// 删除文件，或递归删除目录。
	/// Force 为 true 时，目标不存在也视为成功，适合清理构建产物。
	public static partial void Rm(str Path, bool Force = false);

	/// 复制文件，或递归复制整个目录树。
	public static partial void Cp(str Source, str Destination, bool Overwrite = false);

	/// 移动文件或目录。
	/// Force 为 true 时，源路径不存在也视为成功；Overwrite 控制目标已存在时是否替换。
	public static partial void Mv(str Source, str Destination, bool Overwrite = false, bool Force = false);

	/// 惰性枚举匹配的文件。
	/// Pattern 使用 .NET 的文件名 wildcard；Under 为 null 时从当前目录开始，Recursive 默认递归子目录。
	public static partial IEnumerable<str> Find(str Pattern = "*", str? Under = null, bool Recursive = true);

	/// 以文本方式读取文件。
	public static partial str Read(str Path, Encoding? Encoding = null);

	/// 写入文本；CreateParentDirectory 为 true 时自动建立父目录。
	public static partial void Write(str Path, str Content, Encoding? Encoding = null, bool CreateParentDirectory = true);

	/// 异步读取文本文件。
	public static partial Task<str> Read(str Path, Encoding? Encoding, CT Ct);

	/// 异步写入文本文件。
	public static partial Task<nil> Write(str Path, str Content, Encoding? Encoding, bool CreateParentDirectory, CT Ct);
}
