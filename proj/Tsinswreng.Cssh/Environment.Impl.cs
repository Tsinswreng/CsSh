namespace Tsinswreng.Cssh;

public static partial class Sh{
	public static partial str? GetEnv(str Name) {
		return Environment.GetEnvironmentVariable(Name);
	}

	public static partial void SetEnv(str Name, str Value) {
		Environment.SetEnvironmentVariable(Name, Value);
	}

	public static partial void UnsetEnv(str Name) {
		Environment.SetEnvironmentVariable(Name, null);
	}
}
