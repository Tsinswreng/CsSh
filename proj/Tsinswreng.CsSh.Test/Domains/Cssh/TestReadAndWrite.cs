using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for concise asynchronous text I/O.
public partial class TestCssh{
	/// Registers Read and Write cases.
	public partial void RegisterReadAndWrite(ITestNode Node);

	/// Verifies normal asynchronous Content I/O needs no null options placeholder.
	public partial Task<object?> AsyncReadAndWriteNeedNoNullOptions(object? O);

	/// Verifies string and ordinary Stream both enter the same Content-based file API.
	public partial Task<object?> ContentImplicitConversionsWorkWithFileIo(object? O);
}

