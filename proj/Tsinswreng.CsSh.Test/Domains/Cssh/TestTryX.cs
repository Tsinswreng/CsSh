using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for non-throwing external command execution.
public partial class TestCssh{
	/// Registers TryCmd and TryExe cases.
	public partial void RegisterTryX(ITestNode Node);

	/// Verifies a non-zero exit produces CommandExit rather than an exception.
	public partial Task<object?> TryXReturnsNonZeroExitWithoutThrowing(object? O);

	/// Verifies the concise asynchronous Exe overload.
	public partial Task<object?> AsyncXNeedsNoNullOptions(object? O);
}

