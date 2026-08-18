using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for FsInfo file-system metadata lookup.
public partial class TestCssh{
	/// Registers FsInfo tests.
	public partial void RegisterFsInfo(ITestNode Node);

	/// Verifies synchronous FsInfo returns the appropriate BCL metadata type.
	public partial Task<object?> FsInfoReturnsFileAndDirectoryInfo(object? O);

	/// Verifies asynchronous FsInfo returns null for a missing path.
	public partial Task<object?> AsyncFsInfoReturnsNullForMissingPath(object? O);
}
