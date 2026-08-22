using System.Text;

namespace Tsinswreng.CsSh;

public partial class Sh{
	private Pth CurrentDirectory;

	public partial Sh() {
		CurrentDirectory = System.IO.Path.GetFullPath(Environment.CurrentDirectory).Replace('\\', '/');
		Stdin = new(Console.OpenStandardInput());
		Stdout = new(Console.OpenStandardOutput());
		Stderr = new(Console.OpenStandardError());
		Null = new(Stream.Null);
	}

	public partial Pth Pwd() {
		return NormalizePath(CurrentDirectory);
	}

	public partial Pth CsxDir() {
		var ScriptPath = Environment.GetCommandLineArgs()
			.FirstOrDefault(Argument => Argument.EndsWith(".csx", StringComparison.OrdinalIgnoreCase));
		if (ScriptPath is null)
			return Pwd();
		return NormalizePath(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(ScriptPath))!);
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

	public partial void Cd(Pth Path) {
		str PathValue = Path;
		var Candidate = PathValue.Replace('\\', '/');
		if (!System.IO.Path.IsPathRooted(Candidate))
			Candidate = System.IO.Path.Combine(CurrentDirectory, Candidate);
		CurrentDirectory = System.IO.Path.GetFullPath(Candidate).Replace('\\', '/');
	}

	public partial Command Cmd(str Exe, IList<str> Args) {
		return Cmd(Exe, Args, new CommandOptions(), CancellationToken.None);
	}

	public partial Command Cmd(str Exe, IList<str> Args, CommandOptions Options) {
		return Cmd(Exe, Args, Options, CancellationToken.None);
	}

	public partial Command Cmd(str Exe, IList<str> Args, CT Ct) {
		return Cmd(Exe, Args, new CommandOptions(), Ct);
	}

	public partial Command Cmd(str Exe, IList<str> Args, CommandOptions Options, CT Ct) {
		ArgumentException.ThrowIfNullOrWhiteSpace(Exe);
		ArgumentNullException.ThrowIfNull(Args);
		var Cwd = (str)FullPath(Options.Cwd ?? CurrentDirectory);
		return new(new(Exe, Args.ToArray(), Options, Cwd, SnapshotEnvironment(Options), Stdout, Stderr, Ct, true));
	}

	public partial Command TryCmd(str Exe, IList<str> Args) {
		return TryCmd(Exe, Args, new CommandOptions(), CancellationToken.None);
	}

	public partial Command TryCmd(str Exe, IList<str> Args, CommandOptions Options) {
		return TryCmd(Exe, Args, Options, CancellationToken.None);
	}

	public partial Command TryCmd(str Exe, IList<str> Args, CT Ct) {
		return TryCmd(Exe, Args, new CommandOptions(), Ct);
	}

	public partial Command TryCmd(str Exe, IList<str> Args, CommandOptions Options, CT Ct) {
		ArgumentException.ThrowIfNullOrWhiteSpace(Exe);
		ArgumentNullException.ThrowIfNull(Args);
		var Cwd = (str)FullPath(Options.Cwd ?? CurrentDirectory);
		return new(new(Exe, Args.ToArray(), Options, Cwd, SnapshotEnvironment(Options), Stdout, Stderr, Ct, false));
	}

	public partial CommandExit Exe(str FileName, IList<str> Args) {
		return Exe(FileName, Args, new CommandOptions());
	}

	public partial CommandExit Exe(str FileName, IList<str> Args, CommandOptions Options) {
		return Exe(FileName, Args, Options, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<CommandExit> Exe(str FileName, IList<str> Args, CT Ct) {
		return Exe(FileName, Args, new CommandOptions(), Ct);
	}

	public async partial Task<CommandExit> Exe(str FileName, IList<str> Args, CommandOptions Options, CT Ct) {
		await using var CommandDto = Cmd(FileName, Args, Options, Ct);
		return await CommandDto.Out(Ct).ConfigureAwait(false);
	}


	public partial CommandExit TryExe(str FileName, IList<str> Args) {
		return TryExe(FileName, Args, new CommandOptions());
	}

	public partial CommandExit TryExe(str FileName, IList<str> Args, CommandOptions Options) {
		return TryExe(FileName, Args, Options, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<CommandExit> TryExe(str FileName, IList<str> Args, CT Ct) {
		return TryExe(FileName, Args, new CommandOptions(), Ct);
	}

	public async partial Task<CommandExit> TryExe(str FileName, IList<str> Args, CommandOptions Options, CT Ct) {
		await using var CommandDto = TryCmd(FileName, Args, Options, Ct);
		return await CommandDto.Out(Ct).ConfigureAwait(false);
	}

	public partial str Q(str Value) {
		ArgumentNullException.ThrowIfNull(Value);
		var Result = new StringBuilder("\"");
		var BackslashCount = 0;
		foreach (var Character in Value) {
			if (Character == '\\') {
				BackslashCount++;
				continue;
			}
			if (Character == '"') {
				// A quote needs its preceding backslashes doubled, plus one escaping backslash.
				Result.Append('\\', BackslashCount * 2 + 1);
				Result.Append(Character);
				BackslashCount = 0;
				continue;
			}
			Result.Append('\\', BackslashCount);
			Result.Append(Character);
			BackslashCount = 0;
		}
		// Backslashes before the closing quote must also be doubled.
		Result.Append('\\', BackslashCount * 2);
		Result.Append('"');
		return Result.ToString();
	}

	private partial IReadOnlyDictionary<str, str> SnapshotEnvironment(CommandOptions Options) {
		var Comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
		var Result = new Dictionary<str, str>(Comparer);
		foreach (var Pair in System.Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>()) {
			if (Pair.Key is str Name && Pair.Value is str Value)
				Result[Name] = Value;
		}
		if (Options.Env is not null) {
			foreach (var Pair in Options.Env) {
				if (Pair.Value is null)
					Result.Remove(Pair.Key);
				else
					Result[Pair.Key] = Pair.Value;
			}
		}
		return Result;
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

	public static partial str NormalizePath(str Path) {
		return Path.Replace('\\', '/');
	}
}
