using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for listing and recursive glob lookup.
public partial class TestCssh{
	public partial void RegisterLsAndFind(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.Ls), nameof(ShGlobal.Glob)], "FileSystem").Register;
		Register(nameof(LsExposesFileAndDirectoryKinds), LsExposesFileAndDirectoryKinds!);
		Register(nameof(LsExposesBclFileAttributes), LsExposesBclFileAttributes!);
		Register(nameof(FindMatchesRecursiveRelativeGlob), FindMatchesRecursiveRelativeGlob!);
		Register(nameof(AsyncLsNeedsNoNullOptions), AsyncLsNeedsNoNullOptions!);
	}

	/// Ls entries identify files and directories without a second file-system lookup.
	public partial Task<object?> LsExposesFileAndDirectoryKinds(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Mkdir(Root + "/folder");
			ShGlobal.Write(Root + "/file.txt", "entry");
			var Entries = ShGlobal.Ls(Root).ToDictionary(Entry => Entry.Name);
			Assert.IsTrue(Entries["folder"] is DirectoryInfo);
			Assert.IsTrue(Entries["file.txt"] is FileInfo File && File.Length == 5);
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// The returned FileInfo exposes BCL metadata directly; CsSh does not mirror these properties in another DTO.
	public partial Task<object?> LsExposesBclFileAttributes(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Write(Root + "/data.bin", "metadata");
			var File = ShGlobal.Ls(Root).OfType<FileInfo>().Single(Entry => Entry.Name == "data.bin");
			Assert.IsTrue(File.Length == 8);
			Assert.IsTrue(!File.Attributes.HasFlag(FileAttributes.Directory));
			Assert.IsTrue(File.LastWriteTimeUtc <= DateTime.UtcNow);
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
			ShGlobal.Write(Root + "/input/a.txt", "a");
			ShGlobal.Write(Root + "/input/nested/b.txt", "b");
			ShGlobal.Write(Root + "/input/nested/c.bin", "c");
			var Found = ShGlobal.Glob(Root + "/input/**/*.txt").Select(Entry => Entry.Name).Order().ToArray();
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
			await ShGlobal.Write(Root + "/entry.txt", "entry", Source.Token);
			var FoundFile = false;
			await foreach (var Entry in ShGlobal.Ls(Root, Source.Token)) {
				FoundFile |= Entry.Name == "entry.txt" && Entry is FileInfo;
			}
			Assert.IsTrue(FoundFile);
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}
}

