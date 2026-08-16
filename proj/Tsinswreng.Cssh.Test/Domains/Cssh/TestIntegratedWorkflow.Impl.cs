using System.Text;
using Tsinswreng.CsTreeTest;
using Tsinswreng.Cssh;

namespace Cs.Test.Domains.Cssh;

/// Implements the one-case, stateful Cssh workflow test.
public partial class TestCssh{
	public partial void RegisterIntegratedWorkflow(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestCssh),
			[typeof(Sh), typeof(ExtnString)],
			[nameof(Sh.Mkdir), nameof(Sh.Write), nameof(Sh.Read), nameof(Sh.Cp), nameof(Sh.Mv), nameof(Sh.Ls), nameof(Sh.Find), nameof(Sh.OpenWrite), nameof(Sh.OpenAppend), nameof(Sh.X), nameof(Sh.TryX), nameof(Sh.Rm)],
			"Integrated").Register;
		Register(nameof(IntegratedWorkflowCreatesUsesAndRemovesEntrySideData), IntegratedWorkflowCreatesUsesAndRemovesEntrySideData!);
	}

	/// This is deliberately one test case: setup, all dependent checks, and cleanup share one owned directory lifecycle.
	public async partial Task<object?> IntegratedWorkflowCreatesUsesAndRemovesEntrySideData(object? O) {
		// Remove a stale artefact from an interrupted earlier run, then establish this case's owned root.
		Sh.Rm(IntegratedRoot);
		Sh.Mkdir(IntegratedRoot);
		try {
			// Write and read ordinary text through a nested portable path.
			var InputDir = IntegratedRoot / "input";
			var SourceFile = InputDir / "message.txt";
			Sh.Write(SourceFile, "Cssh integration");
			Assert.IsTrue(Sh.Read(SourceFile) == "Cssh integration");

			// Ls reports item type; Cp/Mv then produce the state for Find to inspect.
			var Entries = Sh.Ls(IntegratedRoot).ToDictionary(Entry => Entry.Name);
			Assert.IsTrue(Entries["input"].IsDir);
			var CopiedFile = IntegratedRoot / "output" / "copy.txt";
			var MovedFile = IntegratedRoot / "output" / "final.txt";
			Sh.Cp(SourceFile, CopiedFile);
			Sh.Mv(CopiedFile, MovedFile);
			Assert.IsTrue(!Sh.Exists(CopiedFile));
			Assert.IsTrue(Sh.Read(MovedFile) == "Cssh integration");
			var TextFiles = Sh.Find(IntegratedRoot / "**/*.txt").Select(Entry => Entry.Name).Order().ToArray();
			Assert.IsTrue(TextFiles.SequenceEqual(["final.txt", "message.txt"]));

			// OpenWrite/OpenAppend together exercise stream redirection without keeping text data in memory.
			var LogFile = IntegratedRoot / "output" / "log.txt";
			await using (var Log = await Sh.OpenWrite(LogFile, CancellationToken.None)) {
				await using var First = new MemoryStream(Encoding.UTF8.GetBytes("first\n"));
				await Sh.Write(Log, First, CancellationToken.None);
			}
			await using (var Log = await Sh.OpenAppend(LogFile, CancellationToken.None)) {
				await using var Second = new MemoryStream(Encoding.UTF8.GetBytes("second\n"));
				await Sh.Write(Log, Second, CancellationToken.None);
			}
			Assert.IsTrue(Sh.Read(LogFile) == "first\nsecond\n");

			// Done starts the lazy process; consume stdout afterward to validate the complete command result flow.
			await using (var Version = Sh.X("dotnet --version")) {
				var Exit = await Version.Done;
				using var Reader = new StreamReader(Version.Result.Stdout, Encoding.UTF8, leaveOpen: true);
				Assert.IsTrue(Exit.IsSuccess && !string.IsNullOrWhiteSpace(await Reader.ReadToEndAsync()));
			}
			await using (var Failed = Sh.TryX("dotnet cssh-command-that-does-not-exist")) {
				Assert.IsTrue(!(await Failed.Done).IsSuccess);
			}
		}
		finally {
			// The same test case owns teardown, so a failed mid-workflow assertion cannot leave its data behind.
			Sh.Rm(IntegratedRoot);
		}
		Assert.IsTrue(!Sh.Exists(IntegratedRoot));
		return null;
	}
}
