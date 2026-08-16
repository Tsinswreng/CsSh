namespace Tsinswreng.CsSh;

public partial class Sh{
	public partial str? GetEnv(str Name) {
		return Environment.GetEnvironmentVariable(Name);
	}

	public partial void SetEnv(str Name, str Value) {
		Environment.SetEnvironmentVariable(Name, Value);
	}

	public partial void UnsetEnv(str Name) {
		Environment.SetEnvironmentVariable(Name, null);
	}
}

