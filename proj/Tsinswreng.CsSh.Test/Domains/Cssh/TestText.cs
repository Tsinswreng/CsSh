using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares Command.Text terminal-consumption tests.
public partial class TestCssh{
	/// Registers Text tests.
	public partial void RegisterText(ITestNode Node);

	/// Verifies Text drains standard error together with standard output and exposes the successful exit result.
	public partial Task<object?> TextReadsBothOutputs(object? O);

	/// Verifies CommandOptions.Env only changes the child process environment.
	public partial Task<object?> CommandEnvironmentOverridesArePerCommand(object? O);
}
