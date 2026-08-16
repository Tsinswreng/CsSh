namespace Tsinswreng.CsSh;

public partial class Sh{
	private string CurrentDirectory;

	public partial Sh() {
		CurrentDirectory = Path.GetFullPath(Environment.CurrentDirectory).Replace('\\', '/');
		Stdin = new(Console.OpenStandardInput());
		Stdout = new(Console.OpenStandardOutput());
		Stderr = new(Console.OpenStandardError());
		Null = new(Stream.Null);
	}

	public partial str Pwd() {
		return NormalizePath(CurrentDirectory);
	}

	public partial str CsxDir() {
		var ScriptPath = Environment.GetCommandLineArgs()
			.FirstOrDefault(Argument => Argument.EndsWith(".csx", StringComparison.OrdinalIgnoreCase));
		if (ScriptPath is null)
			return Pwd();
		return NormalizePath(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(ScriptPath))!);
	}

	public partial IReadOnlyList<str> Args() {
		return Environment.GetCommandLineArgs().Skip(1).ToArray();
	}

	public partial void Echo(str Text) {
		Console.Out.WriteLine(Text);
	}

	public partial async Task<nil> Echo(str Text, CT Ct) {
		var Bytes = System.Text.Encoding.UTF8.GetBytes(Text + Environment.NewLine);
		await Stdout.Stream.WriteAsync(Bytes, Ct).ConfigureAwait(false);
		await Stdout.Stream.FlushAsync(Ct).ConfigureAwait(false);
		return NIL;
	}

	public partial void Cd(str Path) {
		var Candidate = Path.Replace('\\', '/');
		if (!System.IO.Path.IsPathRooted(Candidate))
			Candidate = System.IO.Path.Combine(CurrentDirectory, Candidate);
		CurrentDirectory = System.IO.Path.GetFullPath(Candidate).Replace('\\', '/');
	}

	public partial Command Cmd(str Command) {
		return Cmd(Command, new(), CancellationToken.None);
	}

	public partial Command Cmd(str Command, CommandOptions Options) {
		return Cmd(Command, Options, CancellationToken.None);
	}

	public partial Command Cmd(str Command, in CT Ct) {
		return Cmd(Command, new(), Ct);
	}

	public partial Command Cmd(str Command, CommandOptions Options, CT Ct) {
		var Cwd = NormalizeFileSystemPath(Options.Cwd ?? CurrentDirectory);
		return new(new(Command, Options, Cwd, Stdout, Stderr, Ct, true));
	}

	public partial Command TryCmd(str Command) {
		return TryCmd(Command, new(), CancellationToken.None);
	}

	public partial Command TryCmd(str Command, CommandOptions Options) {
		return TryCmd(Command, Options, CancellationToken.None);
	}

	public partial Command TryCmd(str Command, in CT Ct) {
		return TryCmd(Command, new(), Ct);
	}

	public partial Command TryCmd(str Command, CommandOptions Options, CT Ct) {
		var Cwd = NormalizeFileSystemPath(Options.Cwd ?? CurrentDirectory);
		return new(new(Command, Options, Cwd, Stdout, Stderr, Ct, false));
	}

	public partial void Write(Content Target, Content Source) {
		Source.Stream.CopyTo(Target.Stream);
		Target.Stream.Flush();
	}

	public partial async Task<nil> Write(Content Target, Content Source, CT Ct) {
		await Source.Stream.CopyToAsync(Target.Stream, Ct).ConfigureAwait(false);
		await Target.Stream.FlushAsync(Ct).ConfigureAwait(false);
		return NIL;
	}

	public partial void Write(Content Target, IReadOnlyList<Content> Sources) {
		foreach (var Source in Sources)
			Write(Target, Source);
	}

	public partial async Task<nil> Write(Content Target, IReadOnlyList<Content> Sources, CT Ct) {
		foreach (var Source in Sources) {
			await Write(Target, Source, Ct).ConfigureAwait(false);
		}
		return NIL;
	}

	internal static str NormalizePath(str Path) {
		return Path.Replace('\\', '/');
	}

	internal str NormalizeFileSystemPath(str Path) {
		var Candidate = Path.Replace('/', System.IO.Path.DirectorySeparatorChar);
		if (!System.IO.Path.IsPathRooted(Candidate))
			Candidate = System.IO.Path.Combine(CurrentDirectory, Candidate);
		return Candidate;
	}
}

