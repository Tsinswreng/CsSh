namespace Cs.Test.Domains.Calculator;
/// example Testee class. this is a sample proj , we put it here for convenience
/// actually testee class are not in the same csproj as test code
public interface ICalculator {
	public int Add(int a, int b);
	public bool TryIntDivide(int a, int b, out int R);
}
public class Calculator:ICalculator{
	public int Add(int a, int b) {
		return a + b;
	}
	public bool TryIntDivide(int a, int b, out int R) {
		if (b == 0) {
			R = default;
			return false;
		}
		R = a / b;
		return true;
	}
}
