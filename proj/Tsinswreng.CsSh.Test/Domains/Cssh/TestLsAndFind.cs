using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for listing and recursive glob lookup.
public partial class TestCssh{
	/// Registers Ls and Find cases.
	public partial void RegisterLsAndFind(ITestNode Node);

	/// Verifies Ls reports file-system kinds.
	public partial Task<object?> LsExposesFileAndDirectoryKinds(object? O);

	/// Verifies recursive ** glob matching.
	public partial Task<object?> FindMatchesRecursiveRelativeGlob(object? O);

	/// Verifies concise asynchronous Ls overloads.
	public partial Task<object?> AsyncLsNeedsNoNullOptions(object? O);
}

