using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Groups public Cssh tests. Each API remains in its own partial source file.
public partial class TestCssh:ITester{
	/// Registers tests serially because every case owns and cleans a temporary directory.
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		Node ??= new TestNode();
		Node.Ordered = true;
		Node.IsParallelRecursive = false;
		RegisterIntegratedWorkflow(Node);
		RegisterExtnString(Node);
		RegisterNormalizePath(Node);
		RegisterBaseName(Node);
		RegisterDirName(Node);
		RegisterFullPath(Node);
		RegisterMkdirAndRm(Node);
		RegisterCpAndMv(Node);
		RegisterLsAndFind(Node);
		RegisterFsInfo(Node);
		RegisterReadAndWrite(Node);
		RegisterX(Node);
		RegisterText(Node);
		RegisterTryX(Node);
		return Node;
	}
}

