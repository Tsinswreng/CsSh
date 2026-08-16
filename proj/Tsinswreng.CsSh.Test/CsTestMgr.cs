using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSh.Test.Domains.CsSh;
namespace Tsinswreng.CsSh.Test;

public class CsTestMgr:DiEtTestMgr{
	public static CsTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestCssh>();
		return Node;
	}
}
