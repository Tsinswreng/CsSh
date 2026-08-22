using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for copy and move operations.
public partial class TestCssh{
	public partial void RegisterCpAndMv(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.Cp), nameof(ShGlobal.Mv)], "FileSystem").Register;
		Register(nameof(CpPreservesFilesAndEmptyDirectories), CpPreservesFilesAndEmptyDirectories!);
		Register(nameof(MvCreatesDestinationParent), MvCreatesDestinationParent!);
		Register(nameof(AsyncCpAndMvNeedNoNullOptions), AsyncCpAndMvNeedNoNullOptions!);
		Register(nameof(CpGlobCopiesSourceContents), CpGlobCopiesSourceContents!);
		Register(nameof(CpDirectoryGlobCopiesDirectories), CpDirectoryGlobCopiesDirectories!);
		Register(nameof(CpFileIntoExistingDirectory), CpFileIntoExistingDirectory!);
		Register(nameof(MvDirectoryIntoExistingDirectory), MvDirectoryIntoExistingDirectory!);
	}

	/// Recursive copy retains an empty directory as well as ordinary file content.
	public partial Task<object?> CpPreservesFilesAndEmptyDirectories(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Mkdir(Root + "/source/empty");
			ShGlobal.Write(Root + "/source/content/data.txt", "copied");
			ShGlobal.Cp(Root + "/source", Root + "/destination");
			using (Content Copied = ShGlobal.Read(Root + "/destination/content/data.txt")) {
				Assert.IsTrue((string)Copied == "copied");
			}
			Assert.IsTrue(ShGlobal.Exists(Root + "/destination/empty"));
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
			ShGlobal.Write(Root + "/source.txt", "moved");
			ShGlobal.Mv(Root + "/source.txt", Root + "/deep/path/destination.txt");
			Assert.IsTrue(!ShGlobal.Exists(Root + "/source.txt"));
			using (Content Moved = ShGlobal.Read(Root + "/deep/path/destination.txt")) {
				Assert.IsTrue((string)Moved == "moved");
			}
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
			await ShGlobal.Write(Root + "/source.txt", "async", Source.Token);
			await ShGlobal.Cp(Root + "/source.txt", Root + "/copy.txt", Source.Token);
			await ShGlobal.Mv(Root + "/copy.txt", Root + "/moved.txt", Source.Token);
			await using (Content Moved = await ShGlobal.Read(Root + "/moved.txt", Source.Token)) {
				Assert.IsTrue(await Moved.Text(Source.Token) == "async");
			}
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}

	/// A trailing star has Bash's source-contents shape and preserves nested empty directories.
	public async partial Task<object?> CpGlobCopiesSourceContents(object? O) {
		var Root = TestSupport.NewRoot();
		using var CtSource = new CancellationTokenSource();
		var Ct = CtSource.Token;
		try {
			await ShGlobal.Mkdir(Root + "/source/folder/empty", Ct);
			await ShGlobal.Write(Root + "/source/file.txt", "glob", Ct);
			await ShGlobal.Write(Root + "/destination/folder/kept.txt", "kept", Ct);
			await ShGlobal.Cp(Root + "/source/*", Root + "/destination", Ct);

			Assert.IsTrue(await ShGlobal.Exists(Root + "/destination/file.txt", Ct));
			Assert.IsTrue(await ShGlobal.Exists(Root + "/destination/folder/empty", Ct));
			Assert.IsTrue(await ShGlobal.Exists(Root + "/destination/folder/kept.txt", Ct));
			Assert.IsTrue(!await ShGlobal.Exists(Root + "/destination/source", Ct));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}

	/// A trailing slash keeps the third-party directory-only glob mode when copying.
	public async partial Task<object?> CpDirectoryGlobCopiesDirectories(object? O) {
		var Root = TestSupport.NewRoot();
		using var CtSource = new CancellationTokenSource();
		var Ct = CtSource.Token;
		try {
			await ShGlobal.Mkdir(Root + "/source/folder", Ct);
			await ShGlobal.Write(Root + "/source/file.txt", "file", Ct);
			await ShGlobal.Cp(Root + "/source/*/", Root + "/destination", Ct);

			Assert.IsTrue(await ShGlobal.Exists(Root + "/destination/folder", Ct));
			Assert.IsTrue(!await ShGlobal.Exists(Root + "/destination/file.txt", Ct));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}

	/// Copying a file to an existing directory appends the source file name, matching cp's destination rule.
	public async partial Task<object?> CpFileIntoExistingDirectory(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Write(Root + "/source.txt", "file");
			ShGlobal.Mkdir(Root + "/out");
			await ShGlobal.Cp(Root + "/source.txt", Root + "/out", CancellationToken.None);
			Assert.IsTrue(ShGlobal.Exists(Root + "/out/source.txt"));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}

	/// Moving a directory to an existing directory appends the source directory name.
	public async partial Task<object?> MvDirectoryIntoExistingDirectory(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Write(Root + "/source/data.txt", "dir");
			ShGlobal.Mkdir(Root + "/out");
			await ShGlobal.Mv(Root + "/source", Root + "/out", CancellationToken.None);
			Assert.IsTrue(ShGlobal.Exists(Root + "/out/source/data.txt"));
			Assert.IsTrue(!ShGlobal.Exists(Root + "/source"));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}
}

