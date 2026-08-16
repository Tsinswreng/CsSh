namespace Tsinswreng.CsSh;

/// Adds the path-join operator to ordinary strings for Cssh scripts.
/// The result always uses forward slashes, so it can be supplied unchanged to every Cssh path API.
public static class ExtnString{
	/// Extends string without introducing a wrapper type at script call sites.
	extension(string z){
		/// Joins two path segments without duplicating the separator at their boundary.
		/// An empty segment is ignored; a leading slash in B is treated as a separator rather than a new rooted path.
		public static string operator / (
			string a, string b
		){
			// Normalize first so Windows literals and Cssh's forward-slash paths compose identically.
			var Left = a.Replace('\\', '/');
			var Right = b.Replace('\\', '/');
			if (string.IsNullOrEmpty(Left))
				return Right.TrimStart('/');
			if (string.IsNullOrEmpty(Right))
				return Left.TrimEnd('/');

			// Keep an intentional root such as / or C:/, while owning exactly one joining separator.
			return Left.TrimEnd('/') + "/" + Right.TrimStart('/');
		}
	}
}
