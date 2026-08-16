namespace Tsinswreng.Cssh;

public static partial class Sh{
	public static partial str Pwd() {
		return NormalizePath(Environment.CurrentDirectory);
	}

	public static partial str ScriptDir() {
		var ScriptPath = Environment.GetCommandLineArgs()
			.FirstOrDefault(Argument => Argument.EndsWith(".csx", StringComparison.OrdinalIgnoreCase));
		if (ScriptPath is null)
			return Pwd();
		return NormalizePath(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(ScriptPath))!);
	}

	public static partial IReadOnlyList<str> Args() {
		return Environment.GetCommandLineArgs().Skip(1).ToArray();
	}

	public static partial void Echo(str Text) {
		Console.Out.WriteLine(Text);
	}

	public static partial async Task<nil> Echo(str Text, CT Ct) {
		var Bytes = System.Text.Encoding.UTF8.GetBytes(Text + Environment.NewLine);
		await Stdout.WriteAsync(Bytes, Ct).ConfigureAwait(false);
		await Stdout.FlushAsync(Ct).ConfigureAwait(false);
		return NIL;
	}

	public static partial void Cd(str Path) {
		Environment.CurrentDirectory = NormalizeFileSystemPath(Path);
	}

	public static partial Command X(str Command) {
		return X(Command, new(), CancellationToken.None);
	}

	public static partial Command X(str Command, CommandOptions Options) {
		return X(Command, Options, CancellationToken.None);
	}

	public static partial Command X(str Command, in CT Ct) {
		return X(Command, new(), Ct);
	}

	public static partial Command X(str Command, CommandOptions Options, CT Ct) {
		return new(new(Command, Options, Ct, true));
	}

	public static partial Command TryX(str Command) {
		return TryX(Command, new(), CancellationToken.None);
	}

	public static partial Command TryX(str Command, CommandOptions Options) {
		return TryX(Command, Options, CancellationToken.None);
	}

	public static partial Command TryX(str Command, in CT Ct) {
		return TryX(Command, new(), Ct);
	}

	public static partial Command TryX(str Command, CommandOptions Options, CT Ct) {
		return new(new(Command, Options, Ct, false));
	}

	public static partial void Write(Stream Target, Stream Source) {
		Source.CopyTo(Target);
		Target.Flush();
	}

	public static partial async Task<nil> Write(Stream Target, Stream Source, CT Ct) {
		await Source.CopyToAsync(Target, Ct).ConfigureAwait(false);
		await Target.FlushAsync(Ct).ConfigureAwait(false);
		return NIL;
	}

	public static partial void Write(Stream Target, IReadOnlyList<Stream> Sources) {
		foreach (var Source in Sources)
			Write(Target, Source);
	}

	public static partial async Task<nil> Write(Stream Target, IReadOnlyList<Stream> Sources, CT Ct) {
		foreach (var Source in Sources) {
			await Write(Target, Source, Ct).ConfigureAwait(false);
		}
		return NIL;
	}

	internal static str NormalizePath(str Path) {
		return Path.Replace('\\', '/');
	}

	internal static str NormalizeFileSystemPath(str Path) {
		return Path.Replace('/', System.IO.Path.DirectorySeparatorChar);
	}
}
