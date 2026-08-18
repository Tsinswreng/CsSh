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
	public partial Pth Pwd();

	/// 取得当前 csx 脚本所在的目录。
	/// 与 Pwd 不同：无论调用者从哪个工作目录启动 dotnet script，此值均指向脚本文件的父目录。
	/// 返回路径统一使用正斜杠。
	public partial Pth CsxDir();

	/// 同步输出一行文本到标准输出，等价于 Bash 的 echo。
	public partial void Echo(str Text);

	/// 异步输出一行文本到标准输出。
	public partial Task<nil> Echo(str Text, CT Ct);

	/// 切换当前进程的工作目录，等价于 Bash 的 cd。
	/// Cssh 的所有路径参数均接受正斜杠；例如 Cd("src/app") 在 Windows、Linux 与 macOS 上含义相同。
	/// 此变更影响后续相对路径和未显式传入 Cwd 的 X 命令。
	public partial void Cd(Pth Path);

	/// 建立尚未啟動的命令；Args 的每一項是一個完整參數，不需呼叫方處理引號或跳脫。
	public partial Command Cmd(str Exe, IList<str> Args);

	/// 建立尚未啟動的命令；Args 的每一項是一個完整參數。
	public partial Command Cmd(str Exe, IList<str> Args, CommandOptions Options);

	/// 非同步建立尚未啟動的命令；Ct 必須作為最後一個位置參數。
	public partial Command Cmd(str Exe, IList<str> Args, CT Ct);

	/// 非同步建立尚未啟動的命令；Args 的每一項是一個完整參數，Ct 必須作為最後一個位置參數。
	public partial Command Cmd(str Exe, IList<str> Args, CommandOptions Options, CT Ct);

	/// 建立不因非零退出碼丟例外的命令；Args 的每一項是一個完整參數。
	public partial Command TryCmd(str Exe, IList<str> Args);

	/// 建立帶可選設定且不因非零退出碼丟例外的命令。
	public partial Command TryCmd(str Exe, IList<str> Args, CommandOptions Options);

	/// 非同步建立不因非零退出碼丟例外的命令；Ct 必須作為最後一個位置參數。
	public partial Command TryCmd(str Exe, IList<str> Args, CT Ct);

	/// 非同步建立帶可選設定且不因非零退出碼丟例外的命令；Ct 必須作為最後一個位置參數。
	public partial Command TryCmd(str Exe, IList<str> Args, CommandOptions Options, CT Ct);

	/// 執行以參數列表描述的命令；Args 不需呼叫方處理引號或跳脫。
	public partial CommandExit Exe(str FileName, IList<str> Args);

	/// 以可選設定執行參數列表命令。
	public partial CommandExit Exe(str FileName, IList<str> Args, CommandOptions Options);

	/// 非同步執行參數列表命令；Ct 必須作為最後一個位置參數。
	public partial Task<CommandExit> Exe(str FileName, IList<str> Args, CT Ct);

	/// 非同步以可選設定執行參數列表命令；Ct 必須作為最後一個位置參數。
	public partial Task<CommandExit> Exe(str FileName, IList<str> Args, CommandOptions Options, CT Ct);

	/// 執行參數列表命令；非零退出碼回傳 CommandExit，不丟例外。
	public partial CommandExit TryExe(str FileName, IList<str> Args);

	/// 以可選設定執行參數列表命令；非零退出碼不丟例外。
	public partial CommandExit TryExe(str FileName, IList<str> Args, CommandOptions Options);

	/// 非同步執行參數列表命令；非零退出碼不丟例外。
	public partial Task<CommandExit> TryExe(str FileName, IList<str> Args, CT Ct);

	/// 非同步以可選設定執行參數列表命令；非零退出碼不丟例外。
	public partial Task<CommandExit> TryExe(str FileName, IList<str> Args, CommandOptions Options, CT Ct);

	/// 將一個值轉成可嵌入字串命令的單一安全參數。
	/// 新程式優先使用 Args 列表重載；Q 適用於保留 Bash 風格命令字串的腳本。
	public partial str Q(str Value);

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
