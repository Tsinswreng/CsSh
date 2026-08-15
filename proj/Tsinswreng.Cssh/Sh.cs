namespace Tsinswreng.Cssh;

/// Cssh 面向 csx 的唯一脚本入口。
/// 脚本可使用 using static Tsinswreng.Cssh.Sh，直接书写 Cd、Run、Mkdir、Rm、Cp、Mv 与 Find。
/// 普通 C# 程序若存在并发命令，不应修改全局工作目录，而应通过 CommandSpec.Cwd 为各命令指定目录。
public static partial class Sh{
	/// 取得当前进程的工作目录。
	public static partial str Pwd();

	/// 切换当前进程的工作目录，等价于 Bash 的 cd。
	/// Cssh 的所有路径参数均接受正斜杠；例如 Cd("src/app") 在 Windows、Linux 与 macOS 上含义相同。
	/// Cssh 的所有路径参数均接受正斜杠；例如 Cd("src/app") 在 Windows、Linux 与 macOS 上含义相同。
	/// 此变更影响后续相对路径和未设置 CommandSpec.Cwd 的命令。
	public static partial void Cd(str Path);

	/// 执行命令并继承当前终端的输出。
	/// 命令以非零退出码结束时抛出 CommandFailedException。
	public static partial CommandResult Run(str Program, params str[] Arguments);

	/// 按完整命令描述执行命令。
	/// 可通过 CommandSpec 指定子进程工作目录、临时环境变量和捕获输出模式；非零退出码会抛异常。
	public static partial CommandResult Run(CommandSpec Command);

	/// 异步执行完整命令描述；Ct 取消等待并终止尚未结束的子进程。
	public static partial Task<CommandResult> Run(CommandSpec Command, CT Ct);

	/// 执行命令但不因非零退出码抛异常。
	/// 适合探测可选工具或将退出码作为正常分支处理的脚本。
	public static partial CommandResult TryRun(str Program, params str[] Arguments);

	/// 按完整命令描述执行命令但不因非零退出码抛异常。
	public static partial CommandResult TryRun(CommandSpec Command);

	/// 异步执行完整命令描述但不因非零退出码抛异常。
	public static partial Task<CommandResult> TryRun(CommandSpec Command, CT Ct);
}
