using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for listing and recursive glob lookup.
public partial class TestCssh{
	public partial void RegisterLsAndFind(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.Ls), nameof(Sh.Find)], "FileSystem").Register;
		Register(nameof(LsExposesFileAndDirectoryKinds), LsExposesFileAndDirectoryKinds!);
		Register(nameof(FindMatchesRecursiveRelativeGlob), FindMatchesRecursiveRelativeGlob!);
		Register(nameof(AsyncLsNeedsNoNullOptions), AsyncLsNeedsNoNullOptions!);
	}

	/// Ls entries identify files and directories without a second file-system lookup.
	public partial Task<object?> LsExposesFileAndDirectoryKinds(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			Sh.Mkdir(Root + "/folder");
			Sh.Write(Root + "/file.txt", "entry");
			var Entries = Sh.Ls(Root).ToDictionary(Entry => Entry.Name);
			Assert.IsTrue(Entries["folder"].IsDir && !Entries["folder"].IsFile);
			Assert.IsTrue(Entries["file.txt"].IsFile && !Entries["file.txt"].IsDir);
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// The ** glob traverses only its static prefix and matches every descendant depth.
	public partial Task<object?> FindMatchesRecursiveRelativeGlob(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			Sh.Write(Root + "/input/a.txt", "a");
			Sh.Write(Root + "/input/nested/b.txt", "b");
			Sh.Write(Root + "/input/nested/c.bin", "c");
			var Found = Sh.Find(Root + "/input/**/*.txt").Select(Entry => Entry.Name).Order().ToArray();
			Assert.IsTrue(Found.SequenceEqual(["a.txt", "b.txt"]));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// The concise async Ls overload exposes direct children without a null Options value.
	public async partial Task<object?> AsyncLsNeedsNoNullOptions(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			using var Source = new CancellationTokenSource();
			await Sh.Write(Root + "/entry.txt", "entry", Source.Token);
			var FoundFile = false;
			await foreach (var Entry in Sh.Ls(Root, Source.Token)) {
				FoundFile |= Entry.Name == "entry.txt" && Entry.IsFile;
			}
			Assert.IsTrue(FoundFile);
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}
}

