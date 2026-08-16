using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Keeps every temporary test artefact beneath the test entry in the workspace.
internal static class TestSupport{
	/// Creates a unique entry-side path without creating it, so Mkdir itself is still tested.
	internal static string NewRoot() {
		return (AppContext.BaseDirectory / "CsSh.Test-" + Guid.NewGuid().ToString("N"));
	}

	/// Removes test data even after an assertion failure; Rm is intentionally idempotent.
	internal static void Clean(string Root) {
		ShGlobal.Rm(Root);
	}
}

