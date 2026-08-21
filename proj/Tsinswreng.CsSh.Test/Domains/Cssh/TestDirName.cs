using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for DirName path-component extraction.
public partial class TestCssh{
	/// Registers DirName cases.
	public partial void RegisterDirName(ITestNode Node);

	/// Verifies DirName removes one final component after ignoring a trailing separator.
	public partial Task<object?> DirNameReturnsParentPath(object? O);

	/// Verifies DirName returns an empty value when no parent component exists.
	public partial Task<object?> DirNameReturnsEmptyWithoutParentComponent(object? O);
}
