namespace Tsinswreng.CsSh;

/// 全局默认 Shell 实例的静态 facade，供 csx 使用 using static 简化调用。
public static partial class ShGlobal{
	public static Sh Sh{get;set;} = new();
	public static partial Pth Pwd();
	public static partial Pth CsxDir();
	public static partial void Echo(str Text);
	public static partial Task<nil> Echo(str Text, CT Ct);
	public static partial void Cd(Pth Path);
	public static partial Command Cmd(str Exe, IList<str> Args);
	public static partial Command Cmd(str Exe, IList<str> Args, CommandOptions Options);
	public static partial Command Cmd(str Exe, IList<str> Args, CT Ct);
	public static partial Command Cmd(str Exe, IList<str> Args, CommandOptions Options, CT Ct);
	public static partial Command TryCmd(str Exe, IList<str> Args);
	public static partial Command TryCmd(str Exe, IList<str> Args, CommandOptions Options);
	public static partial Command TryCmd(str Exe, IList<str> Args, CT Ct);
	public static partial Command TryCmd(str Exe, IList<str> Args, CommandOptions Options, CT Ct);
	public static partial CommandExit Exe(str Exe, IList<str> Args);
	public static partial CommandExit Exe(str Exe, IList<str> Args, CommandOptions Options);
	public static partial Task<CommandExit> Exe(str Exe, IList<str> Args, CT Ct);
	public static partial Task<CommandExit> Exe(str Exe, IList<str> Args, CommandOptions Options, CT Ct);
	public static partial CommandExit TryExe(str Exe, IList<str> Args);
	public static partial CommandExit TryExe(str Exe, IList<str> Args, CommandOptions Options);
	public static partial Task<CommandExit> TryExe(str Exe, IList<str> Args, CT Ct);
	public static partial Task<CommandExit> TryExe(str Exe, IList<str> Args, CommandOptions Options, CT Ct);
	public static partial str Q(str Value);
	public static partial void Write(Content Target, Content Source);
	public static partial Task<nil> Write(Content Target, Content Source, CT Ct);
	public static partial void Write(Content Target, IReadOnlyList<Content> Sources);
	public static partial Task<nil> Write(Content Target, IReadOnlyList<Content> Sources, CT Ct);
	public static partial str? GetEnv(str Name);
	public static partial void SetEnv(str Name, str Value);
	public static partial void UnsetEnv(str Name);
	public static partial bool Exists(Pth Path);
	public static partial Task<bool> Exists(Pth Path, CT Ct);
	public static partial FileSystemInfo? FsInfo(Pth Path);
	public static partial Task<FileSystemInfo?> FsInfo(Pth Path, CT Ct);
	public static partial bool IsFile(Pth Path);
	public static partial Task<bool> IsFile(Pth Path, CT Ct);
	public static partial bool IsDir(Pth Path);
	public static partial Task<bool> IsDir(Pth Path, CT Ct);
	public static partial void Mkdir(Pth Path);
	public static partial Task<nil> Mkdir(Pth Path, CT Ct);
	public static partial void Rm(Pth Path);
	public static partial Task<nil> Rm(Pth Path, CT Ct);
	public static partial void Cp(Pth Source, Pth Destination, CpOptions? Options = null);
	public static partial Task<nil> Cp(Pth Source, Pth Destination, CT Ct);
	public static partial Task<nil> Cp(Pth Source, Pth Destination, CpOptions? Options, CT Ct);
	public static partial void Mv(Pth Source, Pth Destination, MvOptions? Options = null);
	public static partial Task<nil> Mv(Pth Source, Pth Destination, CT Ct);
	public static partial Task<nil> Mv(Pth Source, Pth Destination, MvOptions? Options, CT Ct);
	public static partial Pth BaseName(Pth Path);
	public static partial Pth DirName(Pth Path);
	public static partial Pth FullPath(Pth Path);
	/// 在預設 Sh 的目前工作目錄下惰性列舉 Glob 模式；不以 / 結尾匹配檔案，以 / 結尾匹配目錄。
	/// 例如 foreach (var FilePath in Glob("src/**/*.cs")) { Console.WriteLine(FilePath); }。
	public static partial IEnumerable<Pth> Glob(Pth Pattern);
	/// 在預設 Sh 的目前工作目錄下惰性列舉檔案與目錄路徑；Path 為 null 時列舉目前工作目錄。
	/// 例如 foreach (var EntryPath in Ls("artifacts")) { Console.WriteLine(EntryPath); }。
	public static partial IEnumerable<Pth> Ls(Pth? Path = null, LsOptions? Options = null);
	/// 在預設 Sh 的目前工作目錄下惰性列舉目錄路徑；傳入 new LsOptions(Recursive: true) 可遞迴。
	/// 例如 foreach (var DirectoryPath in LsDir("src")) { Console.WriteLine(DirectoryPath); }。
	public static partial IEnumerable<Pth> LsDir(Pth? Path = null, LsOptions? Options = null);
	/// 在預設 Sh 的目前工作目錄下惰性列舉檔案路徑；傳入 new LsOptions(Recursive: true) 可遞迴。
	/// 例如 foreach (var FilePath in LsFile("src")) { Console.WriteLine(FilePath); }。
	public static partial IEnumerable<Pth> LsFile(Pth? Path = null, LsOptions? Options = null);
	public static partial Content Read(Pth Path);
	public static partial Task<Content> Read(Pth Path, CT Ct);
	public static partial void Write(Pth Path, Content Source);
	public static partial void Write(str Path, Content Source);
	public static partial Task<nil> Write(Pth Path, Content Source, CT Ct);
	public static partial Task<nil> Write(str Path, Content Source, CT Ct);
	public static partial void Append(Pth Path, Content Source);
	public static partial void Append(str Path, Content Source);
	public static partial Task<nil> Append(Pth Path, Content Source, CT Ct);
	public static partial Task<nil> Append(str Path, Content Source, CT Ct);
}
