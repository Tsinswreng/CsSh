using System.Text;
using Tsinswreng.CsTreeTest;
using Tsinswreng.Cssh;
using static Tsinswreng.Cssh.Sh;

namespace Cs.Test.Domains.Cssh;

/// Implements the one-case, stateful Cssh workflow test.
public partial class TestCssh{
	public partial void RegisterIntegratedWorkflow(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestCssh),
			[typeof(Sh), typeof(ExtnString)],
			[nameof(Mkdir), nameof(Write), nameof(Read), nameof(Cp), nameof(Mv), nameof(Ls), nameof(Find), nameof(OpenWrite), nameof(OpenAppend), nameof(X), nameof(TryX), nameof(Rm)],
			"Integrated").Register;
		Register(nameof(IntegratedWorkflowCreatesUsesAndRemovesEntrySideData), IntegratedWorkflowCreatesUsesAndRemovesEntrySideData!);
	}

	/// This is deliberately one test case: setup, all dependent checks, and cleanup share one owned directory lifecycle.
	public async partial Task<object?> IntegratedWorkflowCreatesUsesAndRemovesEntrySideData(object? O) {
		// Remove a stale artefact from an interrupted earlier run, then establish this case's owned root.
		var T = Assert.IsTrue;
		Rm(IntegratedRoot);
		Mkdir(IntegratedRoot);
		try {
			// Write and read ordinary text through a nested portable path.
			var InputDir = IntegratedRoot / "input";
			var SourceFile = InputDir / "message.txt";
			Write(SourceFile, "Cssh integration");
			T(Read(SourceFile) == "Cssh integration");

			// Ls reports item type; Cp/Mv then produce the state for Find to inspect.
			var Entries = Ls(IntegratedRoot).ToDictionary(Entry => Entry.Name);
			T(Entries["input"].IsDir);
			var CopiedFile = IntegratedRoot / "output" / "copy.txt";
			var MovedFile = IntegratedRoot / "output" / "final.txt";
			Cp(SourceFile, CopiedFile);
			Mv(CopiedFile, MovedFile);
			T(!Exists(CopiedFile));
			T(Read(MovedFile) == "Cssh integration");
			var TextFiles = Find(IntegratedRoot / "**/*.txt").Select(Entry => Entry.Name).Order().ToArray();
			T(TextFiles.SequenceEqual(["final.txt", "message.txt"]));

			// OpenWrite/OpenAppend together exercise stream redirection without keeping text data in memory.
			var LogFile = IntegratedRoot / "output" / "log.txt";
			await using (var Log = await OpenWrite(LogFile, CancellationToken.None)) {
				await using var First = new MemoryStream(Encoding.UTF8.GetBytes("first\n"));
				await Write(Log, First, CancellationToken.None);
			}
			await using (var Log = await OpenAppend(LogFile, CancellationToken.None)) {
				await using var Second = new MemoryStream(Encoding.UTF8.GetBytes("second\n"));
				await Write(Log, Second, CancellationToken.None);
			}
			T(Read(LogFile) == "first\nsecond\n");

			// Done starts the lazy process; consume stdout afterward to validate the complete command result flow.
			await using (var Version = X("dotnet --version")) {
				var Exit = await Version.Done;
				using var Reader = new StreamReader(Version.Result.Stdout, Encoding.UTF8, leaveOpen: true);
				T(Exit.IsSuccess && !string.IsNullOrWhiteSpace(await Reader.ReadToEndAsync()));
			}
			await using (var Failed = TryX("dotnet cssh-command-that-does-not-exist")) {
				T(!(await Failed.Done).IsSuccess);
			}
		}
		finally {
			// The same test case owns teardown, so a failed mid-workflow assertion cannot leave its data behind.
			Rm(IntegratedRoot);
		}
		T(!Exists(IntegratedRoot));
		return null;
	}
}
