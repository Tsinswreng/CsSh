using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements FsInfo tests with isolated test directories.
public partial class TestCssh{
	public partial void RegisterFsInfo(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.FsInfo)], "FileSystem").Register;
		Register(nameof(FsInfoReturnsFileAndDirectoryInfo), FsInfoReturnsFileAndDirectoryInfo!);
		Register(nameof(AsyncFsInfoReturnsNullForMissingPath), AsyncFsInfoReturnsNullForMissingPath!);
	}

	/// FsInfo preserves the .NET FileInfo/DirectoryInfo distinction and exposes metadata.
	public partial Task<object?> FsInfoReturnsFileAndDirectoryInfo(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Mkdir(Root + "/folder");
			ShGlobal.Write(Root + "/file.txt", "data");
			var File = ShGlobal.FsInfo(Root + "/file.txt");
			var Directory = ShGlobal.FsInfo(Root + "/folder");
			Assert.IsTrue(File is FileInfo FileInfo && FileInfo.Length == 4);
			Assert.IsTrue(Directory is DirectoryInfo);
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// Missing paths map to null instead of being reported as either file or directory.
	public async partial Task<object?> AsyncFsInfoReturnsNullForMissingPath(object? O) {
		var Root = TestSupport.NewRoot();
		using var Source = new CancellationTokenSource();
		try {
			var Result = await ShGlobal.FsInfo(Root + "/missing.txt", Source.Token);
			Assert.IsTrue(Result is null);
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}
}
