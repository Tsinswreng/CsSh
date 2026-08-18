namespace Tsinswreng.CsSh;

/// 表示供 Cssh API 使用的檔案系統路徑。
/// 此類型保留與 string 的隱式互轉，以維持腳本中既有的字面量呼叫方式。
public readonly partial struct Pth{
	/// 路徑的原始字串值。
	public str Value{get;}

	/// 以原始字串建立路徑。
	public partial Pth(str Value);

	/// 取得供 BCL 與一般文字 API 使用的原始路徑值。
	/// C# 不支援 partial conversion operator，故此處僅作語法層轉發。
	public static implicit operator str(Pth z) {
		return z.Value;
	}

	/// 讓既有字串字面量可直接傳入 Cssh 的路徑 API。
	/// C# 不支援 partial conversion operator，故此處僅轉發至已分離的建構子。
	public static implicit operator Pth(str z) {
		return new Pth(z);
	}

	/// 將右側路徑片段附加到目前路徑，並統一使用正斜線。
	/// C# 不支援 partial operator，故此處僅轉發至已分離的 Join。
	public static Pth operator /(Pth A, str B) {
		return Join(A.Value, B);
	}

	/// 將兩個路徑附加為一條 Cssh 路徑。
	/// C# 不支援 partial operator，故此處僅轉發至已分離的 Join。
	public static Pth operator /(Pth A, Pth B) {
		return Join(A.Value, B.Value);
	}

	/// 對兩段路徑做不改變根語意的字串層級連接。
	public static partial Pth Join(str A, str B);

	/// 以原始路徑值表示此實例，供記錄與插值字串使用。
	public override partial str ToString();
}
