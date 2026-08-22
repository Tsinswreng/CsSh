using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for directory-only listing.
public partial class TestCssh{
	/// Registers LsDir cases.
	public partial void RegisterLsDir(ITestNode Node);

	/// Verifies LsDir excludes files and obeys the recursive option.
	public partial Task<object?> LsDirFiltersDirectoriesAndRecurses(object? O);
}
