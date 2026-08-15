namespace Tsinswreng.Cssh;

/// Cssh 面向 csx 的唯一脚本入口。
/// 脚本可使用 using static Tsinswreng.Cssh.Sh，直接书写 Cd、X、Mkdir、Rm、Cp、Mv 与 Find。
/// 普通 C# 程序若存在并发命令，不应修改全局工作目录，而应通过 CommandSpec.Cwd 为各命令指定目录。
public static partial class Sh{
	/// 取得当前进程的工作目录。
	public static partial str Pwd();

	/// 取得当前 csx 脚本所在的目录。
	/// 与 Pwd 不同：无论调用者从哪个工作目录启动 dotnet script，此值均指向脚本文件的父目录。
	public static partial str ScriptDir();

	/// 取得传给当前 csx 脚本的命令行参数，不包含 dotnet-script 自身参数和脚本路径。
	public static partial IReadOnlyList<str> Args();

	/// 切换当前进程的工作目录，等价于 Bash 的 cd。
	/// Cssh 的所有路径参数均接受正斜杠；例如 Cd("src/app") 在 Windows、Linux 与 macOS 上含义相同。
	/// 此变更影响后续相对路径和未设置 CommandSpec.Cwd 的命令。
	public static partial void Cd(str Path);

	/// 执行一行命令，等价于脚本中的 `dotnet publish -c Release`。
	/// Cssh 负责将 Command 解析为程序名和参数，并支持双引号包裹含空格的参数；Cwd 指定子进程目录，为 null 时使用当前目录；非零退出码时抛出 CommandFailedException。
	/// 此重载不启动 Bash、PowerShell 或 cmd，故不解释管道、重定向、变量展开、&& 等 shell 语法。
	public static partial CommandResult X(str Command, str? Cwd = null);

	/// 按完整命令描述执行命令。
	/// 可通过 CommandSpec 指定子进程工作目录、临时环境变量和捕获输出模式；非零退出码会抛异常。
	public static partial CommandResult X(CommandSpec Command);

	/// 异步执行完整命令描述；Ct 取消等待并终止尚未结束的子进程。
	public static partial Task<CommandResult> X(CommandSpec Command, CT Ct);

	/// 执行一行命令但不因非零退出码抛异常。
	/// 命令字符串的解析规则与 X 相同；Cwd 指定子进程目录，为 null 时使用当前目录。
	public static partial CommandResult TryX(str Command, str? Cwd = null);

	/// 按完整命令描述执行命令但不因非零退出码抛异常。
	public static partial CommandResult TryX(CommandSpec Command);

	/// 异步执行完整命令描述但不因非零退出码抛异常。
	public static partial Task<CommandResult> TryX(CommandSpec Command, CT Ct);
}
