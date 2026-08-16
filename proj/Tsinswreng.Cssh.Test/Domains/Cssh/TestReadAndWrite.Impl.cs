using Tsinswreng.CsTreeTest;
using Tsinswreng.Cssh;

namespace Cs.Test.Domains.Cssh;

/// Implements tests for concise asynchronous text I/O.
public partial class TestCssh{
	public partial void RegisterReadAndWrite(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.Read), nameof(Sh.Write)], "FileSystem").Register;
		Register(nameof(AsyncReadAndWriteNeedNoNullOptions), AsyncReadAndWriteNeedNoNullOptions!);
	}

	/// Normal async script use passes only path, text and the final Ct.
	public async partial Task<object?> AsyncReadAndWriteNeedNoNullOptions(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			using var Source = new CancellationTokenSource();
			await Sh.Write(Root + "/nested/message.txt", "async", Source.Token);
			var Text = await Sh.Read(Root + "/nested/message.txt", Source.Token);
			Assert.IsTrue(Text == "async");
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}
}
