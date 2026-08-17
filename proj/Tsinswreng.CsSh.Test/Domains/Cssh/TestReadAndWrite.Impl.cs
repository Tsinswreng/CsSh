using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements tests for concise asynchronous text I/O.
public partial class TestCssh{
	public partial void RegisterReadAndWrite(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(ShGlobal)], [nameof(ShGlobal.Read), nameof(ShGlobal.Write)], "FileSystem").Register;
		Register(nameof(AsyncReadAndWriteNeedNoNullOptions), AsyncReadAndWriteNeedNoNullOptions!);
		Register(nameof(ContentImplicitConversionsWorkWithFileIo), ContentImplicitConversionsWorkWithFileIo!);
		Register(nameof(ContentTextIsCached), ContentTextIsCached!);
	}

	/// Text() and Text(Ct) share one cached consumption instead of advancing the stream twice.
	public async partial Task<object?> ContentTextIsCached(object? O) {
		await using Content Source = "cached text";
		var First = await Source.Text(CancellationToken.None);
		var Second = Source.Text();
		var Third = (string)Source;
		Assert.IsTrue(First == "cached text");
		Assert.IsTrue(Second == First);
		Assert.IsTrue(Third == First);
		return null;
	}

	/// Normal async script use passes only path, text and the final Ct.
	public async partial Task<object?> AsyncReadAndWriteNeedNoNullOptions(object? O) {
		var Root = TestSupport.NewRoot();
		try {
			using var Source = new CancellationTokenSource();
			await ShGlobal.Write(Root + "/nested/message.txt", "async", Source.Token);
			await using var Content = await ShGlobal.Read(Root + "/nested/message.txt", Source.Token);
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
			await ShGlobal.Write(FromText, "from string", Ct);
			await using var Input = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("from stream"));
			await ShGlobal.Write(FromStream, Input, Ct);
			await using (Content TextContent = await ShGlobal.Read(FromText, Ct)) {
				string Text = TextContent;
				Assert.IsTrue(Text == "from string");
			}
			await using (Content StreamContent = await ShGlobal.Read(FromStream, Ct)) {
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

