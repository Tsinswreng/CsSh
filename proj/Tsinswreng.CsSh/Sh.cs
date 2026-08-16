namespace Tsinswreng.CsSh;

/// Cssh 的一个脚本上下文实例。
/// 每个实例拥有自己的当前目录与标准流；脚本通常通过 ShGlobal 的 using static facade 使用默认实例。
public partial class Sh{
	/// 建立一个以当前进程目录为初始目录的 Shell 上下文。
	public partial Sh();

	/// 当前脚本进程的标准输入内容。
	public Content Stdin{get;}

	/// 当前脚本进程的标准输出内容。
	public Content Stdout{get;}

	/// 当前脚本进程的标准错误内容。
	public Content Stderr{get;}

	/// 跨平台的空内容目标，等价于 Bash 的 /dev/null 或 Windows 的 NUL。
	public Content Null{get;}

	/// 取得当前进程的工作目录。
	/// 返回路径统一使用正斜杠，因而可直接与 Cssh 的所有路径 API 拼接。
	public partial str Pwd();

	/// 取得当前 csx 脚本所在的目录。
	/// 与 Pwd 不同：无论调用者从哪个工作目录启动 dotnet script，此值均指向脚本文件的父目录。
	/// 返回路径统一使用正斜杠。
	public partial str CsxDir();

	/// 取得传给当前 csx 脚本的命令行参数，不包含 dotnet-script 自身参数和脚本路径。
	public partial IReadOnlyList<str> Args();

	/// 同步输出一行文本到标准输出，等价于 Bash 的 echo。
	public partial void Echo(str Text);

	/// 异步输出一行文本到标准输出。
	public partial Task<nil> Echo(str Text, CT Ct);

	/// 切换当前进程的工作目录，等价于 Bash 的 cd。
	/// Cssh 的所有路径参数均接受正斜杠；例如 Cd("src/app") 在 Windows、Linux 与 macOS 上含义相同。
	/// 此变更影响后续相对路径和未显式传入 Cwd 的 X 命令。
	public partial void Cd(str Path);

	/// 创建一条尚未启动的命令。
	/// Command 仅按第一个空白字符切出可执行文件，其余原样作为 arguments 字符串交给子进程；不解析 shell 的管道、重定向、变量展开或引号语法。
	/// 子进程不重定向标准输入并使用当前目录。
	public partial Command Cmd(str Command);

	/// 创建一条带输入流或工作目录配置的尚未启动命令。
	public partial Command Cmd(str Command, CommandOptions Options);

	/// 使用可取消的异步流转送创建一条尚未启动的命令。
	/// Ct 必须作为最后一个位置参数传入。
	public partial Command Cmd(str Command, in CT Ct);

	/// 使用可取消的异步流转送创建一条尚未启动的命令。
	/// Ct 必须作为最后一个位置参数传入；需要输入流或工作目录时使用带 Options 的重载。
	public partial Command Cmd(str Command, CommandOptions Options, CT Ct);

	/// 创建一条尚未启动、且非零退出码不抛异常的命令。
	/// 其他语义与 X 相同，退出码通过 Command.Done 的 CommandExit 返回。
	public partial Command TryCmd(str Command);

	/// 创建一条带输入流或工作目录配置的尚未启动命令，且非零退出码不抛异常。
	public partial Command TryCmd(str Command, CommandOptions Options);

	/// 创建一条尚未启动、且可由 Ct 取消的命令；Ct 必须作为最后一个位置参数传入。
	public partial Command TryCmd(str Command, in CT Ct);

	/// 创建一条尚未启动、且可由 Ct 取消的命令；Ct 必须作为最后一个位置参数传入。
	public partial Command TryCmd(str Command, CommandOptions Options, CT Ct);

	/// 執行命令，將 stdout/stderr 寫入此 Shell 的標準輸出與標準錯誤。
	public partial CommandExit Exe(str Command);

	/// 以輸入或工作目錄設定執行命令，並轉送兩條輸出流。
	public partial CommandExit Exe(str Command, CommandOptions Options);

	/// 非同步執行命令並轉送兩條輸出流；Ct 必須作為最後一個位置參數。
	public partial Task<CommandExit> Exe(str Command, CT Ct);

	/// 非同步以可選設定執行命令並轉送兩條輸出流；Ct 必須作為最後一個位置參數。
	public partial Task<CommandExit> Exe(str Command, CommandOptions Options, CT Ct);

	/// 執行命令並轉送兩條輸出流；非零退出碼回傳 CommandExit，不丟例外。
	public partial CommandExit TryExe(str Command);

	/// 以可選設定執行命令；非零退出碼回傳 CommandExit，不丟例外。
	public partial CommandExit TryExe(str Command, CommandOptions Options);

	/// 非同步執行命令；非零退出碼回傳 CommandExit，不丟例外。
	public partial Task<CommandExit> TryExe(str Command, CT Ct);

	/// 非同步以可選設定執行命令；非零退出碼回傳 CommandExit，不丟例外。
	public partial Task<CommandExit> TryExe(str Command, CommandOptions Options, CT Ct);

	/// 非同步地将 Source 复制到 Target。
	/// 读取命令的 Result.Stdout 或 Result.Stderr 会触发命令执行；Source 结束时不会关闭调用方提供的 Target。
	public partial void Write(Content Target, Content Source);

	/// 非同步地将 Source 复制到 Target；Ct 必须作为最后一个位置参数传入。
	public partial Task<nil> Write(Content Target, Content Source, CT Ct);

	/// 非同步合并多条 Source 流并写入同一 Target。
	/// Cssh 串行化对 Target 的写入，因此可安全实现 stdout 与 stderr 合并，而不并发写同一 Stream。
	public partial void Write(Content Target, IReadOnlyList<Content> Sources);

	/// 非同步合并多条 Source 流并写入同一 Target；Ct 必须作为最后一个位置参数传入。
	public partial Task<nil> Write(Content Target, IReadOnlyList<Content> Sources, CT Ct);
}
