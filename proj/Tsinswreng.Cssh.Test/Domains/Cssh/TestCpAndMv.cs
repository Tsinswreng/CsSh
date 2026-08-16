using Tsinswreng.CsTreeTest;

namespace Cs.Test.Domains.Cssh;

/// Declares tests for copy and move operations.
public partial class TestCssh{
	/// Registers Cp and Mv cases.
	public partial void RegisterCpAndMv(ITestNode Node);

	/// Verifies recursive copying retains both files and empty directories.
	public partial Task<object?> CpPreservesFilesAndEmptyDirectories(object? O);

	/// Verifies moving builds missing destination parents.
	public partial Task<object?> MvCreatesDestinationParent(object? O);

	/// Verifies concise asynchronous Cp and Mv overloads.
	public partial Task<object?> AsyncCpAndMvNeedNoNullOptions(object? O);
}
