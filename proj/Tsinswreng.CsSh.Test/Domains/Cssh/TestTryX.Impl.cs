using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for non-throwing external command execution.
public partial class TestCssh{
	public partial void RegisterTryX(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.TryX)], "Command").Register;
		Register(nameof(TryXReturnsNonZeroExitWithoutThrowing), TryXReturnsNonZeroExitWithoutThrowing!);
		Register(nameof(AsyncXNeedsNoNullOptions), AsyncXNeedsNoNullOptions!);
	}

	/// TryX reports a non-zero exit in CommandExit instead of throwing it.
	public async partial Task<object?> TryXReturnsNonZeroExitWithoutThrowing(object? O) {
		await using var Command = Sh.TryX("dotnet nonexistent-cssh-command");
		var Exit = await Command.Done;
		Assert.IsTrue(!Exit.IsSuccess && Exit.ExitCode != 0);
		return null;
	}

	/// The common async call is X(command, Ct), without a null Options placeholder.
	public async partial Task<object?> AsyncXNeedsNoNullOptions(object? O) {
		using var Source = new CancellationTokenSource();
		await using var Command = Sh.X("dotnet --version", Source.Token);
		var Exit = await Command.Done;
		Assert.IsTrue(Exit.IsSuccess);
		return null;
	}
}

