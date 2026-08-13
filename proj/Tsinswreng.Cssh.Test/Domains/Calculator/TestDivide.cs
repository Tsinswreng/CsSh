using Tsinswreng.CsTreeTest;

namespace Cs.Test.Domains.Calculator;

public partial class TestCalculator {
	public void RegisterTryIntDivide(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestCalculator),
			[typeof(ICalculator)],
			[nameof(ICalculator.TryIntDivide)],
			"Divide"
		);

		var R = register.Register;
		var T = Assert.IsTrue;
		R("positive", async (o) => {
			T(Calculator.TryIntDivide(20, 4, out var R));
			T(R==5);
			return null;
		});
		R("Divide by zero", async (o) => {
			T(!Calculator.TryIntDivide(20, 0, out var R));
			return null;
		});
	}
}
