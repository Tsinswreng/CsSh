using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for separator-only path normalization.
public partial class TestCssh{
	public partial void RegisterNormalizePath(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.NormalizePath)], "Path").Register;
		Register(nameof(NormalizePathChangesSeparatorsOnly), NormalizePathChangesSeparatorsOnly!);
	}

	/// NormalizePath is deliberately lexical: it must not resolve dot segments while changing separators.
	public partial Task<object?> NormalizePathChangesSeparatorsOnly(object? O) {
		Assert.IsTrue(Sh.NormalizePath(@"src\nested/file.txt") == "src/nested/file.txt");
		Assert.IsTrue(Sh.NormalizePath(@"src\.\nested\..\file.txt") == "src/./nested/../file.txt");
		return Task.FromResult<object?>(null);
	}
}
