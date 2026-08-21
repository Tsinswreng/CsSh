using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for BaseName path-component extraction.
public partial class TestCssh{
	/// Registers BaseName cases.
	public partial void RegisterBaseName(ITestNode Node);

	/// Verifies BaseName returns the final component for ordinary and trailing-separator paths.
	public partial Task<object?> BaseNameReturnsFinalPathComponent(object? O);

	/// Verifies BaseName returns an empty value for a path consisting only of its root.
	public partial Task<object?> BaseNameReturnsEmptyForRootPath(object? O);
}
