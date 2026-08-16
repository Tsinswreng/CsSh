using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace Tsinswreng.CsSh;

public sealed partial class Command{
	private readonly CommandRunOptions Options;
	private readonly Pipe StdoutPipe = new(new PipeOptions(pauseWriterThreshold: long.MaxValue, resumeWriterThreshold: long.MaxValue - 1));
	private readonly Pipe StderrPipe = new(new PipeOptions(pauseWriterThreshold: long.MaxValue, resumeWriterThreshold: long.MaxValue - 1));
	private readonly TaskCompletionSource<CommandExit> ExitSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
	private readonly object Gate = new();
	private Task? StartTask;
	private Process? Process;
	private bool IsDisposed;

	internal partial Command(CommandRunOptions Options) {
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
		return Out(Sh.Stdout, Sh.Stderr, Ct);
	}

	public partial Task<CommandExit> Out(Content Target, CT Ct) {
		return Out([Target, Target], Ct);
	}

	public async partial Task<CommandExit> Out(str TargetPath, CT Ct) {
		var FileSystemPath = Sh.NormalizeFileSystemPath(TargetPath);
		var Parent = Path.GetDirectoryName(FileSystemPath);
		if (!string.IsNullOrEmpty(Parent))
			Directory.CreateDirectory(Parent);
		await using var Target = new Content(
			new FileStream(FileSystemPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true),
			new(LeaveOpen: false));
		return await Out(Target, Ct).ConfigureAwait(false);
	}

	public async partial Task<CommandExit> Out(Content Stdout, Content Stderr, CT Ct) {
		await Task.WhenAll(
			Sh.Write(Stdout, Result.Stdout, Ct),
			Sh.Write(Stderr, Result.Stderr, Ct),
			Done).ConfigureAwait(false);
		return await Done.ConfigureAwait(false);
	}

	/// Keeps a single output target safe from concurrent stdout/stderr writes.
	private async Task<CommandExit> Out(IReadOnlyList<Content> Targets, CT Ct) {
		await Task.WhenAll(
			Sh.Write(Targets[0], [Result.Stdout, Result.Stderr], Ct),
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

	internal void EnsureStarted() {
		lock (Gate) {
			if (IsDisposed)
				return;
			StartTask ??= Start();
		}
	}

	private async Task Start() {
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

	private ProcessStartInfo MakeStartInfo() {
		var FirstSpace = Options.Text.IndexOfAny([' ', '\t']);
		var Program = FirstSpace < 0 ? Options.Text : Options.Text[..FirstSpace];
		var Arguments = FirstSpace < 0 ? "" : Options.Text[(FirstSpace + 1)..].TrimStart();
		if (string.IsNullOrWhiteSpace(Program))
			throw new ArgumentException("Command cannot be empty.", nameof(Options));

		return new(){
			FileName = Program,
			Arguments = Arguments,
			WorkingDirectory = Options.Options.Cwd is null ? Environment.CurrentDirectory : Sh.NormalizeFileSystemPath(Options.Options.Cwd),
			UseShellExecute = false,
			RedirectStandardInput = Options.Options.Input is not null,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
	}

	private async Task CopyInput(Process Process) {
		if (Options.Options.Input is null)
			return;
		await Options.Options.Input.Stream.CopyToAsync(Process.StandardInput.BaseStream, Options.Ct).ConfigureAwait(false);
		await Process.StandardInput.BaseStream.DisposeAsync().ConfigureAwait(false);
	}

	private async Task CompletePipes(Exception? Error = null) {
		await StdoutPipe.Writer.CompleteAsync(Error).ConfigureAwait(false);
		await StderrPipe.Writer.CompleteAsync(Error).ConfigureAwait(false);
	}

	private void TryKill() {
		try {
			if (Process is {HasExited: false})
				Process.Kill(entireProcessTree: true);
		}
		catch (InvalidOperationException) {
			// The process completed between the status check and Kill.
		}
	}
}

/// 讀取前啟動所屬 Command 的只讀 Stream 包裝。
/// 包裝不緩衝資料，讀取會直接進入 PipeReader 對應的非同步 Stream。
internal sealed class CommandReadStream:Stream{
	private readonly Stream Inner;
	private readonly Action Start;

	internal CommandReadStream(Stream Inner, Action Start) {
		this.Inner = Inner;
		this.Start = Start;
	}

	public override bool CanRead => Inner.CanRead;
	public override bool CanSeek => false;
	public override bool CanWrite => false;
	public override long Length => throw new NotSupportedException();
	public override long Position{get => throw new NotSupportedException(); set => throw new NotSupportedException();}
	public override void Flush() {
		throw new NotSupportedException();
	}
	public override Task FlushAsync(CT Ct) {
		throw new NotSupportedException();
	}
	public override int Read(byte[] Buffer, int Offset, int Count) {
		Start();
		return Inner.Read(Buffer, Offset, Count);
	}
	public override int Read(Span<byte> Buffer) {
		Start();
		return Inner.Read(Buffer);
	}
	public override Task<i32> ReadAsync(byte[] Buffer, int Offset, int Count, CT Ct) {
		Start();
		return Inner.ReadAsync(Buffer, Offset, Count, Ct);
	}
	public override ValueTask<i32> ReadAsync(Memory<byte> Buffer, CT Ct = default) {
		Start();
		return Inner.ReadAsync(Buffer, Ct);
	}
	public override long Seek(long Offset, SeekOrigin Origin) {
		throw new NotSupportedException();
	}
	public override void SetLength(long Value) {
		throw new NotSupportedException();
	}
	public override void Write(byte[] Buffer, int Offset, int Count) {
		throw new NotSupportedException();
	}
}

public sealed partial class CommandFailedException{
	public partial CommandFailedException(CommandExit Exit):base($"Command failed with exit code {Exit.ExitCode}.") {
		this.Exit = Exit;
	}
}

