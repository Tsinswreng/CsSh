using System.Text;
using Tsinswreng.CsTreeTest;
using Tsinswreng.Cssh;

namespace Cs.Test.Domains.Cssh;

/// Tests successful, lazy external command execution through the public X entry point.
public partial class TestCssh{
	public void RegisterX(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Sh)], [nameof(Sh.X)], "Command").Register;
		Register(nameof(XStartsWhenDoneIsObservedAndReturnsStdout), XStartsWhenDoneIsObservedAndReturnsStdout!);
		Register(nameof(XPassesExternalStreamAsStdin), XPassesExternalStreamAsStdin!);
	}

	/// Observing Done starts the lazy process; its stdout remains consumable after exit.
	public async Task<object?> XStartsWhenDoneIsObservedAndReturnsStdout(object? O) {
		await using var Command = Sh.X("dotnet --version");
		var Exit = await Command.Done;
		using var Reader = new StreamReader(Command.Result.Stdout, Encoding.UTF8, leaveOpen: true);
		var Text = await Reader.ReadToEndAsync();
		Assert.IsTrue(Exit.IsSuccess);
		Assert.IsTrue(!string.IsNullOrWhiteSpace(Text));
		return null;
	}

	/// Command input belongs to CommandOptions, leaving Command itself as an output/result DTO.
	public async Task<object?> XPassesExternalStreamAsStdin(object? O) {
		await using var Input = new MemoryStream(Encoding.UTF8.GetBytes("stream-input"));
		await using var Command = Sh.X("dotnet --version", new(Input));
		var Exit = await Command.Done;
		Assert.IsTrue(Exit.IsSuccess);
		return null;
	}
}
