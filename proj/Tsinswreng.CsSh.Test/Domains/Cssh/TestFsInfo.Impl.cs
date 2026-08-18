using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements FsInfo tests with isolated test directories.
public partial class TestCssh{
	public partial void RegisterFsInfo(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.FsInfo), nameof(ShGlobal.IsFile), nameof(ShGlobal.IsDir)], "FileSystem").Register;
		Register(nameof(FsInfoReturnsFileAndDirectoryInfo), FsInfoReturnsFileAndDirectoryInfo!);
		Register(nameof(AsyncFsInfoReturnsNullForMissingPath), AsyncFsInfoReturnsNullForMissingPath!);
		Register(nameof(IsFileAndIsDirDistinguishPathKinds), IsFileAndIsDirDistinguishPathKinds!);
		Register(nameof(AsyncIsFileAndIsDirDistinguishMissingPath), AsyncIsFileAndIsDirDistinguishMissingPath!);
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

	/// The predicate APIs keep the common Bash-style file-versus-directory branch concise.
	public partial Task<object?> IsFileAndIsDirDistinguishPathKinds(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Mkdir(Root / "folder");
			ShGlobal.Write(Root / "file.txt", "data");
			Assert.IsTrue(ShGlobal.IsFile(Root / "file.txt"));
			Assert.IsTrue(!ShGlobal.IsDir(Root / "file.txt"));
			Assert.IsTrue(!ShGlobal.IsFile(Root / "folder"));
			Assert.IsTrue(ShGlobal.IsDir(Root / "folder"));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// Missing paths are neither files nor directories in the asynchronous overloads.
	public async partial Task<object?> AsyncIsFileAndIsDirDistinguishMissingPath(object? O) {
		var Root = TestSupport.NewRoot();
		using var Source = new CancellationTokenSource();
		try {
			var Missing = Root / "missing";
			Assert.IsTrue(!await ShGlobal.IsFile(Missing, Source.Token));
			Assert.IsTrue(!await ShGlobal.IsDir(Missing, Source.Token));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}
}
