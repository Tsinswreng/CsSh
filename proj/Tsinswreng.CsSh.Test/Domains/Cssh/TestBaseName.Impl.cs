using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for BaseName path-component extraction.
public partial class TestCssh{
	public partial void RegisterBaseName(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.BaseName)], "Path").Register;
		Register(nameof(BaseNameReturnsFinalPathComponent), BaseNameReturnsFinalPathComponent!);
		Register(nameof(BaseNameReturnsEmptyForRootPath), BaseNameReturnsEmptyForRootPath!);
	}

	/// A final separator identifies a directory but BaseName still returns that directory's name.
	public partial Task<object?> BaseNameReturnsFinalPathComponent(object? O) {
		Assert.IsTrue(ShGlobal.BaseName("src/app/config.json") == "config.json");
		Assert.IsTrue(ShGlobal.BaseName("/a/b/c/d/") == "d");
		Assert.IsTrue(ShGlobal.BaseName("d") == "d");
		return Task.FromResult<object?>(null);
	}

	/// The .NET component API has no final name to return when the input is only its root.
	public partial Task<object?> BaseNameReturnsEmptyForRootPath(object? O) {
		Assert.IsTrue(ShGlobal.BaseName("/") == "");
		return Task.FromResult<object?>(null);
	}
}
