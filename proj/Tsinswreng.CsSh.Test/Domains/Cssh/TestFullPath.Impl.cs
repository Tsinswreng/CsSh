using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for FullPath resolution through a Sh instance directory.
public partial class TestCssh{
	public partial void RegisterFullPath(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.FullPath)], "Path").Register;
		Register(nameof(FullPathUsesShCurrentDirectory), FullPathUsesShCurrentDirectory!);
	}

	/// An independent Sh instance prevents this test from mutating the process-wide shell directory.
	public partial Task<object?> FullPathUsesShCurrentDirectory(object? O) {
		var Sh = new Sh();
		var Start = Sh.FullPath(".");
		Sh.Cd("sandbox/child");
		Assert.IsTrue(Sh.FullPath(".././file.txt") == Start / "sandbox/file.txt");
		return Task.FromResult<object?>(null);
	}
}
