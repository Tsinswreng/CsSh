using Tsinswreng.CsTreeTest;
using Tsinswreng.Cssh;

namespace Cs.Test.Domains.Cssh;

/// Tests text I/O and the short cancellation-token overloads intended for scripts.
public partial class TestCssh{
	public void RegisterReadAndWrite(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.Read), nameof(Sh.Write)], "FileSystem").Register;
		Register(nameof(AsyncReadAndWriteNeedNoNullOptions), AsyncReadAndWriteNeedNoNullOptions!);
	}

	/// Normal async script use passes only the path, text and final cancellation token.
	public async Task<object?> AsyncReadAndWriteNeedNoNullOptions(object? O) {
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
