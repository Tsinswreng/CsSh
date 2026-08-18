namespace Tsinswreng.CsSh;

/// Adds the path-join operator to ordinary strings for Cssh scripts.
/// The result always uses forward slashes, so it can be supplied unchanged to every Cssh path API.
public static class ExtnString{
	/// Extends string without introducing a wrapper type at script call sites.
	extension(str z){
		/// Joins two path segments without duplicating the separator at their boundary.
		/// An empty segment is ignored; a leading slash in B is treated as a separator rather than a new rooted path.
		public static Pth operator /(str A, str B){
			return Pth.Join(A, B);
		}
	}
}
