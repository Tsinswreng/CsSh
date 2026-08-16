namespace Tsinswreng.CsSh;

/// Implements the static facade by forwarding to its replaceable default instance.
public static partial class ShGlobal{
	public static Content Stdin => Sh.Stdin;
	public static Content Stdout => Sh.Stdout;
	public static Content Stderr => Sh.Stderr;
	public static Content Null => Sh.Null;
	public static partial str Pwd(){ return Sh.Pwd(); }
	public static partial str CsxDir(){ return Sh.CsxDir(); }
	public static partial IReadOnlyList<str> Args(){ return Sh.Args(); }
	public static partial void Echo(str Text){ Sh.Echo(Text); }
	public static partial Task<nil> Echo(str Text, CT Ct){ return Sh.Echo(Text, Ct); }
	public static partial void Cd(str Path){ Sh.Cd(Path); }
	public static partial Command Cmd(str Command){ return Sh.Cmd(Command); }
	public static partial Command Cmd(str Command, CommandOptions Options){ return Sh.Cmd(Command, Options); }
	public static partial Command Cmd(str Command, in CT Ct){ return Sh.Cmd(Command, Ct); }
	public static partial Command Cmd(str Command, CommandOptions Options, CT Ct){ return Sh.Cmd(Command, Options, Ct); }
	public static partial Command Cmd(str Exe, IList<str> Args){ return Sh.Cmd(Exe, Args); }
	public static partial Command Cmd(str Exe, IList<str> Args, CommandOptions Options){ return Sh.Cmd(Exe, Args, Options); }
	public static partial Command Cmd(str Exe, IList<str> Args, CT Ct){ return Sh.Cmd(Exe, Args, Ct); }
	public static partial Command Cmd(str Exe, IList<str> Args, CommandOptions Options, CT Ct){ return Sh.Cmd(Exe, Args, Options, Ct); }
	public static partial Command TryCmd(str Command){ return Sh.TryCmd(Command); }
	public static partial Command TryCmd(str Command, CommandOptions Options){ return Sh.TryCmd(Command, Options); }
	public static partial Command TryCmd(str Command, in CT Ct){ return Sh.TryCmd(Command, Ct); }
	public static partial Command TryCmd(str Command, CommandOptions Options, CT Ct){ return Sh.TryCmd(Command, Options, Ct); }
	public static partial Command TryCmd(str Exe, IList<str> Args){ return Sh.TryCmd(Exe, Args); }
	public static partial Command TryCmd(str Exe, IList<str> Args, CommandOptions Options){ return Sh.TryCmd(Exe, Args, Options); }
	public static partial Command TryCmd(str Exe, IList<str> Args, CT Ct){ return Sh.TryCmd(Exe, Args, Ct); }
	public static partial Command TryCmd(str Exe, IList<str> Args, CommandOptions Options, CT Ct){ return Sh.TryCmd(Exe, Args, Options, Ct); }
	public static partial CommandExit Exe(str Command){ return Sh.Exe(Command); }
	public static partial CommandExit Exe(str Command, CommandOptions Options){ return Sh.Exe(Command, Options); }
	public static partial Task<CommandExit> Exe(str Command, CT Ct){ return Sh.Exe(Command, Ct); }
	public static partial Task<CommandExit> Exe(str Command, CommandOptions Options, CT Ct){ return Sh.Exe(Command, Options, Ct); }
	public static partial CommandExit Exe(str Exe, IList<str> Args){ return Sh.Exe(Exe, Args); }
	public static partial CommandExit Exe(str Exe, IList<str> Args, CommandOptions Options){ return Sh.Exe(Exe, Args, Options); }
	public static partial Task<CommandExit> Exe(str Exe, IList<str> Args, CT Ct){ return Sh.Exe(Exe, Args, Ct); }
	public static partial Task<CommandExit> Exe(str Exe, IList<str> Args, CommandOptions Options, CT Ct){ return Sh.Exe(Exe, Args, Options, Ct); }
	public static partial CommandExit TryExe(str Command){ return Sh.TryExe(Command); }
	public static partial CommandExit TryExe(str Command, CommandOptions Options){ return Sh.TryExe(Command, Options); }
	public static partial Task<CommandExit> TryExe(str Command, CT Ct){ return Sh.TryExe(Command, Ct); }
	public static partial Task<CommandExit> TryExe(str Command, CommandOptions Options, CT Ct){ return Sh.TryExe(Command, Options, Ct); }
	public static partial CommandExit TryExe(str Exe, IList<str> Args){ return Sh.TryExe(Exe, Args); }
	public static partial CommandExit TryExe(str Exe, IList<str> Args, CommandOptions Options){ return Sh.TryExe(Exe, Args, Options); }
	public static partial Task<CommandExit> TryExe(str Exe, IList<str> Args, CT Ct){ return Sh.TryExe(Exe, Args, Ct); }
	public static partial Task<CommandExit> TryExe(str Exe, IList<str> Args, CommandOptions Options, CT Ct){ return Sh.TryExe(Exe, Args, Options, Ct); }
	public static partial str Q(str Value){ return Sh.Q(Value); }
	public static partial void Write(Content Target, Content Source){ Sh.Write(Target, Source); }
	public static partial Task<nil> Write(Content Target, Content Source, CT Ct){ return Sh.Write(Target, Source, Ct); }
	public static partial void Write(Content Target, IReadOnlyList<Content> Sources){ Sh.Write(Target, Sources); }
	public static partial Task<nil> Write(Content Target, IReadOnlyList<Content> Sources, CT Ct){ return Sh.Write(Target, Sources, Ct); }
	public static partial str? GetEnv(str Name){ return Sh.GetEnv(Name); }
	public static partial void SetEnv(str Name, str Value){ Sh.SetEnv(Name, Value); }
	public static partial void UnsetEnv(str Name){ Sh.UnsetEnv(Name); }
	public static partial bool Exists(str Path){ return Sh.Exists(Path); }
	public static partial Task<bool> Exists(str Path, CT Ct){ return Sh.Exists(Path, Ct); }
	public static partial void Mkdir(str Path){ Sh.Mkdir(Path); }
	public static partial Task<nil> Mkdir(str Path, CT Ct){ return Sh.Mkdir(Path, Ct); }
	public static partial void Rm(str Path){ Sh.Rm(Path); }
	public static partial Task<nil> Rm(str Path, CT Ct){ return Sh.Rm(Path, Ct); }
	public static partial void Cp(str Source, str Destination, CpOptions? Options){ Sh.Cp(Source, Destination, Options); }
	public static partial Task<nil> Cp(str Source, str Destination, CT Ct){ return Sh.Cp(Source, Destination, Ct); }
	public static partial Task<nil> Cp(str Source, str Destination, CpOptions? Options, CT Ct){ return Sh.Cp(Source, Destination, Options, Ct); }
	public static partial void Mv(str Source, str Destination, MvOptions? Options){ Sh.Mv(Source, Destination, Options); }
	public static partial Task<nil> Mv(str Source, str Destination, CT Ct){ return Sh.Mv(Source, Destination, Ct); }
	public static partial Task<nil> Mv(str Source, str Destination, MvOptions? Options, CT Ct){ return Sh.Mv(Source, Destination, Options, Ct); }
	public static partial IEnumerable<FileSystemInfo> Find(str Pattern){ return Sh.Find(Pattern); }
	public static partial IAsyncEnumerable<FileSystemInfo> Find(str Pattern, CT Ct){ return Sh.Find(Pattern, Ct); }
	public static partial IEnumerable<FileSystemInfo> Ls(str? Path, LsOptions? Options){ return Sh.Ls(Path, Options); }
	public static partial IAsyncEnumerable<FileSystemInfo> Ls(str? Path, CT Ct){ return Sh.Ls(Path, Ct); }
	public static partial IAsyncEnumerable<FileSystemInfo> Ls(str? Path, LsOptions? Options, CT Ct){ return Sh.Ls(Path, Options, Ct); }
	public static partial Content Read(str Path){ return Sh.Read(Path); }
	public static partial Task<Content> Read(str Path, CT Ct){ return Sh.Read(Path, Ct); }
	public static partial void Write(str Path, Content Source){ Sh.Write(Path, Source); }
	public static partial Task<nil> Write(str Path, Content Source, CT Ct){ return Sh.Write(Path, Source, Ct); }
	public static partial void Append(str Path, Content Source){ Sh.Append(Path, Source); }
	public static partial Task<nil> Append(str Path, Content Source, CT Ct){ return Sh.Append(Path, Source, Ct); }
}
