using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for the C# path-join extension operator.
public partial class TestCssh{
	public partial void RegisterExtnString(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ExtnString), typeof(ShGlobal)], ["operator /", nameof(ShGlobal.BaseName), nameof(ShGlobal.DirName), nameof(ShGlobal.RealPath)], "Path").Register;
		Register(nameof(PathDivisionJoinsAndNormalizesSeparators), PathDivisionJoinsAndNormalizesSeparators!);
		Register(nameof(BaseNameAndDirNameExtractPathParts), BaseNameAndDirNameExtractPathParts!);
		Register(nameof(RealPathUsesShCurrentDirectory), RealPathUsesShCurrentDirectory!);
	}

	/// Ordinary strings compose portable Cssh paths while accidental boundary separators collapse to one.
	public partial Task<object?> PathDivisionJoinsAndNormalizesSeparators(object? O) {
		Assert.IsTrue(("src" / "app" / "file.cs") == "src/app/file.cs");
		Assert.IsTrue(("src\\" / "/app") == "src/app");
		Assert.IsTrue(("C:\\work" / "project") == "C:/work/project");
		return Task.FromResult<object?>(null);
	}

	/// Bash-style name functions operate only on syntax and do not require a real file-system entry.
	public partial Task<object?> BaseNameAndDirNameExtractPathParts(object? O) {
		Assert.IsTrue(ShGlobal.BaseName("src/app/config.json") == "config.json");
		Assert.IsTrue(ShGlobal.DirName("src/app/config.json") == "src/app");
		return Task.FromResult<object?>(null);
	}

	/// RealPath follows Cd on the selected Sh instance while preserving the forward-slash CsSh representation.
	public partial Task<object?> RealPathUsesShCurrentDirectory(object? O) {
		var Sh = new Sh();
		var Start = Sh.Pwd();
		Sh.Cd(".");
		Assert.IsTrue(Sh.FullPath("src/../README.typ") == Start / "README.typ");
		return Task.FromResult<object?>(null);
	}
}

