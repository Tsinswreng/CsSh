namespace Tsinswreng.Cssh;

/// 描述一次外部程序调用。
/// 参数逐项传递给子进程，绝不经由 shell 字符串解析；因此参数中的空格和特殊字符不需要调用方自行转义。
public sealed record CommandSpec(
	str Program,
	IReadOnlyList<str> Arguments){
	/// 子进程的工作目录；为 null 时使用调用进程当前目录。
	/// 普通 C# 程序在并发运行命令时应优先设置此属性，而非调用全局的 Sh.Cd。
	public str? Cwd{get;init;}

	/// 仅对此子进程生效的环境变量覆盖项。
	/// value 为 null 表示从子进程环境中移除此变量。
	public IReadOnlyDictionary<str, str?>? EnvironmentVariables{get;init;}

	/// 标准输出与标准错误的处理方式。
	public CommandOutputMode OutputMode{get;init;} = CommandOutputMode.Inherit;
}

/// 控制外部命令的标准输出和标准错误如何处理。
public enum CommandOutputMode{
	/// 继承当前进程的终端。脚本默认使用此模式，以获得与 Bash 一致的即时输出。
	Inherit,
	/// 捕获输出并放入 CommandResult，适合需要解析命令结果的调用方。
	Capture,
}

/// 外部命令的完整执行结果。
/// Sh.Run 在 ExitCode 非零时会抛出 CommandFailedException；Sh.TryRun 始终返回此对象。
public sealed record CommandResult(
	CommandSpec Command,
	i32 ExitCode,
	str? StandardOutput,
	str? StandardError,
	TimeSpan Duration){
	/// 指示命令是否以零退出码完成。
	public bool IsSuccess{get;init;}
}

/// Sh.Run 遇到非零退出码时抛出的异常。
public sealed partial class CommandFailedException:Exception{
	/// 失败命令的结构化描述。
	public CommandSpec Command{get;}

	/// 失败命令的执行结果。
	public CommandResult Result{get;}

	/// 由失败命令及执行结果创建异常。
	public partial CommandFailedException(CommandSpec Command, CommandResult Result);
}
