using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for successful lazy command execution.
public partial class TestCssh{
	/// Registers X cases.
	public partial void RegisterX(ITestNode Node);

	/// Verifies Done starts X and stdout remains consumable after exit.
	public partial Task<object?> XStartsWhenDoneIsObservedAndReturnsStdout(object? O);

	/// Verifies Content input is supplied through CommandOptions.
	public partial Task<object?> XPassesContentAsStdin(object? O);

	/// Verifies Out routes a lazy command result to an explicitly supplied Content target.
	public partial Task<object?> OutWritesCommandOutput(object? O);

	/// Verifies Out can concisely direct both output streams to a file path.
	public partial Task<object?> OutWritesCommandOutputToPath(object? O);
}

