using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for successful lazy Cmd execution.
public partial class TestCssh{
	/// Registers Cmd and Exe cases.
	public partial void RegisterX(ITestNode Node);

	/// Verifies Done starts Cmd and stdout remains consumable after exit.
	public partial Task<object?> XStartsWhenDoneIsObservedAndReturnsStdout(object? O);

	/// Verifies Content input is supplied through CommandOptions.
	public partial Task<object?> XPassesContentAsStdin(object? O);

	/// Verifies Out routes a lazy command result to an explicitly supplied Content target.
	public partial Task<object?> OutWritesCommandOutput(object? O);

	/// Verifies Out can concisely direct both output streams to a file path.
	public partial Task<object?> OutWritesCommandOutputToPath(object? O);

	/// Verifies Exe immediately executes and routes its standard output by default.
	public partial Task<object?> ExeWritesDefaultOutput(object? O);
}

