namespace Tsinswreng.CsSh.Scripts;

using System.Runtime.CompilerServices;

/// CsSh 維護腳本的命令列入口。
/// 第一個引數選擇具體腳本；腳本本身負責完整的一次性流程。
internal static partial class Program{
	/// 將命令列入口分派至具名腳本，Ctrl+C 會透過同一取消令牌中止外部命令。
	internal static async Task Main(str[] Args){
		using var CtSource = new CancellationTokenSource();
		Console.CancelKeyPress += (_, Event) => {
			Event.Cancel = true;
			CtSource.Cancel();
		};

		var Ct = CtSource.Token;
		var CallerDir = System.IO.Path.GetDirectoryName(OwnPath())!;
		// Program.cs is at <root>/proj/Tsinswreng.CsSh.Scripts; resolve from the source path, never from cwd.
		var Root = System.IO.Path.GetFullPath(System.IO.Path.Combine(CallerDir, "../..")) + System.IO.Path.DirectorySeparatorChar;

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

	/// 讓編譯器把這個 dispatcher 原始檔路徑填入；CLR 的 Main 本身不會填 CallerFilePath。
	private static str OwnPath([CallerFilePath] str CallerPath = ""){
		return CallerPath;
	}
}
