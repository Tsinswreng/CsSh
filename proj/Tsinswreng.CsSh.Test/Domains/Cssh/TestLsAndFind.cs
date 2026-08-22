using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for the path-only Ls API.
public partial class TestCssh{
	/// Registers Ls cases.
	public partial void RegisterLs(ITestNode Node);

	/// Verifies Ls returns directly usable complete paths without FileSystemInfo metadata.
	public partial Task<object?> LsReturnsCompletePaths(object? O);

	/// Verifies Ls passes the recursive option through to the BCL enumerator.
	public partial Task<object?> LsRecurses(object? O);
}

