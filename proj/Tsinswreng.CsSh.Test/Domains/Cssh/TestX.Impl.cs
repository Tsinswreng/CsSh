using System.Text;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for successful lazy command execution.
public partial class TestCssh{
	public partial void RegisterX(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.X)], "Command").Register;
		Register(nameof(XStartsWhenDoneIsObservedAndReturnsStdout), XStartsWhenDoneIsObservedAndReturnsStdout!);
		Register(nameof(XPassesContentAsStdin), XPassesContentAsStdin!);
	}

	/// Observing Done starts the lazy process; stdout remains consumable after it exits.
	public async partial Task<object?> XStartsWhenDoneIsObservedAndReturnsStdout(object? O) {
		await using var Command = Sh.X("dotnet --version");
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
		await using var Command = Sh.X("dotnet --version", new(Input), Ct);
		var Exit = await Command.Done;
		Assert.IsTrue(Exit.IsSuccess);
		return null;
	}
}

