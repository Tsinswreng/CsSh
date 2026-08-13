//we suggest one tester class corresponds to one testee class.
// and put different tester classes in separates folders.
//now we are in Calculator/ , this is only for `TestCalculator`
using Tsinswreng.CsTreeTest;

namespace Cs.Test.Domains.Calculator;

//Use partial class. this is the main part. the file name of main part should start with _
// each part of the class should only test one method
// the main part should not do test but assemble the test cases in other parts.
public partial class TestCalculator : ITester {
	ICalculator Calculator;
	public TestCalculator(
		ICalculator Calculator
	) {
		this.Calculator = Calculator;
	}//we use DependencyInjection here
	// In some scenarios we use di, in other cases
	// we can directly new the testee class without di
	
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		Node ??= new TestNode();
		//default is false, if set to true, the tests in this node will run in order.
		Node.Ordered = false;
		//default is false, which means all recursive childred nodes will be run in one thread
		Node.IsParallelRecursive = false;
		// when you do unit test e.g test pure functions without side-effect,
		// you can set the node Node.Ordered = true; Node.IsParallelRecursive = true;
		RegisterAdd(Node);
		RegisterTryIntDivide(Node);
		return Node;
	}
}
