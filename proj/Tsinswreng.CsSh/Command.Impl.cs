using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace Tsinswreng.CsSh;

public sealed partial class Command{


	public partial Command(CommandRunOptions Options) {
		this.Options = Options;
		Result = new(
			new(new CommandReadStream(StdoutPipe.Reader.AsStream(), EnsureStarted), new(LeaveOpen: false)),
			new(new CommandReadStream(StderrPipe.Reader.AsStream(), EnsureStarted), new(LeaveOpen: false)));
		ExitTask = ExitSource.Task;
	}

	public partial TaskAwaiter<CommandExit> GetAwaiter() {
		EnsureStarted();
		return Done.GetAwaiter();
	}

	public partial Task<CommandExit> Out(CT Ct) {
		return Out(Options.Stdout, Options.Stderr, Ct);
	}

	public partial Task<CommandExit> Out(Content Target, CT Ct) {
		return Out([Target, Target], Ct);
	}

	public async partial Task<CommandExit> Out(Pth TargetPath, CT Ct) {
		str TargetPathValue = TargetPath;
		var FileSystemPath = System.IO.Path.IsPathRooted(TargetPathValue)
			? TargetPathValue
			: System.IO.Path.Combine((str)Options.Cwd, TargetPathValue);
		var Parent = System.IO.Path.GetDirectoryName(FileSystemPath);
		if (!string.IsNullOrEmpty(Parent))
			Directory.CreateDirectory(Parent);
		await using var Target = new Content(
			new FileStream(FileSystemPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true),
			new(LeaveOpen: false));
		return await Out(Target, Ct).ConfigureAwait(false);
	}

	public async partial Task<CommandExit> Out(Content Stdout, Content Stderr, CT Ct) {
		await Task.WhenAll(
			Write(Stdout, Result.Stdout, Ct),
			Write(Stderr, Result.Stderr, Ct),
			Done).ConfigureAwait(false);
		return await Done.ConfigureAwait(false);
	}

	public async partial Task<CommandTextResult> Text(CT Ct) {
		// Start both readers before waiting: either pipe can otherwise block a verbose child process.
		var Stdout = Result.Stdout.Text(Ct);
		var Stderr = Result.Stderr.Text(Ct);
		await Task.WhenAll(Stdout, Stderr, Done).ConfigureAwait(false);
		return new(await Stdout.ConfigureAwait(false), await Stderr.ConfigureAwait(false), await Done.ConfigureAwait(false));
	}

	/// Keeps a single output target safe from concurrent stdout/stderr writes.
	private async partial Task<CommandExit> Out(IReadOnlyList<Content> Targets, CT Ct) {
		await Task.WhenAll(
			Write(Targets[0], [Result.Stdout, Result.Stderr], Ct),
			Done).ConfigureAwait(false);
		return await Done.ConfigureAwait(false);
	}

	public partial void Dispose() {
		DisposeAsync().AsTask().GetAwaiter().GetResult();
	}

	public partial async ValueTask DisposeAsync() {
		Task? RunningTask;
		lock (Gate) {
			IsDisposed = true;
			RunningTask = StartTask;
		}
		if (RunningTask is null) {
			// Disposing an unobserved lazy command must not unexpectedly execute it.
			await CompletePipes().ConfigureAwait(false);
			ExitSource.TrySetCanceled();
			return;
		}
		TryKill();
		try {
			await RunningTask.ConfigureAwait(false);
		}
		catch {
			// Disposing a failed command only releases resources; the caller observes its failure through Done.
		}
		await Result.Stdout.DisposeAsync().ConfigureAwait(false);
		await Result.Stderr.DisposeAsync().ConfigureAwait(false);
	}

	internal partial void EnsureStarted() {
		lock (Gate) {
			if (IsDisposed)
				return;
			StartTask ??= Start();
		}
	}

	private async partial Task Start() {
		try {
			var StartInfo = MakeStartInfo();
			Process = new(){StartInfo = StartInfo, EnableRaisingEvents = true};
			var Elapsed = Stopwatch.StartNew();
			if (!Process.Start())
				throw new InvalidOperationException("Failed to start command process.");

			using var Registration = Options.Ct.Register(() => TryKill());
			var InputTask = CopyInput(Process);
			var OutputTask = Process.StandardOutput.BaseStream.CopyToAsync(StdoutPipe.Writer.AsStream(), Options.Ct);
			var ErrorTask = Process.StandardError.BaseStream.CopyToAsync(StderrPipe.Writer.AsStream(), Options.Ct);
			await Task.WhenAll(InputTask, OutputTask, ErrorTask, Process.WaitForExitAsync(Options.Ct)).ConfigureAwait(false);
			Elapsed.Stop();

			var Exit = new CommandExit(Process.ExitCode, Elapsed.Elapsed, Process.ExitCode == 0);
			await CompletePipes().ConfigureAwait(false);
			if (!Exit.IsSuccess && Options.ThrowOnError) {
				ExitSource.SetException(new CommandFailedException(Exit));
			}
			else {
				ExitSource.SetResult(Exit);
			}
		}
		catch (Exception Error) {
			await CompletePipes(Error).ConfigureAwait(false);
			ExitSource.TrySetException(Error);
		}
	}

	private partial ProcessStartInfo MakeStartInfo() {
		if (string.IsNullOrWhiteSpace(Options.Exe))
			throw new ArgumentException("Command cannot be empty.", nameof(Options));

		var Result = new ProcessStartInfo{
			FileName = Options.Exe,
			WorkingDirectory = Options.Cwd,
			UseShellExecute = false,
			RedirectStandardInput = Options.Options.Input is not null,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		Result.Environment.Clear();
		foreach (var Pair in Options.Environment)
			Result.Environment[Pair.Key] = Pair.Value;
		foreach (var Arg in Options.Args)
			Result.ArgumentList.Add(Arg);
		return Result;
	}

	private async partial Task CopyInput(Process Process) {
		if (Options.Options.Input is null)
			return;
		await Options.Options.Input.Stream.CopyToAsync(Process.StandardInput.BaseStream, Options.Ct).ConfigureAwait(false);
		await Process.StandardInput.BaseStream.DisposeAsync().ConfigureAwait(false);
	}

	/// Copies one Content source to its target without requiring a Shell instance.
	private static async partial Task Write(Content Target, Content Source, CT Ct) {
		await Source.Stream.CopyToAsync(Target.Stream, Ct).ConfigureAwait(false);
		await Target.Stream.FlushAsync(Ct).ConfigureAwait(false);
	}

	/// Serializes multiple sources when stdout and stderr share the same target.
	private static async partial Task Write(Content Target, IReadOnlyList<Content> Sources, CT Ct) {
		foreach (var Source in Sources)
			await Write(Target, Source, Ct).ConfigureAwait(false);
	}

	private async partial Task CompletePipes(Exception? Error) {
		await StdoutPipe.Writer.CompleteAsync(Error).ConfigureAwait(false);
		await StderrPipe.Writer.CompleteAsync(Error).ConfigureAwait(false);
	}

	private partial void TryKill() {
		try {
			if (Process is {HasExited: false})
				Process.Kill(entireProcessTree: true);
		}
		catch (InvalidOperationException) {
			// The process completed between the status check and Kill.
		}
	}
}

public sealed partial class CommandFailedException{
	public partial CommandFailedException(CommandExit Exit):base($"Command failed with exit code {Exit.ExitCode}.") {
		this.Exit = Exit;
	}
}
