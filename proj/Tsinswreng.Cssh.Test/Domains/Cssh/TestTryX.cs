using Tsinswreng.CsTreeTest;
using Tsinswreng.Cssh;

namespace Cs.Test.Domains.Cssh;

/// Tests non-throwing command results and the concise Ct overload.
public partial class TestCssh{
	public void RegisterTryX(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.TryX)], "Command").Register;
		Register(nameof(TryXReturnsNonZeroExitWithoutThrowing), TryXReturnsNonZeroExitWithoutThrowing!);
		Register(nameof(AsyncXNeedsNoNullOptions), AsyncXNeedsNoNullOptions!);
	}

	/// TryX represents a non-zero exit in CommandExit instead of turning it into an exception.
	public async Task<object?> TryXReturnsNonZeroExitWithoutThrowing(object? O) {
		await using var Command = Sh.TryX("dotnet nonexistent-cssh-command");
		var Exit = await Command.Done;
		Assert.IsTrue(!Exit.IsSuccess && Exit.ExitCode != 0);
		return null;
	}

	/// The async common path is X(command, Ct), without an incidental null Options argument.
	public async Task<object?> AsyncXNeedsNoNullOptions(object? O) {
		using var Source = new CancellationTokenSource();
		await using var Command = Sh.X("dotnet --version", Source.Token);
		var Exit = await Command.Done;
		Assert.IsTrue(Exit.IsSuccess);
		return null;
	}
}
