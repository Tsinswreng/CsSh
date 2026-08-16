using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for concise asynchronous text I/O.
public partial class TestCssh{
	public partial void RegisterReadAndWrite(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.Read), nameof(Sh.Write)], "FileSystem").Register;
		Register(nameof(AsyncReadAndWriteNeedNoNullOptions), AsyncReadAndWriteNeedNoNullOptions!);
		Register(nameof(ContentImplicitConversionsWorkWithFileIo), ContentImplicitConversionsWorkWithFileIo!);
	}

	/// Normal async script use passes only path, text and the final Ct.
	public async partial Task<object?> AsyncReadAndWriteNeedNoNullOptions(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			using var Source = new CancellationTokenSource();
			await Sh.Write(Root + "/nested/message.txt", "async", Source.Token);
			await using var Content = await Sh.Read(Root + "/nested/message.txt", Source.Token);
			Assert.IsTrue(await Content.Text(Source.Token) == "async");
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}

	/// String and Stream flow through the same Write signature by implicitly becoming Content.
	public async partial Task<object?> ContentImplicitConversionsWorkWithFileIo(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			using var CtSource = new CancellationTokenSource();
			var Ct = CtSource.Token;
			var FromText = Root / "text.txt";
			var FromStream = Root / "stream.txt";
			await Sh.Write(FromText, "from string", Ct);
			await using var Input = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("from stream"));
			await Sh.Write(FromStream, Input, Ct);
			await using (Content TextContent = await Sh.Read(FromText, Ct)) {
				string Text = TextContent;
				Assert.IsTrue(Text == "from string");
			}
			await using (Content StreamContent = await Sh.Read(FromStream, Ct)) {
				Stream Stream = StreamContent;
				using var Reader = new StreamReader(Stream, leaveOpen: true);
				Assert.IsTrue(await Reader.ReadToEndAsync(Ct) == "from stream");
			}
		}
		finally {
			TestSupport.Clean(Root);
		}
		return null;
	}
}

