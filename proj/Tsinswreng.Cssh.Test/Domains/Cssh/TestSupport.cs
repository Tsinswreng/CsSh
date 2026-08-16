using Tsinswreng.Cssh;

namespace Cs.Test.Domains.Cssh;

/// Keeps test artefacts outside the repository and gives every case a collision-free root.
internal static class TestSupport{
	/// Creates a unique path without creating it, so Mkdir itself is still tested.
	internal static string NewRoot() {
		return Path.Combine(Path.GetTempPath(), "Cssh-Test-" + Guid.NewGuid().ToString("N")).Replace('\\', '/');
	}

	/// Removes test data even after an assertion failure; Rm is intentionally idempotent.
	internal static void Clean(string Root) {
		Sh.Rm(Root);
	}
}
