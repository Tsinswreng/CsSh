namespace Tsinswreng.CsSh;

public partial class Sh{
	/// 取得当前进程的环境变量；不存在时返回 null。
	public partial str? GetEnv(str Name);

	/// 设置当前进程的环境变量。
	/// 若只需影响一次外部调用，应在创建 X 命令时传入专用环境变量设置，避免污染脚本后续步骤。
	public partial void SetEnv(str Name, str Value);

	/// 从当前进程的环境变量中移除指定名称。
	public partial void UnsetEnv(str Name);
}
