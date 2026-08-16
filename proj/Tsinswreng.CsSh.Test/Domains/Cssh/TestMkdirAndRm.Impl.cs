using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for directory creation and fixed rm -rf semantics.
public partial class TestCssh{
	public partial void RegisterMkdirAndRm(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.Mkdir), nameof(ShGlobal.Rm)], "FileSystem").Register;
		Register(nameof(MkdirCreatesParentsAndRmRemovesTree), MkdirCreatesParentsAndRmRemovesTree!);
		Register(nameof(RmMissingPathIsSuccessful), RmMissingPathIsSuccessful!);
	}

	/// Rm removes nested content recursively instead of requiring separate file deletion.
	public partial Task<object?> MkdirCreatesParentsAndRmRemovesTree(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Mkdir(Root + "/nested/leaf");
			ShGlobal.Write(Root + "/nested/leaf/file.txt", "content");
			Assert.IsTrue(ShGlobal.Exists(Root + "/nested/leaf/file.txt"));
			ShGlobal.Rm(Root);
			Assert.IsTrue(!ShGlobal.Exists(Root));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// The -f portion of Rm makes an absent target a no-op.
	public partial Task<object?> RmMissingPathIsSuccessful(object? O) {
		var Missing = TestSupport.NewRoot();
		ShGlobal.Rm(Missing);
		Assert.IsTrue(!ShGlobal.Exists(Missing));
		return Task.FromResult<object?>(null);
	}
}

