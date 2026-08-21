using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for FullPath resolution through a Sh instance directory.
public partial class TestCssh{
	/// Registers FullPath cases.
	public partial void RegisterFullPath(ITestNode Node);

	/// Verifies FullPath resolves a relative path from the Sh instance directory and simplifies dot segments.
	public partial Task<object?> FullPathUsesShCurrentDirectory(object? O);
}
