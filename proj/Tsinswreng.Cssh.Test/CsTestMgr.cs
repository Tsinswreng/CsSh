using Tsinswreng.CsTreeTest;
using Cs.Test.Domains.Cssh;
namespace Cs.Test;

public class CsTestMgr:DiEtTestMgr{
	public static CsTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestCssh>();
		return Node;
	}
}
