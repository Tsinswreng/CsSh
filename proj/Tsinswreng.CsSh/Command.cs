namespace Tsinswreng.Cssh;

/// 一条尚未启动的外部命令。
/// X 创建该对象不会执行进程；首次异步读取 Result.Stdout、Result.Stderr，或等待 Done 时才启动一次。
/// 命令实现会同时排空 stdout 与 stderr，防止调用方暂未消费其中一条流时造成子进程 pipe 阻塞。
public sealed partial class Command:IDisposable,IAsyncDisposable{
	/// 僅供 Sh 以完整執行配置建立命令。
	internal partial Command(CommandRunOptions Options);

	/// 命令产生的两条标准输出流。
	public CommandResult Result{get;}

	/// 等待子进程退出并取得退出结果。
	/// X 创建的命令以非零退出码结束时，此任务抛出 CommandFailedException；TryX 创建的命令始终返回结果。
	public Task<CommandExit> Done{
		get{
			EnsureStarted();
			return ExitTask;
		}
	}

	/// 尚未觀察命令時也可被釋放；實作以此欄位保存唯一的退出工作。
	private readonly Task<CommandExit> ExitTask;

	/// 让 await Command 等价于 await Command.Done。
	/// 等待该对象会启动命令，但不会自动转送 Result 中的输出；需要显示输出时应显式调用 Write。
	public partial System.Runtime.CompilerServices.TaskAwaiter<CommandExit> GetAwaiter();

	/// 释放结果流的临时资源；进程尚未结束时同时终止该进程。
	public partial void Dispose();

	/// 异步释放结果流的临时资源；进程尚未结束时同时终止该进程。
	public partial ValueTask DisposeAsync();
}

/// 建立 Command 時的可選配置。
/// Input 為命令的外部標準輸入來源；Cwd 指定子進程工作目錄。
public sealed record CommandOptions(
	Stream? Input = null,
	str? Cwd = null);

/// Command 的內部執行配置。
internal sealed record CommandRunOptions(
	str Text,
	CommandOptions Options,
	CT Ct,
	bool ThrowOnError);

/// Command 产生的标准输出。
/// 两条 Stream 均为只读、惰性流：首次 ReadAsync 会启动所属命令；数据边产生边可读，不预先全部载入内存。
public sealed record CommandResult(
	Stream Stdout,
	Stream Stderr);

/// 子进程退出后的结构化结果。
public sealed record CommandExit(
	i32 ExitCode,
	TimeSpan Duration,
	bool IsSuccess);

/// Sh.X 遇到非零退出码时抛出的异常。
public sealed partial class CommandFailedException:Exception{
	/// 失败命令退出时的结果。
	public CommandExit Exit{get;}

	/// 由失败命令及执行结果创建异常。
	public partial CommandFailedException(CommandExit Exit);
}
