using Tsinswreng.CsTreeTest;
using Tsinswreng.Cssh;

namespace Cs.Test.Domains.Cssh;

/// Tests the C# extension operator used for terse script path composition.
public partial class TestCssh{
	public void RegisterExtnString(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ExtnString)], ["operator /"], "Path").Register;
		Register(nameof(PathDivisionJoinsAndNormalizesSeparators), PathDivisionJoinsAndNormalizesSeparators!);
	}

	/// Ordinary strings can compose portable Cssh paths while accidental boundary separators collapse to one.
	public Task<object?> PathDivisionJoinsAndNormalizesSeparators(object? O) {
		Assert.IsTrue(("src" / "app" / "file.cs") == "src/app/file.cs");
		Assert.IsTrue(("src\\" / "/app") == "src/app");
		Assert.IsTrue(("C:\\work" / "project") == "C:/work/project");
		return Task.FromResult<object?>(null);
	}
}
