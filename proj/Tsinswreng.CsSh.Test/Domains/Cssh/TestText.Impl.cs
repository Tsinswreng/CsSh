using Tsinswreng.CsSh;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Implements Command.Text terminal-consumption tests.
public partial class TestCssh{
	public partial void RegisterText(ITestNode Node) {
		var Register = Node.MkTestFnRegister(typeof(TestCssh), [typeof(Command)], [nameof(Command.Text)], "Command").Register;
		Register(nameof(TextReadsBothOutputs), TextReadsBothOutputs!);
		Register(nameof(CommandEnvironmentOverridesArePerCommand), CommandEnvironmentOverridesArePerCommand!);
	}

	/// A failing dotnet invocation writes diagnostics to stderr; TryCmd preserves its exit result for inspection.
	public async partial Task<object?> TextReadsBothOutputs(object? O) {
		using var CtSource = new CancellationTokenSource();
		var Ct = CtSource.Token;
		await using var Command = ShGlobal.TryCmd("dotnet", ["--definitely-invalid-option"], Ct);
		var Result = await Command.Text(Ct);
		Assert.IsTrue(!Result.Exit.IsSuccess);
		Assert.IsTrue(!string.IsNullOrWhiteSpace(Result.Stderr));
		return null;
	}

	/// Each command receives its own environment snapshot without changing the host process.
	public async partial Task<object?> CommandEnvironmentOverridesArePerCommand(object? O) {
		using var CtSource = new CancellationTokenSource();
		var Ct = CtSource.Token;
		var Name = "CSSH_REVIEW_ENV";
		var Value = "per-command-value";
		var Options = new CommandOptions(Env: new Dictionary<string, string?> {{Name, Value}});
		var Result = OperatingSystem.IsWindows()
			? await ShGlobal.TryCmd("cmd.exe", ["/c", "echo", $"%{Name}%"], Options, Ct).Text(Ct)
			: await ShGlobal.TryCmd("/bin/sh", ["-c", "printf '%s' \"$CSSH_REVIEW_ENV\""], Options, Ct).Text(Ct);
		Assert.IsTrue(Result.Exit.IsSuccess);
		Assert.IsTrue(Result.Stdout.Trim() == Value);
		return null;
	}
}
