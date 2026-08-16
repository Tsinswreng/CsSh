using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsTreeTest;

namespace Cs.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr = null!;
	public static async Task Main(string[] args){
		//SvcColct.SetupMyDi();
		// Cssh is a static script library; its tests intentionally require no services.

		var mgr = CsTestMgr.Inst;
		SvcProvdr = mgr.InitSvc(SvcColct, sc => sc.BuildServiceProvider());

		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
	}
}
