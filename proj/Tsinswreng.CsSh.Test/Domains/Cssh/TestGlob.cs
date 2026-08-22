using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for glob-based path enumeration.
public partial class TestCssh{
	/// Registers Glob cases.
	public partial void RegisterGlob(ITestNode Node);

	/// Verifies the third-party Standard dialect's file and directory pattern semantics.
	public partial Task<object?> GlobMatchesPathSegments(object? O);

	/// Verifies Glob resolves relative patterns from its owning Sh instance.
	public partial Task<object?> GlobUsesShCurrentDirectory(object? O);

	/// Verifies a relative glob can traverse to a parent of the owning Sh directory.
	public partial Task<object?> GlobTraversesParentOfShCurrentDirectory(object? O);

	/// Verifies literal file and directory patterns use the same third-party semantics.
	public partial Task<object?> GlobMatchesLiteralPaths(object? O);
}
