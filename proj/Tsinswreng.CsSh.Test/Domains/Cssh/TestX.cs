using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for successful lazy command execution.
public partial class TestCssh{
	/// Registers X cases.
	public partial void RegisterX(ITestNode Node);

	/// Verifies Done starts X and stdout remains consumable after exit.
	public partial Task<object?> XStartsWhenDoneIsObservedAndReturnsStdout(object? O);

	/// Verifies external input streams are supplied through CommandOptions.
	public partial Task<object?> XPassesExternalStreamAsStdin(object? O);
}

