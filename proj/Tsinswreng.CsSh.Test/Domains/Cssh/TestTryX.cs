using Tsinswreng.CsTreeTest;

namespace Cs.Test.Domains.Cssh;

/// Declares tests for non-throwing external command execution.
public partial class TestCssh{
	/// Registers TryX cases.
	public partial void RegisterTryX(ITestNode Node);

	/// Verifies a non-zero exit produces CommandExit rather than an exception.
	public partial Task<object?> TryXReturnsNonZeroExitWithoutThrowing(object? O);

	/// Verifies the concise asynchronous X overload.
	public partial Task<object?> AsyncXNeedsNoNullOptions(object? O);
}
