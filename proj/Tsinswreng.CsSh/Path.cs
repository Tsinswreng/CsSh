namespace Tsinswreng.CsSh;

/// 專表路徑之類型。
/// 可添成員/擴展于其上而無需污染string類型
public struct Path{
	public string Value{get;set;}
	public Path(string Value){
		this.Value = Value;
	}
	
	public static implicit operator string(Path z){
		return z.Value;
	}
	
	public static implicit operator Path(string z){
		return new Path(z);
	}
	
	public static Path operator / (
		Path a, string b
	){
		throw new NotImplementedException();
	}
	
	public static Path operator / (
		Path a, string b
	){
		throw new NotImplementedException();
	}
	
	public static Path operator / (
		Path a, Path b
	){
		throw new NotImplementedException();
	}
	
}