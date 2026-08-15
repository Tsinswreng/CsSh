namespace Tsinswreng.Cssh;

public static partial class Sh{
	/// 取得当前进程的环境变量；不存在时返回 null。
	public static partial str? GetEnv(str Name);

	/// 设置当前进程的环境变量。
	/// 若只需影响一次外部调用，应改用 CommandSpec.EnvironmentVariables，避免污染脚本后续步骤。
	public static partial void SetEnv(str Name, str Value);

	/// 从当前进程的环境变量中移除指定名称。
	public static partial void UnsetEnv(str Name);
}
