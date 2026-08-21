using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for DirName path-component extraction.
public partial class TestCssh{
	public partial void RegisterDirName(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.DirName)], "Path").Register;
		Register(nameof(DirNameReturnsParentPath), DirNameReturnsParentPath!);
		Register(nameof(DirNameReturnsEmptyWithoutParentComponent), DirNameReturnsEmptyWithoutParentComponent!);
	}

	/// The final separator is ignored before selecting the parent path.
	public partial Task<object?> DirNameReturnsParentPath(object? O) {
		Assert.IsTrue(ShGlobal.DirName("src/app/config.json") == "src/app");
		Assert.IsTrue(ShGlobal.DirName("/a/b/c/d/") == "/a/b/c");
		return Task.FromResult<object?>(null);
	}

	/// A single component and the root path have no parent component under the .NET path contract.
	public partial Task<object?> DirNameReturnsEmptyWithoutParentComponent(object? O) {
		Assert.IsTrue(ShGlobal.DirName("d") == "");
		Assert.IsTrue(ShGlobal.DirName("/") == "");
		return Task.FromResult<object?>(null);
	}
}
