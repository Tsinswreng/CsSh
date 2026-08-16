using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for the C# path-join extension operator.
public partial class TestCssh{
	public partial void RegisterExtnString(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ExtnString)], ["operator /"], "Path").Register;
		Register(nameof(PathDivisionJoinsAndNormalizesSeparators), PathDivisionJoinsAndNormalizesSeparators!);
	}

	/// Ordinary strings compose portable Cssh paths while accidental boundary separators collapse to one.
	public partial Task<object?> PathDivisionJoinsAndNormalizesSeparators(object? O) {
		Assert.IsTrue(("src" / "app" / "file.cs") == "src/app/file.cs");
		Assert.IsTrue(("src\\" / "/app") == "src/app");
		Assert.IsTrue(("C:\\work" / "project") == "C:/work/project");
		return Task.FromResult<object?>(null);
	}
}

