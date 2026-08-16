using Tsinswreng.CsTreeTest;

namespace Cs.Test.Domains.Cssh;

/// Groups public Cssh tests. Each API remains in its own partial source file.
public partial class TestCssh:ITester{
	/// Registers tests serially because every case owns and cleans a temporary directory.
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		Node ??= new TestNode();
		Node.Ordered = true;
		Node.IsParallelRecursive = false;
		RegisterExtnString(Node);
		RegisterMkdirAndRm(Node);
		RegisterCpAndMv(Node);
		RegisterLsAndFind(Node);
		RegisterReadAndWrite(Node);
		RegisterX(Node);
		RegisterTryX(Node);
		return Node;
	}
}
