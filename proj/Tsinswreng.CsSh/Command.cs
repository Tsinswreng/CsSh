using System.Diagnostics;
using System.IO.Pipelines;

namespace Tsinswreng.CsSh;

/// 一条尚未启动的外部命令。
/// Cmd 创建该对象不会执行进程；首次异步读取 Result.Stdout、Result.Stderr，或等待 Done 时才启动一次。
/// 命令实现会同时排空 stdout 与 stderr，防止调用方暂未消费其中一条流时造成子进程 pipe 阻塞。
public sealed partial class Command:IDisposable,IAsyncDisposable{
	/// 未指定 CommandOptions.PipeBufferSizeBytes 時，每一條 stdout 或 stderr Pipe 使用的預設容量。
	/// 此值限制每條輸出在消費者尚未讀取時可暫存的資料量；兩條 Pipe 各自使用此容量。
	public const i64 DefaultPipeBufferSizeBytes = 64 * 1024;

	/// 建立命令時凍結的可執行檔、參數、工作目錄、環境、標準流與錯誤策略。
	public readonly CommandRunOptions Options;
	/// 接收子程序標準輸出的暫存管線；讀取 Result.Stdout 時由此取得資料。
	public readonly Pipe StdoutPipe;
	/// 接收子程序標準錯誤的暫存管線；讀取 Result.Stderr 時由此取得資料。
	public readonly Pipe StderrPipe;
	/// 儲存唯一的命令退出結果或執行例外，避免多個觀察者重複完成任務。
	public readonly TaskCompletionSource<CommandExit> ExitSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
	/// 保護啟動、釋放與 Process 欄位的同步鎖，避免命令被並行啟動或釋放。
	public readonly object Gate = new();
	/// 唯一的背景啟動工作；null 代表命令尚未被觀察，因而尚未啟動。
	public Task? StartTask;
	/// 已啟動的 .NET Process；完成或釋放後設為 null，避免持有原生控制代碼。
	public Process? Process;
	/// 標示 Command 已釋放；釋放後不得再啟動新的子程序。
	public bool IsDisposed;
	/// 僅供 Sh 以完整執行配置建立命令。
	public partial Command(CommandRunOptions Options);

	/// 命令产生的两条标准输出流。
	public CommandResult Result{get;}

	/// 等待子进程退出并取得退出结果。
	/// Cmd 创建的命令以非零退出码结束时，此任务抛出 CommandFailedException；TryCmd 创建的命令始终返回结果。
	public Task<CommandExit> Done{
		get{
			EnsureStarted();
			return ExitTask;
		}
	}

	/// 尚未觀察命令時也可被釋放；實作以此欄位保存唯一的退出工作。
	public readonly Task<CommandExit> ExitTask;

	/// 让 await Command 等价于 await Command.Done。
	/// 等待该对象会启动命令，但不会自动转送 Result 中的输出；要转送时使用 Out。
	public partial System.Runtime.CompilerServices.TaskAwaiter<CommandExit> GetAwaiter();

	/// 非同步消費命令結果：stdout 寫入 Sh.Stdout，stderr 寫入 Sh.Stderr，並等待命令退出。
	public partial Task<CommandExit> Out(CT Ct);

	/// 非同步消費命令結果：stdout 與 stderr 都寫入 Target，並等待命令退出。
	/// 兩條流會並行讀取、分段序列化寫入，因此有限 Pipe 不會因其中一條暫未被讀取而卡住。
	public partial Task<CommandExit> Out(Content Target, CT Ct);

	/// 非同步消費命令結果：stdout 與 stderr 都覆寫寫入 TargetPath，並等待命令退出。
	/// 這是腳本將整條命令輸出寫入檔案的簡寫；TargetPath 的父目錄會自動建立。
	public partial Task<CommandExit> Out(Pth TargetPath, CT Ct);

	/// 非同步消費命令結果：分別指定 stdout 與 stderr 的輸出目標，並等待命令退出。
	public partial Task<CommandExit> Out(Content Stdout, Content Stderr, CT Ct);

	/// 完整讀取 stdout 與 stderr 文字，並等待命令退出。
	/// 這是終端消費操作，適用於 JSON、版本資訊等有限輸出；大量輸出應直接消費 Result 中的 Content 流。
	public partial Task<CommandTextResult> Text(CT Ct);

	/// 释放结果流的临时资源；进程尚未结束时同时终止该进程。
	public partial void Dispose();

	/// 异步释放结果流的临时资源；进程尚未结束时同时终止该进程。
	public partial ValueTask DisposeAsync();

	/// 同時讀取 stdout 與 stderr，並將各資料區塊序列化寫入同一目標。
	private partial Task<CommandExit> OutMerged(Content Target, CT Ct);

	/// 確保命令只會啟動一次；由結果資料流的讀取包裝呼叫。
	internal partial void EnsureStarted();

	/// 啟動外部進程、轉送各條資料流並完成退出結果。
	private partial Task Start();

	/// 根據命令執行設定建立不經 Shell 解譯的 ProcessStartInfo。
	private partial ProcessStartInfo MakeStartInfo();

