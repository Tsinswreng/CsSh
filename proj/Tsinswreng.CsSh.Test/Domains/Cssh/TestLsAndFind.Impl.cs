using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for path-only directory listing.
public partial class TestCssh{
	public partial void RegisterLs(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.Ls)], "Ls").Register;
		Register(nameof(LsReturnsCompletePaths), LsReturnsCompletePaths!);
		Register(nameof(LsRecurses), LsRecurses!);
	}

	/// Ls returns complete Pth values, so every result can be passed directly to other file APIs.
	public partial Task<object?> LsReturnsCompletePaths(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Mkdir(Root + "/folder");
			ShGlobal.Write(Root + "/file.txt", "entry");
			var Entries = ShGlobal.Ls(Root).OrderBy(Entry => Entry.Value).ToArray();
			Pth[] Expected = [(Pth)(Root + "/file.txt"), (Pth)(Root + "/folder")];
			Assert.IsTrue(Entries.SequenceEqual(Expected.OrderBy(Entry => Entry.Value)));
			Assert.IsTrue(Entries.All(Entry => ShGlobal.Exists(Entry)));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// Recursive listing must include both files and directories at every descended level.
	public partial Task<object?> LsRecurses(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Mkdir(Root + "/top/nested");
			ShGlobal.Write(Root + "/top/file.txt", "file");
			ShGlobal.Write(Root + "/top/nested/child.txt", "child");
			var Entries = ShGlobal.Ls(Root, new LsOptions(Recursive: true)).OrderBy(Entry => Entry.Value).ToArray();
			Pth[] Expected = [
				(Pth)(Root + "/top"),
				(Pth)(Root + "/top/file.txt"),
				(Pth)(Root + "/top/nested"),
				(Pth)(Root + "/top/nested/child.txt"),
			];
			Assert.IsTrue(Entries.SequenceEqual(Expected.OrderBy(Entry => Entry.Value)));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

}

