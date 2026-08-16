using Tsinswreng.CsTreeTest;

namespace Cs.Test.Domains.Cssh;

/// Declares tests for the C# path-join extension operator.
public partial class TestCssh{
	/// Registers the path-join cases.
	public partial void RegisterExtnString(ITestNode Node);

	/// Verifies separator normalization and boundary joining.
	public partial Task<object?> PathDivisionJoinsAndNormalizesSeparators(object? O);
}
