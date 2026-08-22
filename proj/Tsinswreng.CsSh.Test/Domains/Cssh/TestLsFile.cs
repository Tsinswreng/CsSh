using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for file-only listing.
public partial class TestCssh{
	/// Registers LsFile cases.
	public partial void RegisterLsFile(ITestNode Node);

	/// Verifies LsFile excludes directories and obeys the recursive option.
	public partial Task<object?> LsFileFiltersFilesAndRecurses(object? O);
}
