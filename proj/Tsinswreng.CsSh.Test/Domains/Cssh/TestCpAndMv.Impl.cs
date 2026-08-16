using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for copy and move operations.
public partial class TestCssh{
	public partial void RegisterCpAndMv(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.Cp), nameof(Sh.Mv)], "FileSystem").Register;
		Register(nameof(CpPreservesFilesAndEmptyDirectories), CpPreservesFilesAndEmptyDirectories!);
		Register(nameof(MvCreatesDestinationParent), MvCreatesDestinationParent!);
		Register(nameof(AsyncCpAndMvNeedNoNullOptions), AsyncCpAndMvNeedNoNullOptions!);
	}

	/// Recursive copy retains an empty directory as well as ordinary file content.
	public partial Task<object?> CpPreservesFilesAndEmptyDirectories(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			Sh.Mkdir(Root + "/source/empty");
			Sh.Write(Root + "/source/content/data.txt", "copied");
			Sh.Cp(Root + "/source", Root + "/destination");
			Assert.IsTrue(Sh.Read(Root + "/destination/content/data.txt") == "copied");
			Assert.IsTrue(Sh.Exists(Root + "/destination/empty"));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// Moving a file builds a missing destination parent without an explicit Mkdir.
	public partial Task<object?> MvCreatesDestinationParent(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			Sh.Write(Root + "/source.txt", "moved");
			Sh.Mv(Root + "/source.txt", Root + "/deep/path/destination.txt");
			Assert.IsTrue(!Sh.Exists(Root + "/source.txt"));
			Assert.IsTrue(Sh.Read(Root + "/deep/path/destination.txt") == "moved");
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// The common asynchronous path takes just source, destination and the final Ct.
	public async partial Task<object?> AsyncCpAndMvNeedNoNullOptions(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			using var Source = new CancellationTokenSource();
			await Sh.Write(Root + "/source.txt", "async", Source.Token);
			await Sh.Cp(Root + "/source.txt", Root + "/copy.txt", Source.Token);
			await Sh.Mv(Root + "/copy.txt", Root + "/moved.txt", Source.Token);
			Assert.IsTrue(await Sh.Read(Root + "/moved.txt", Source.Token) == "async");
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}
}

