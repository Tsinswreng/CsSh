using System.Text;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for successful lazy command execution.
public partial class TestCssh{
	public partial void RegisterX(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.Cmd), nameof(ShGlobal.Exe)], "Command").Register;
		Register(nameof(XStartsWhenDoneIsObservedAndReturnsStdout), XStartsWhenDoneIsObservedAndReturnsStdout!);
		Register(nameof(XPassesContentAsStdin), XPassesContentAsStdin!);
		Register(nameof(OutWritesCommandOutput), OutWritesCommandOutput!);
		Register(nameof(OutWritesCommandOutputToPath), OutWritesCommandOutputToPath!);
		Register(nameof(ExeWritesDefaultOutput), ExeWritesDefaultOutput!);
		Register(nameof(CmdArgumentListNeedsNoEscaping), CmdArgumentListNeedsNoEscaping!);
		Register(nameof(QQuotesCommandStringArgument), QQuotesCommandStringArgument!);
	}

	/// Observing Done starts the lazy process; stdout remains consumable after it exits.
	public async partial Task<object?> XStartsWhenDoneIsObservedAndReturnsStdout(object? O) {
		await using var Command = ShGlobal.Cmd("dotnet", ["--version"]);
		var Exit = await Command.Done;
		using var Reader = new StreamReader(Command.Result.Stdout, Encoding.UTF8, leaveOpen: true);
		var Text = await Reader.ReadToEndAsync();
		Assert.IsTrue(Exit.IsSuccess);
		Assert.IsTrue(!string.IsNullOrWhiteSpace(Text));
		return null;
	}

	/// Command input is configured externally as Content rather than becoming a Command property.
	public async partial Task<object?> XPassesContentAsStdin(object? O) {
		using var CtSource = new CancellationTokenSource();
		var Ct = CtSource.Token;
		Content Input = "stream-input";
		await using var Command = ShGlobal.Cmd("dotnet", ["--version"], new CommandOptions(Input), Ct);
		var Exit = await Command.Done;
		Assert.IsTrue(Exit.IsSuccess);
		return null;
	}

	/// Out starts the command, drains its streams, and leaves an externally owned target readable.
	public async partial Task<object?> OutWritesCommandOutput(object? O) {
		using var CtSource = new CancellationTokenSource();
		var Ct = CtSource.Token;
		await using var Buffer = new MemoryStream();
		await using var Output = new Content(Buffer, new(LeaveOpen: true));
		await using var Command = ShGlobal.Cmd("dotnet", ["--version"], Ct);

		var Exit = await Command.Out(Output, Ct);
		Buffer.Position = 0;
		var Text = await Output.Text(Ct);
		Assert.IsTrue(Exit.IsSuccess);
		Assert.IsTrue(!string.IsNullOrWhiteSpace(Text));
		return null;
	}

	/// A file path is the concise Out target for script logs, without manually opening a FileStream.
	public async partial Task<object?> OutWritesCommandOutputToPath(object? O) {
		var Root = TestSupport.NewRoot();
		using var CtSource = new CancellationTokenSource();
		var Ct = CtSource.Token;
		try {
			var Path = Root / "logs/dotnet-version.txt";
			await using var Command = ShGlobal.Cmd("dotnet", ["--version"], Ct);
			Assert.IsTrue((await Command.Out(Path, Ct)).IsSuccess);
			await using var Output = await ShGlobal.Read(Path, Ct);
			Assert.IsTrue(!string.IsNullOrWhiteSpace(await Output.Text(Ct)));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}

	/// Exe consumes its Cmd internally so a normal script command cannot be forgotten unexecuted.
	public async partial Task<object?> ExeWritesDefaultOutput(object? O) {
		using var CtSource = new CancellationTokenSource();
		var Exit = await ShGlobal.Exe("dotnet", ["--version"], CtSource.Token);
		Assert.IsTrue(Exit.IsSuccess);
		return null;
	}

	/// ArgumentList bypasses the raw command-line parser, so one list element always arrives as one argument.
	public async partial Task<object?> CmdArgumentListNeedsNoEscaping(object? O) {
		using var CtSource = new CancellationTokenSource();
		var Ct = CtSource.Token;
		await using var Command = ShGlobal.Cmd("dotnet", ["--version"], Ct);
		Assert.IsTrue((await Command.Done).IsSuccess);
		return null;
	}

	/// Q wraps a raw string command argument with escaping for quotes and backslashes.
	public partial Task<object?> QQuotesCommandStringArgument(object? O) {
		var Value = "a b\\c\"d";
		Assert.IsTrue(ShGlobal.Q(Value) == "\"a b\\c\\\"d\"");
		Assert.IsTrue(ShGlobal.Q("tail\\") == "\"tail\\\\\"");
		return Task.FromResult<object?>(null);
	}
}

