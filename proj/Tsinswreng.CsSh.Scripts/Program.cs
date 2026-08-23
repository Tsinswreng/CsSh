namespace Tsinswreng.CsSh.Scripts;

using System.Runtime.CompilerServices;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

/// CsSh 維護腳本的命令列入口。
/// 第一個引數選擇具體腳本；腳本本身負責完整的一次性流程。
internal static partial class Program{
	/// 將命令列入口分派至具名腳本。
	internal static async Task Main(str[] Args){
		var Ct = default(CT);
		var Root = FullPath(DirName(OwnPath())/"../..");

		if(Args.Length == 0){
			PrintUsage();
			return;
		}

		switch(Args[0]){
			case nameof(Release):
				await Release.Main(Root, Ct);
				break;
			case nameof(TestAot):
				await TestAot.Main(Root, Ct);
				break;
			default:
				throw new ArgumentException($"Unknown CsSh script: {Args[0]}.", nameof(Args));
		}
	}

	/// 列出可由 dotnet run -- <entry> 呼叫的腳本名稱。
	private static void PrintUsage(){
		Console.Error.WriteLine("Usage: dotnet run --project proj/Tsinswreng.CsSh.Scripts -- <entry>");
		Console.Error.WriteLine("Entries: Release, TestAot");
	}

	/// 讓編譯器提供腳本源文件路徑，故腳本不依賴啟動時的當前目錄。
	private static str OwnPath([CallerFilePath] str CallerPath = ""){
		return CallerPath;
	}
}
