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
		Register(nameof(CpGlobCopiesSourceContents), CpGlobCopiesSourceContents!);
	}

	/// Recursive copy retains an empty directory as well as ordinary file content.
	public partial Task<object?> CpPreservesFilesAndEmptyDirectories(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			Sh.Mkdir(Root + "/source/empty");
			Sh.Write(Root + "/source/content/data.txt", "copied");
			Sh.Cp(Root + "/source", Root + "/destination");
			using (Content Copied = Sh.Read(Root + "/destination/content/data.txt")) {
				Assert.IsTrue((string)Copied == "copied");
			}
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
			using (Content Moved = Sh.Read(Root + "/deep/path/destination.txt")) {
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
			await Sh.Write(Root + "/source.txt", "async", Source.Token);
			await Sh.Cp(Root + "/source.txt", Root + "/copy.txt", Source.Token);
			await Sh.Mv(Root + "/copy.txt", Root + "/moved.txt", Source.Token);
			await using (Content Moved = await Sh.Read(Root + "/moved.txt", Source.Token)) {
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
			await Sh.Mkdir(Root + "/source/folder/empty", Ct);
			await Sh.Write(Root + "/source/file.txt", "glob", Ct);
			await Sh.Write(Root + "/destination/folder/kept.txt", "kept", Ct);
			await Sh.Cp(Root + "/source/*", Root + "/destination", Ct);

			Assert.IsTrue(await Sh.Exists(Root + "/destination/file.txt", Ct));
			Assert.IsTrue(await Sh.Exists(Root + "/destination/folder/empty", Ct));
			Assert.IsTrue(await Sh.Exists(Root + "/destination/folder/kept.txt", Ct));
			Assert.IsTrue(!await Sh.Exists(Root + "/destination/source", Ct));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}
}

