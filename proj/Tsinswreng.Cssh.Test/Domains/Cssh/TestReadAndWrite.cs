using Tsinswreng.CsTreeTest;

namespace Cs.Test.Domains.Cssh;

/// Declares tests for concise asynchronous text I/O.
public partial class TestCssh{
	/// Registers Read and Write cases.
	public partial void RegisterReadAndWrite(ITestNode Node);

	/// Verifies normal asynchronous text I/O needs no null options placeholder.
	public partial Task<object?> AsyncReadAndWriteNeedNoNullOptions(object? O);
}
