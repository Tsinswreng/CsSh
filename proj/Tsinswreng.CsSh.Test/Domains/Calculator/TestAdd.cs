using Tsinswreng.CsTreeTest;

namespace Cs.Test.Domains.Calculator;
// each part should only mainly test one function. in this part we test Add
public partial class TestCalculator {
	public void RegisterAdd(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestCalculator), // tester type
			[typeof(ICalculator)], // testee types
			[nameof(ICalculator.Add)], // testee fn names, must use nameof()
			"YourTestNamePrefix" // optional
		);
		var R = register.Register;
		var T = Assert.IsTrue;
		R("Add positive numbers", async (o) => {
			T(Calculator.Add(5, 3)==8);
			T(Calculator.Add(6, 4)==10);
			return null;
		});
		//you can change the props of `register` here and then you can still use `R` directly
		//e.g. register.UniqNamePrefix = "NewPrefix";
		R("Add positive and negative numbers", async (o) => {
			var r = Calculator.Add(5, -3);
			T(r==2);
			return null;
		});
	}
}
