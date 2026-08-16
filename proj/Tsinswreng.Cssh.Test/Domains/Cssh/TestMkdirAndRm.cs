using Tsinswreng.CsTreeTest;

namespace Cs.Test.Domains.Cssh;

/// Declares tests for directory creation and fixed rm -rf semantics.
public partial class TestCssh{
	/// Registers Mkdir and Rm cases.
	public partial void RegisterMkdirAndRm(ITestNode Node);

	/// Verifies recursive removal of a populated directory tree.
	public partial Task<object?> MkdirCreatesParentsAndRmRemovesTree(object? O);

	/// Verifies that an absent target is a successful Rm no-op.
	public partial Task<object?> RmMissingPathIsSuccessful(object? O);
}
