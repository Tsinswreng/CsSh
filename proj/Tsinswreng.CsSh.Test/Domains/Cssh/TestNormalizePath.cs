using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for separator-only path normalization.
public partial class TestCssh{
	/// Registers NormalizePath cases.
	public partial void RegisterNormalizePath(ITestNode Node);

	/// Verifies NormalizePath changes backslashes only and preserves other path syntax.
	public partial Task<object?> NormalizePathChangesSeparatorsOnly(object? O);
}
