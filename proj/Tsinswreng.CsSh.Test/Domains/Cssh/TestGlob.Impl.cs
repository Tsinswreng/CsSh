using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for glob-based path enumeration.
public partial class TestCssh{
	public partial void RegisterGlob(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.Glob)], "Glob").Register;
		Register(nameof(GlobMatchesPathSegments), GlobMatchesPathSegments!);
		Register(nameof(GlobUsesShCurrentDirectory), GlobUsesShCurrentDirectory!);
		Register(nameof(GlobTraversesParentOfShCurrentDirectory), GlobTraversesParentOfShCurrentDirectory!);
		Register(nameof(GlobMatchesLiteralPaths), GlobMatchesLiteralPaths!);
	}

	/// The third-party Standard dialect matches files without a trailing slash and directories with one.
	public partial Task<object?> GlobMatchesPathSegments(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Write(Root + "/input/top.cs", "top");
			ShGlobal.Write(Root + "/input/a.cs", "single character");
			ShGlobal.Write(Root + "/input/nested/child.cs", "child");
			ShGlobal.Write(Root + "/input/nested/skip.txt", "skip");
			var Direct = ShGlobal.Glob(Root + "/input/*.cs").OrderBy(Entry => Entry.Value).ToArray();
			var SingleCharacter = ShGlobal.Glob(Root + "/input/?.cs").ToArray();
			var Recursive = ShGlobal.Glob(Root + "/input/**/*.cs").OrderBy(Entry => Entry.Value).ToArray();
			var DirectEntries = ShGlobal.Glob(Root + "/input/*").OrderBy(Entry => Entry.Value).ToArray();
			var DirectDirectories = ShGlobal.Glob(Root + "/input/*/").ToArray();
			Pth[] DirectExpected = [(Pth)(Root + "/input/a.cs"), (Pth)(Root + "/input/top.cs")];
			Pth[] SingleCharacterExpected = [(Pth)(Root + "/input/a.cs")];
			Pth[] RecursiveExpected = [(Pth)(Root + "/input/a.cs"), (Pth)(Root + "/input/top.cs"), (Pth)(Root + "/input/nested/child.cs")];
			Pth[] DirectEntriesExpected = [(Pth)(Root + "/input/a.cs"), (Pth)(Root + "/input/top.cs")];
			Pth[] DirectDirectoriesExpected = [(Pth)(Root + "/input/nested")];
			Assert.IsTrue(Direct.SequenceEqual(DirectExpected));
			Assert.IsTrue(SingleCharacter.SequenceEqual(SingleCharacterExpected));
			Assert.IsTrue(Recursive.SequenceEqual(RecursiveExpected.OrderBy(Entry => Entry.Value)));
			Assert.IsTrue(DirectEntries.SequenceEqual(DirectEntriesExpected.OrderBy(Entry => Entry.Value)));
			Assert.IsTrue(DirectDirectories.SequenceEqual(DirectDirectoriesExpected));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// A local shell must not resolve a relative pattern from the process-wide current directory.
	public partial Task<object?> GlobUsesShCurrentDirectory(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Write(Root + "/input/entry.txt", "entry");
			var LocalSh = new Sh();
			LocalSh.Cd(Root);
			Assert.IsTrue(LocalSh.Glob("input/*.txt").SequenceEqual([(Pth)(Root + "/input/entry.txt")]));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// A glob rooted in the parent directory must remain relative to LocalSh, not process state.
	public partial Task<object?> GlobTraversesParentOfShCurrentDirectory(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			ShGlobal.Write(Root + "/outside/entry.txt", "entry");
			var LocalSh = new Sh();
			LocalSh.Cd(Root + "/current");
			Assert.IsTrue(LocalSh.Glob("../outside/*.txt").SequenceEqual([(Pth)(Root + "/outside/entry.txt")]));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

	/// Literal paths still pass through the third-party matcher, including its directory suffix rule.
	public partial Task<object?> GlobMatchesLiteralPaths(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			var File = Root + "/input/entry.txt";
			var Directory = Root + "/input/folder";
			ShGlobal.Write(File, "entry");
			ShGlobal.Mkdir(Directory);
			Assert.IsTrue(ShGlobal.Glob(File).SequenceEqual([(Pth)File]));
			Assert.IsTrue(ShGlobal.Glob(Directory + "/").SequenceEqual([(Pth)Directory]));
			var LocalSh = new Sh();
			LocalSh.Cd(Root);
			Assert.IsTrue(LocalSh.Glob("input/folder/").SequenceEqual([(Pth)Directory]));
			Assert.IsTrue(LocalSh.Glob("input\\folder\\").SequenceEqual([(Pth)Directory]));
		}
		finally {
			TestSupport.Clean(Root);
		}
		return Task.FromResult<object?>(null);
	}

}
