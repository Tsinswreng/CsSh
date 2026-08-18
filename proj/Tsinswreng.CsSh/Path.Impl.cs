namespace Tsinswreng.CsSh;

/// Implements path-value construction, conversion, and separator-normalizing composition.
public readonly partial struct Pth{
	public partial Pth(str Value) {
		ArgumentNullException.ThrowIfNull(Value);
		this.Value = Value;
	}

	public static partial Pth Join(str A, str B) {
		ArgumentNullException.ThrowIfNull(A);
		ArgumentNullException.ThrowIfNull(B);
		var Left = A.Replace('\\', '/');
		var Right = B.Replace('\\', '/');
		if (str.IsNullOrEmpty(Left)) {
			return new(Right.TrimStart('/'));
		}
		if (str.IsNullOrEmpty(Right)) {
			return new(Left.TrimEnd('/'));
		}
		return new(Left.TrimEnd('/') + "/" + Right.TrimStart('/'));
	}

	public override partial str ToString() {
		return Value;
	}
}
