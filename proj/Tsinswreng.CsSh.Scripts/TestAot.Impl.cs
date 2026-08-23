using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Tsinswreng.CsSh.Scripts;

/// TestAot 的流程實現；NativeAOT 開關由專案的 Directory.Build.props 統一提供。
internal static partial class TestAot{
	/// 依作業系統選擇 RID，允許 AOT_TEST_RID 覆蓋預設值以便 CI 或交叉編譯。
	internal static async partial Task Main(str Root, CT Ct){
		var TestProject = Root/"proj/Tsinswreng.CsSh.Test/Tsinswreng.CsSh.Test.csproj";
		var Rid = GetEnv("AOT_TEST_RID");
		if(String.IsNullOrWhiteSpace(Rid)){
			Rid = OperatingSystem.IsWindows() ? "win-x64" : "linux-x64";
		}

		// step 1: 以 Release 配置發布 NativeAOT 測試 executable。
		await Exe("dotnet",[
			"publish", TestProject,
			"-c", "Release",
			"-r", Rid,
		], Ct);

		// step 2: 執行 publish 目錄中的原生測試程式，確保 AOT 產物本身可用。
		//TODO .net10.0 弄成全局變量 不要硬編碼
		var PublishDir = Root/"proj/Tsinswreng.CsSh.Test/bin/Release/net10.0"/Rid/"publish";
		var ExecutableName = OperatingSystem.IsWindows() ? "Tsinswreng.CsSh.Test.exe" : "Tsinswreng.CsSh.Test";
		await Exe(PublishDir/ExecutableName, [], Ct);
	}
}
