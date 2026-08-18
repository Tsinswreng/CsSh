using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for the C# path-join extension operator.
public partial class TestCssh{
	/// Registers the path-join cases.
	public partial void RegisterExtnString(ITestNode Node);

	/// Verifies separator normalization and boundary joining.
	public partial Task<object?> PathDivisionJoinsAndNormalizesSeparators(object? O);

	/// Verifies shell-style basename and dirname extraction.
	public partial Task<object?> BaseNameAndDirNameExtractPathParts(object? O);

	/// Verifies RealPath resolves through the Sh instance current directory.
	public partial Task<object?> RealPathUsesShCurrentDirectory(object? O);
}

