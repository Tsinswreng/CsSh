using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Shows the intended one-case CsSh workflow after the Content declaration redesign.
public partial class TestCssh{
	public partial void RegisterIntegratedWorkflow(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestCssh),
			[typeof(Sh), typeof(ExtnString)],
			[nameof(Mkdir), nameof(Write), nameof(Read), nameof(Append), nameof(Cp), nameof(Mv), nameof(Ls), nameof(Find), nameof(X), nameof(TryX), nameof(Rm)],
			"Integrated").Register;
		Register(nameof(IntegratedWorkflowCreatesUsesAndRemovesEntrySideData), IntegratedWorkflowCreatesUsesAndRemovesEntrySideData!);
	}

	/// This is deliberately one test case: setup, Content conversion, stream transfer, command transfer and cleanup share one owned directory lifecycle.
	public async partial Task<object?> IntegratedWorkflowCreatesUsesAndRemovesEntrySideData(object? O) {
		// Remove a stale artefact from an interrupted earlier run, then establish this case's owned root.
		var T = Assert.IsTrue;
		using var CtSource = new CancellationTokenSource();
		var Ct = CtSource.Token;
		await Rm(IntegratedRoot, Ct);
		await Mkdir(IntegratedRoot, Ct);
		try {
			// string enters the unified API through its implicit string-to-Content conversion.
			var InputDir = IntegratedRoot / "input";
			var SourceFile = InputDir / "message.txt";
			await Write(SourceFile, "CsSh integration", Ct);
			await using (Content TextContent = await Read(SourceFile, Ct)) {
				string Text = TextContent;
				T(Text == "CsSh integration");
			}

			// Ls reports item type; Cp/Mv then produce the state for Find to inspect.
			var Entries = new Dictionary<str, FileSystemEntry>();
			await foreach (var Entry in Ls(IntegratedRoot, Ct)) {
				Entries.Add(Entry.Name, Entry);
			}
			T(Entries["input"].IsDir);
			var CopiedFile = IntegratedRoot / "output" / "copy.txt";
			var MovedFile = IntegratedRoot / "output" / "final.txt";
			await Cp(SourceFile, CopiedFile, Ct);
			await Mv(CopiedFile, MovedFile, Ct);
			T(!await Exists(CopiedFile, Ct));
			await using (Content MovedContent = await Read(MovedFile, Ct)) {
				string Text = await MovedContent.Text(Ct);
				T(Text == "CsSh integration");
			}
			var TextFiles = new List<str>();
			await foreach (var Entry in Find(IntegratedRoot / "**/*.txt", Ct)) {
				TextFiles.Add(Entry.Name);
			}
			T(TextFiles.Order().SequenceEqual(["final.txt", "message.txt"]));

			// Append uses exactly the same Content source shape as Write; only the target open mode differs.
			var LogFile = IntegratedRoot / "output" / "log.txt";
			await Write(LogFile, "first\n", Ct);
			await Append(LogFile, "second\n", Ct);
			await using (Content LogContent = await Read(LogFile, Ct)) {
				T((await LogContent.Text(Ct)) == "first\nsecond\n");
			}

			// Ordinary Stream also enters Write implicitly through Content, without a second file API.
			var StreamCopy = IntegratedRoot / "output" / "stream-copy.txt";
			await using (Content SourceContent = await Read(SourceFile, Ct)) {
				Stream SourceStream = SourceContent;
				await Write(StreamCopy, SourceStream, Ct);
			}
			await using (Content StreamCopyContent = await Read(StreamCopy, Ct)) {
				T((string)StreamCopyContent == "CsSh integration");
			}

			// Command output is Content too, so it can be written straight to a file with no XOut or stream-specific overload.
			var VersionFile = IntegratedRoot / "output" / "dotnet-version.txt";
			await using (var Version = X("dotnet --version", Ct)) {
				await Write(VersionFile, Version.Result.Stdout, Ct);
				T((await Version.Done).IsSuccess);
			}
			await using (Content VersionContent = await Read(VersionFile, Ct)) {
				T(!string.IsNullOrWhiteSpace(await VersionContent.Text(Ct)));
			}
			await using (var Failed = TryX("dotnet cssh-command-that-does-not-exist", Ct)) {
				T(!(await Failed.Done).IsSuccess);
			}
		}
		finally {
			// The same test case owns teardown, so a failed mid-workflow assertion cannot leave its data behind.
			await Rm(IntegratedRoot, Ct);
		}
		T(!await Exists(IntegratedRoot, Ct));
		return null;
	}
}

