using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for file-only listing.
public partial class TestCssh{
	public partial void RegisterLsFile(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.LsFile)], "LsFile").Register;
		Register(nameof(LsFileFiltersFilesAndRecurses), LsFileFiltersFilesAndRecurses!);
	}

	/// LsFile delegates to Directory.EnumerateFiles, so directories never appear in the result.
	public partial Task<object?> LsFileFiltersFilesAndRecurses(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Mkdir(Root + "/top/nested");
			ShGlobal.Write(Root + "/top/file.txt", "top");
			ShGlobal.Write(Root + "/top/nested/child.txt", "child");
			Assert.IsTrue(ShGlobal.LsFile(Root).SequenceEqual(Array.Empty<Pth>()));
			var Recursive = ShGlobal.LsFile(Root, new LsOptions(Recursive: true)).OrderBy(Entry => Entry.Value).ToArray();
			Pth[] Expected = [(Pth)(Root + "/top/file.txt"), (Pth)(Root + "/top/nested/child.txt")];
			Assert.IsTrue(Recursive.SequenceEqual(Expected.OrderBy(Entry => Entry.Value)));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

}
