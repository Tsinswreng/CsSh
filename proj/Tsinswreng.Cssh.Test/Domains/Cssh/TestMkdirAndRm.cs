using Tsinswreng.CsTreeTest;
using Tsinswreng.Cssh;

namespace Cs.Test.Domains.Cssh;

/// Tests directory creation and the fixed rm -rf behavior.
public partial class TestCssh{
	public void RegisterMkdirAndRm(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.Mkdir), nameof(Sh.Rm)], "FileSystem").Register;
		Register(nameof(MkdirCreatesParentsAndRmRemovesTree), MkdirCreatesParentsAndRmRemovesTree!);
		Register(nameof(RmMissingPathIsSuccessful), RmMissingPathIsSuccessful!);
	}

	/// Verifies that Rm removes nested content recursively instead of requiring separate file deletion.
	public Task<object?> MkdirCreatesParentsAndRmRemovesTree(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			Sh.Mkdir(Root + "/nested/leaf");
			Sh.Write(Root + "/nested/leaf/file.txt", "content");
			Assert.IsTrue(Sh.Exists(Root + "/nested/leaf/file.txt"));
			Sh.Rm(Root);
			Assert.IsTrue(!Sh.Exists(Root));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// Verifies the -f half of the contract: no target is a successful no-op.
	public Task<object?> RmMissingPathIsSuccessful(object? O) {
		var Missing = TestSupport.NewRoot();
		Sh.Rm(Missing);
		Assert.IsTrue(!Sh.Exists(Missing));
		return Task.FromResult<object?>(null);
	}
}
