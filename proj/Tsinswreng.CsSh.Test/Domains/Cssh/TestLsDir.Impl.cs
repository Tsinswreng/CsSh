using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for directory-only listing.
public partial class TestCssh{
	public partial void RegisterLsDir(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.LsDir)], "LsDir").Register;
		Register(nameof(LsDirFiltersDirectoriesAndRecurses), LsDirFiltersDirectoriesAndRecurses!);
	}

	/// LsDir delegates to Directory.EnumerateDirectories, so files never appear in the result.
	public partial Task<object?> LsDirFiltersDirectoriesAndRecurses(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Mkdir(Root + "/top/nested");
			ShGlobal.Write(Root + "/top/file.txt", "file");
			Assert.IsTrue(ShGlobal.LsDir(Root).SequenceEqual([(Pth)(Root + "/top")]));
			var Recursive = ShGlobal.LsDir(Root, new LsOptions(Recursive: true)).OrderBy(Entry => Entry.Value).ToArray();
			Pth[] Expected = [(Pth)(Root + "/top"), (Pth)(Root + "/top/nested")];
			Assert.IsTrue(Recursive.SequenceEqual(Expected.OrderBy(Entry => Entry.Value)));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

}