	/// 將可選標準輸入來源複製至已啟動進程。
	private partial Task CopyInput(Process Process);

	/// 確保標準輸入在正常完成或失敗時都被關閉，讓子進程能觀察到 EOF。
	private partial Task CloseInput(Process Process);

	/// 將一條 Content 資料流複製至目標。
	private static partial Task Write(Content Target, Content Source, CT Ct);

	/// 同時讀取多條來源，並以每個資料區塊為單位序列化寫入同一目標。
	private static partial Task WriteMerged(Content Target, IReadOnlyList<Content> Sources, CT Ct);

	/// 依指定的單條 Pipe 容量建立具備背壓的 PipeOptions。
	private static partial PipeOptions MkPipeOptions(i64? PipeBufferSizeBytes);

	/// 完成 stdout 與 stderr 管線，並將啟動或轉送失敗傳遞給讀取端。
	private partial Task CompletePipes(Exception? Error = null);

	/// 終止仍在執行中的子進程。
	private partial void TryKill();

	/// 釋放已建立的 Process 控制代碼；輸出管線完成後才可呼叫。
	private partial void DisposeProcess();
}

/// 建立 Command 時的可選配置。
/// 每個設定只影響本次命令；不會改變 Sh 實例或目前程序的全域狀態。
public sealed record CommandOptions(
	/// 子程序的標準輸入來源；null 代表不重定向 stdin，子程序沿用宿主程序的標準輸入。
	Content? Stdin = null,
	/// 子程序工作目錄；null 時使用建立此 Command 的 Sh 目前工作目錄。
	Pth? Cwd = null,
	/// 僅套用於子程序的環境變數覆寫；鍵的值為 null 時從子程序環境移除該鍵。
	IReadOnlyDictionary<str, str?>? Env = null,
	/// 每一條 stdout 或 stderr Pipe 的最大暫存位元組數；null 時使用 Command.DefaultPipeBufferSizeBytes。
	/// 此值必須大於 0；Pipe 滿時會以背壓暫停上游輸出，而非無限制佔用記憶體。
	i64? PipeBufferSizeBytes = null);

/// Command 的內部執行配置。
/// 此 record 在建立 Command 時完成快照，避免呼叫端在命令啟動前修改參數或環境集合而改變行為。
public sealed record CommandRunOptions(
	/// 要啟動的可執行檔名稱或路徑；不經由 Shell 字串解析。
	str Exe,
	/// 子程序參數；每個元素是一個完整參數，會直接加入 ProcessStartInfo.ArgumentList。
	IReadOnlyList<str> Args,
	/// 呼叫端提供的選用設定；保留 stdin 來源與原始選項供命令生命週期使用。
	CommandOptions Options,
	/// 已展開為絕對路徑的子程序工作目錄。
	Pth Cwd,
	/// 建立命令當下取得的子程序環境快照；不會受後續宿主環境變更影響。
	IReadOnlyDictionary<str, str> Environment,
	/// Exe 或 Command.Out() 未指定目標時接收子程序 stdout 的預設 Content。
	Content Stdout,
	/// Exe 或 Command.Out() 未指定目標時接收子程序 stderr 的預設 Content。
	Content Stderr,
	/// 取消命令啟動、資料轉送與等待的取消令牌。
	CT Ct,
	/// true 時非零退出碼會讓 Done 以 CommandFailedException 失敗；false 時回傳失敗的 CommandExit。
	bool ThrowOnError);

/// Command 产生的标准输出。
/// 两条 Stream 均为只读、惰性流：首次 ReadAsync 会启动所属命令；数据边产生边可读，不预先全部载入内存。
public sealed record CommandResult(
	/// 子程序標準輸出；只能按 Stream 的一次性消費規則讀取。
	Content Stdout,
	/// 子程序標準錯誤；只能按 Stream 的一次性消費規則讀取。
	Content Stderr);

/// 完整消費命令文字輸出後取得的結構化結果。
/// Stdout 與 Stderr 各自保留，Exit 保留退出碼、耗時與成功狀態。
public sealed record CommandTextResult(
	/// 完整消費 stdout 後以文字解碼得到的內容；只適合有限大小輸出。
	str Stdout,
	/// 完整消費 stderr 後以文字解碼得到的內容；只適合有限大小輸出。
	str Stderr,
	/// 命令退出碼、耗時及成功狀態。
	CommandExit Exit);

/// 子进程退出后的结构化结果。
public sealed record CommandExit(
	/// 子程序回報的原始退出碼；0 代表成功。
	i32 ExitCode,
	/// 從 Process.Start 成功到子程序退出的耗時。
	TimeSpan Duration,
	/// ExitCode 是否為 0 的便利判斷值。
	bool IsSuccess);

/// Sh.Cmd 遇到非零退出码时抛出的异常。
public sealed partial class CommandFailedException:Exception{
	/// 失败命令退出时的结果。
	public CommandExit Exit{get;}

	/// 由失败命令及执行结果创建异常。
	public partial CommandFailedException(CommandExit Exit);
}
