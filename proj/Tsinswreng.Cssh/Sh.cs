namespace Tsinswreng.Cssh;

/// Cssh 面向 csx 的唯一脚本入口。
/// 脚本可使用 using static Tsinswreng.Cssh.Sh，直接书写 Cd、X、Mkdir、Rm、Cp、Mv、Ls 与 Find。
public static partial class Sh{
	/// 当前脚本进程的标准输入流。
	/// 作为 X 的 Input 时，子进程将从当前终端读取输入。
	public static Stream Stdin{get;}

	/// 当前脚本进程的标准输出流。
	public static Stream Stdout{get;}

	/// 当前脚本进程的标准错误流。
	public static Stream Stderr{get;}

	/// 跨平台的空流，等价于 Bash 的 /dev/null 或 Windows 的 NUL。
	public static Stream Null{get;}

	/// 取得当前进程的工作目录。
	/// 返回路径统一使用正斜杠，因而可直接与 Cssh 的所有路径 API 拼接。
	public static partial str Pwd();

	/// 取得当前 csx 脚本所在的目录。
	/// 与 Pwd 不同：无论调用者从哪个工作目录启动 dotnet script，此值均指向脚本文件的父目录。
	/// 返回路径统一使用正斜杠。
	public static partial str ScriptDir();

	/// 取得传给当前 csx 脚本的命令行参数，不包含 dotnet-script 自身参数和脚本路径。
	public static partial IReadOnlyList<str> Args();

	/// 异步输出一行文本到标准输出，等价于 Bash 的 echo。
	public static partial Task<nil> Echo(str Text, CT Ct = default);

	/// 切换当前进程的工作目录，等价于 Bash 的 cd。
	/// Cssh 的所有路径参数均接受正斜杠；例如 Cd("src/app") 在 Windows、Linux 与 macOS 上含义相同。
	/// 此变更影响后续相对路径和未显式传入 Cwd 的 X 命令。
	public static partial void Cd(str Path);

	/// 创建一条尚未启动的命令。
	/// Command 仅按第一个空白字符切出可执行文件，其余原样作为 arguments 字符串交给子进程；不解析 shell 的管道、重定向、变量展开或引号语法。
	/// Input 为 null 时子进程继承 Stdin；Cwd 为 null 时使用当前目录；Ct 取消时终止子进程及其流转送。
	public static partial Command X(str Command, Stream? Input = null, str? Cwd = null, CT Ct = default);

	/// 创建一条尚未启动、且非零退出码不抛异常的命令。
	/// 其他语义与 X 相同，退出码通过 Command.Done 的 CommandExit 返回。
	public static partial Command TryX(str Command, Stream? Input = null, str? Cwd = null, CT Ct = default);

	/// 非同步地将 Source 复制到 Target。
	/// 读取命令的 Result.Stdout 或 Result.Stderr 会触发命令执行；Source 结束时不会关闭调用方提供的 Target。
	public static partial Task<nil> Write(Stream Target, Stream Source, CT Ct = default);

	/// 非同步合并多条 Source 流并写入同一 Target。
	/// Cssh 串行化对 Target 的写入，因此可安全实现 stdout 与 stderr 合并，而不并发写同一 Stream。
	public static partial Task<nil> Write(Stream Target, IReadOnlyList<Stream> Sources, CT Ct = default);
}
