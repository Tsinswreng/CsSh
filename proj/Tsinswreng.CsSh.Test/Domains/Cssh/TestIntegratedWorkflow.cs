using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares the one-case, stateful Cssh workflow test.
public partial class TestCssh{
	/// Test data lives beside the executable so its lifetime is visible to the test runner and reviewer.
	private static readonly string IntegratedRoot = AppContext.BaseDirectory / "Cssh.IntegratedTestData";

	/// Registers the whole test-data lifecycle as one test case.
	public partial void RegisterIntegratedWorkflow(ITestNode Node);

	/// Creates test data beside the test entry, exercises multiple Cssh APIs in order, then always removes it.
	public partial Task<object?> IntegratedWorkflowCreatesUsesAndRemovesEntrySideData(object? O);
}
