using Meziantou.Framework.Globbing;
using GlobPattern = Meziantou.Framework.Globbing.Glob;

namespace Tsinswreng.CsSh;

public partial class Sh{
	/// Keeps the shell-style basename result as a Cssh Path value.
	public partial Pth BaseName(Pth Path) {
		var Trimmed = ((str)Path).TrimEnd('/', '\\');
		return new(System.IO.Path.GetFileName(Trimmed));
	}

	/// Keeps the shell-style dirname result as a Cssh Path value.
	public partial Pth DirName(Pth Path) {
		var Trimmed = ((str)Path).TrimEnd('/', '\\');
		return new(NormalizePath(System.IO.Path.GetDirectoryName(Trimmed) ?? ""));
	}

	/// Resolves a relative value through this Sh instance rather than the process-wide current directory.
	public partial Pth FullPath(Pth Path) {
		return new(NormalizePath(System.IO.Path.GetFullPath(Path, CurrentDirectory)));
	}

	public partial bool Exists(Pth Path) {
		return Exists(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<bool> Exists(Pth Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		var FileSystemPath = (str)FullPath(Path);
		return Task.FromResult(File.Exists(FileSystemPath) || Directory.Exists(FileSystemPath));
	}

	public partial FileSystemInfo? FsInfo(Pth Path) {
		return FsInfo(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public async partial Task<FileSystemInfo?> FsInfo(Pth Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		var FileSystemPath = (str)FullPath(Path);
		FileSystemInfo? Result;
		try {
			// GetAttributes preserves real access errors while allowing a missing path to map to null.
			var Attributes = File.GetAttributes(FileSystemPath);
			Result = Attributes.HasFlag(FileAttributes.Directory)
				? new DirectoryInfo(FileSystemPath)
				: new FileInfo(FileSystemPath);
		}
		catch (FileNotFoundException) {
			Result = null;
		}
		catch (DirectoryNotFoundException) {
			Result = null;
		}
		await Task.CompletedTask.ConfigureAwait(false);
		return Result;
	}

	public partial bool IsFile(Pth Path) {
		return FsInfo(Path) is FileInfo;
	}

	public async partial Task<bool> IsFile(Pth Path, CT Ct) {
		return await FsInfo(Path, Ct).ConfigureAwait(false) is FileInfo;
	}

	public partial bool IsDir(Pth Path) {
		return FsInfo(Path) is DirectoryInfo;
	}

	public async partial Task<bool> IsDir(Pth Path, CT Ct) {
		return await FsInfo(Path, Ct).ConfigureAwait(false) is DirectoryInfo;
	}

	public partial void Mkdir(Pth Path) {
		Mkdir(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<nil> Mkdir(Pth Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		Directory.CreateDirectory(FullPath(Path));
		return Task.FromResult(NIL);
	}

	public partial void Rm(Pth Path) {
		Rm(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<nil> Rm(Pth Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		var FileSystemPath = (str)FullPath(Path);
		FileAttributes Attributes;
		try {
			// Do not use File.Exists/Directory.Exists here: both APIs intentionally
			// return false for some access failures, which would turn a real deletion
			// error into a false "missing path" success.
			Attributes = File.GetAttributes(FileSystemPath);
		}
		catch (FileNotFoundException) {
			// The -f part of rm -rf only suppresses an absent target.
			return Task.FromResult(NIL);
		}
		catch (DirectoryNotFoundException) {
			// A missing parent also means that the requested target is absent.
			return Task.FromResult(NIL);
		}
		if (Attributes.HasFlag(FileAttributes.Directory)) {
			// Directory.Delete removes a directory link itself rather than walking its target.
			Directory.Delete(FileSystemPath, recursive: true);
		}
		else {
			File.Delete(FileSystemPath);
		}
		return Task.FromResult(NIL);
	}

	public partial void Cp(Pth Source, Pth Destination, CpOptions? Options) {
		Cp(Source, Destination, Options, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<nil> Cp(Pth Source, Pth Destination, CT Ct) {
		return Cp(Source, Destination, null, Ct);
	}

	/// Copies a file, directory tree, or glob matches while preserving the caller's overwrite policy.
	public partial async Task<nil> Cp(Pth Source, Pth Destination, CpOptions? Options, CT Ct) {
		// step 1: observe cancellation before resolving paths or changing the file system.
		Ct.ThrowIfCancellationRequested();
		var Overwrite = Options?.Overwrite ?? false;
		if (HasGlob(Source)) {
			await CopyMatches(Source, Destination, Overwrite, Ct).ConfigureAwait(false);
			return NIL;
		}
		var SourcePath = (str)FullPath(Source);
		var DestinationPath = (str)FullPath(Destination);
		if (File.Exists(SourcePath)) {
			DestinationPath = ResolveDestinationPath(SourcePath, DestinationPath);
			ValidateSourceAndDestination(SourcePath, DestinationPath, false);
			await CopyFile(SourcePath, DestinationPath, Overwrite, Ct).ConfigureAwait(false);
		}
		else if (Directory.Exists(SourcePath)) {
			DestinationPath = ResolveDestinationPath(SourcePath, DestinationPath);
			ValidateSourceAndDestination(SourcePath, DestinationPath, true);
			await CopyDirectory(SourcePath, DestinationPath, Overwrite, Ct).ConfigureAwait(false);
		}
		else {
			throw new FileNotFoundException("Source path does not exist.", SourcePath);
		}
		return NIL;
	}

	public partial void Mv(Pth Source, Pth Destination, MvOptions? Options) {
		Mv(Source, Destination, Options, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<nil> Mv(Pth Source, Pth Destination, CT Ct) {
		return Mv(Source, Destination, null, Ct);
	}

	/// Moves a file or directory after validating that replacement cannot target the source itself.
	public partial async Task<nil> Mv(Pth Source, Pth Destination, MvOptions? Options, CT Ct) {
		// step 1: reject cancellation before any destination replacement can occur.
		Ct.ThrowIfCancellationRequested();
		var Overwrite = Options?.Overwrite ?? false;
		var SourcePath = (str)FullPath(Source);
		var DestinationPath = (str)FullPath(Destination);
		if (!File.Exists(SourcePath) && !Directory.Exists(SourcePath)) {
			throw new FileNotFoundException("Source path does not exist.", SourcePath);
		}
		if (File.Exists(SourcePath)) {
			DestinationPath = ResolveDestinationPath(SourcePath, DestinationPath);
			ValidateSourceAndDestination(SourcePath, DestinationPath, false);
			EnsureParentDirectory(DestinationPath);
			File.Move(SourcePath, DestinationPath, Overwrite);
		}
		else {
			DestinationPath = ResolveDestinationPath(SourcePath, DestinationPath);
			ValidateSourceAndDestination(SourcePath, DestinationPath, true);
			EnsureParentDirectory(DestinationPath);
			if (Directory.Exists(DestinationPath)) {
				if (!Overwrite)
					throw new IOException("Destination directory already exists.");
				Directory.Delete(DestinationPath, recursive: true);
			}
			Directory.Move(SourcePath, DestinationPath);
		}
		Ct.ThrowIfCancellationRequested();
		await Task.CompletedTask.ConfigureAwait(false);
		return NIL;
	}

	public partial IEnumerable<Pth> Glob(Pth Pattern) {
		// Preserve the caller's type selector before BCL path operations erase or reinterpret it.
		var IsDirectoryPattern = ((str)Pattern).EndsWith('/') || ((str)Pattern).EndsWith('\\');
		// Expand from this Sh instance's CWD, then select a static ancestor that the library can enumerate.
		var FullPattern = (str)FullPath(Pattern);
		var Root = GetGlobSearchRoot(FullPattern, IsDirectoryPattern);
		var RelativePattern = NormalizePath(System.IO.Path.GetRelativePath(Root, FullPattern));
		if (IsDirectoryPattern) {
			// Path.GetRelativePath removes a trailing separator from ordinary directories, but the
			// Standard dialect uses that separator to select directory matches.
			RelativePattern += "/";
		}
		// The glob library owns matching and recursion pruning. Cssh only adapts the Cssh
		// absolute pattern to the library's root-relative input, then wraps output as Pth.
		var PatternOptions = OperatingSystem.IsWindows() ? GlobOptions.IgnoreCase : GlobOptions.None;
		var ParsedPattern = GlobPattern.Parse(RelativePattern, GlobDialect.Standard, PatternOptions);
		var Options = new EnumerationOptions {
			RecurseSubdirectories = true,
			AttributesToSkip = FileAttributes.None,
			ReturnSpecialDirectories = false,
		};
		return ParsedPattern
			.EnumerateFileSystemEntries(Root, Options)
			.Select(Entry => new Pth(NormalizePath(Entry)));
	}

	public partial IEnumerable<Pth> Ls(Pth? Path, LsOptions? Options) {
		// Keep Directory's lazy enumeration; Select only changes each yielded string into Cssh's path type.
		var Root = (str)FullPath(Path ?? new("."));
		return Directory
			.EnumerateFileSystemEntries(Root, "*", MkLsEnumerationOptions(Options))
			.Select(Entry => new Pth(NormalizePath(Entry)));
	}

	public partial IEnumerable<Pth> LsDir(Pth? Path, LsOptions? Options) {
		// Use the BCL's directory-specific enumerator so callers do not need a second IsDir lookup.
		var Root = (str)FullPath(Path ?? new("."));
		return Directory
			.EnumerateDirectories(Root, "*", MkLsEnumerationOptions(Options))
			.Select(Entry => new Pth(NormalizePath(Entry)));
	}

	public partial IEnumerable<Pth> LsFile(Pth? Path, LsOptions? Options) {
		// Use the BCL's file-specific enumerator so callers do not need a second IsFile lookup.
		var Root = (str)FullPath(Path ?? new("."));
		return Directory
			.EnumerateFiles(Root, "*", MkLsEnumerationOptions(Options))
			.Select(Entry => new Pth(NormalizePath(Entry)));
	}

	public partial Content Read(Pth Path) {
		return Read(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<Content> Read(Pth Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		Stream Result = new FileStream(FullPath(Path), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
		return Task.FromResult<Content>(new(Result, new(LeaveOpen: false)));
	}

	public partial void Write(Pth Path, Content Source) {
		Write(Path, Source, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial void Write(str Path, Content Source) {
		Write(new Tsinswreng.CsSh.Pth(Path), Source);
	}

	public partial async Task<nil> Write(Pth Path, Content Source, CT Ct) {
		ArgumentNullException.ThrowIfNull(Source);
		var FileSystemPath = (str)FullPath(Path);
		EnsureParentDirectory(FileSystemPath);
		await using var Target = new FileStream(FileSystemPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await Source.Stream.CopyToAsync(Target, Ct).ConfigureAwait(false);
		return NIL;
	}

	public partial Task<nil> Write(str Path, Content Source, CT Ct) {
		return Write(new Tsinswreng.CsSh.Pth(Path), Source, Ct);
	}

	public partial void Append(Pth Path, Content Source) {
		Append(Path, Source, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial void Append(str Path, Content Source) {
		Append(new Tsinswreng.CsSh.Pth(Path), Source);
	}

	public partial async Task<nil> Append(Pth Path, Content Source, CT Ct) {
		ArgumentNullException.ThrowIfNull(Source);
		var FileSystemPath = (str)FullPath(Path);
		EnsureParentDirectory(FileSystemPath);
		await using var Target = new FileStream(FileSystemPath, FileMode.Append, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await Source.Stream.CopyToAsync(Target, Ct).ConfigureAwait(false);
		return NIL;
	}

	public partial Task<nil> Append(str Path, Content Source, CT Ct) {
		return Append(new Tsinswreng.CsSh.Pth(Path), Source, Ct);
	}

	/// Copies one file and creates its parent directory; existing files require explicit overwrite permission.
	private async partial Task CopyFile(str Source, str Destination, bool Overwrite, CT Ct) {
		// Check cancellation before creating parent directories or opening either file.
		Ct.ThrowIfCancellationRequested();
		EnsureParentDirectory(Destination);
		if (File.Exists(Destination) && !Overwrite)
			throw new IOException("Destination file already exists.");
		await using var Input = new FileStream(Source, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
		await using var Output = new FileStream(Destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await Input.CopyToAsync(Output, Ct).ConfigureAwait(false);
	}

	/// Copies a complete directory tree, including empty directories, without following a destination into the source.
	private async partial Task CopyDirectory(str Source, str Destination, bool Overwrite, CT Ct) {
		// Check cancellation before replacing an existing destination tree.
		Ct.ThrowIfCancellationRequested();
		if (Directory.Exists(Destination)) {
			if (!Overwrite)
				throw new IOException("Destination directory already exists.");
			Directory.Delete(Destination, recursive: true);
		}
		Directory.CreateDirectory(Destination);
		// Copy empty directories too: copying only files would silently alter a directory tree.
		foreach (var SourceDirectory in Directory.EnumerateDirectories(Source, "*", SearchOption.AllDirectories)) {
			// Keep cancellation responsive between directory creations.
			Ct.ThrowIfCancellationRequested();
			var Relative = System.IO.Path.GetRelativePath(Source, SourceDirectory);
			Directory.CreateDirectory(System.IO.Path.Combine(Destination, Relative));
		}
		foreach (var SourceFile in Directory.EnumerateFiles(Source, "*", SearchOption.AllDirectories)) {
			// Keep cancellation responsive between file copies.
			Ct.ThrowIfCancellationRequested();
			var Relative = System.IO.Path.GetRelativePath(Source, SourceFile);
			await CopyFile(SourceFile, System.IO.Path.Combine(Destination, Relative), false, Ct).ConfigureAwait(false);
		}
	}

	/// Copies every glob match as a direct child of Destination, preserving Bash's source/* shape.
	/// Copies glob matches as direct children of the destination, applying the same overwrite rule to nested files.
	private async partial Task CopyMatches(str Source, str Destination, bool Overwrite, CT Ct) {
		// Do not create a destination for a call that was already cancelled.
		Ct.ThrowIfCancellationRequested();
		var DestinationPath = (str)FullPath(Destination);
		// A glob rooted in a directory must not copy into that same directory tree while it is being enumerated.
		var IsDirectoryPattern = Source.EndsWith('/') || Source.EndsWith('\\');
		var SourceRoot = GetGlobSourceRoot((str)FullPath(Source));
		ValidateSourceAndDestination(SourceRoot, DestinationPath, true);
		Directory.CreateDirectory(DestinationPath);
		var FoundAny = false;
		// A trailing slash already selects the library's directory mode. Do not append one
		// again, or every directory would be copied twice.
		if (IsDirectoryPattern) {
			foreach (var Entry in Glob(Source)) {
				FoundAny = true;
				await CopyDirectoryMerge(FullPath(Entry), FullPath(Destination / BaseName(Entry)), Overwrite, Ct).ConfigureAwait(false);
			}
		}
		else {
			// The un-suffixed library pattern yields files. Bash's source/* additionally
			// includes directories, which the trailing-slash library pattern yields lazily.
			foreach (var Entry in Glob(Source)) {
				FoundAny = true;
				await CopyFile(FullPath(Entry), FullPath(Destination / BaseName(Entry)), Overwrite, Ct).ConfigureAwait(false);
			}
			foreach (var Entry in Glob(new Pth(Source + "/"))) {
				FoundAny = true;
				await CopyDirectoryMerge(FullPath(Entry), FullPath(Destination / BaseName(Entry)), Overwrite, Ct).ConfigureAwait(false);
			}
		}
		if (!FoundAny)
			throw new FileNotFoundException("Source glob did not match any file-system entries.", Source);
	}

	/// Recursively merges Source into Destination for glob-copy semantics.
	/// Merges one glob-matched directory into its destination while preserving existing files unless overwrite is enabled.
	private async partial Task CopyDirectoryMerge(str Source, str Destination, bool Overwrite, CT Ct) {
		// step 1: create only the destination root; never delete an existing merge target.
		Directory.CreateDirectory(Destination);
		foreach (var SourceDirectory in Directory.EnumerateDirectories(Source, "*", SearchOption.AllDirectories)) {
			// Cancellation is checked between directory operations so a cancelled copy stops before the next mutation.
			Ct.ThrowIfCancellationRequested();
			var Relative = System.IO.Path.GetRelativePath(Source, SourceDirectory);
			Directory.CreateDirectory(System.IO.Path.Combine(Destination, Relative));
		}
		foreach (var SourceFile in Directory.EnumerateFiles(Source, "*", SearchOption.AllDirectories)) {
			// The caller's overwrite policy applies equally to files discovered below glob-matched directories.
			Ct.ThrowIfCancellationRequested();
			var Relative = System.IO.Path.GetRelativePath(Source, SourceFile);
			await CopyFile(SourceFile, System.IO.Path.Combine(Destination, Relative), Overwrite, Ct).ConfigureAwait(false);
		}
	}

	/// Rejects directory operations whose destination is the source itself or one of its descendants.
	private partial void ValidateSourceAndDestination(str SourcePath, str DestinationPath, bool SourceIsDirectory) {
		// A directory cannot be copied or moved into itself: the operation would recurse or delete its own source.
		if (SourceIsDirectory && IsSameOrDescendantPath(SourcePath, DestinationPath))
			throw new IOException("Source and destination paths overlap.");
	}

	/// Compares two absolute paths using platform case rules without resolving symbolic links.
	private static partial bool IsSameOrDescendantPath(str Root, str Candidate) {
		// Full paths are compared with the platform's case rules to avoid bypassing the safety check on Windows.
		var Comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
		var NormalizedRoot = System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(Root));
		var NormalizedCandidate = System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(Candidate));
		if (string.Equals(NormalizedRoot, NormalizedCandidate, Comparison))
			return true;
		var Relative = System.IO.Path.GetRelativePath(NormalizedRoot, NormalizedCandidate);
		return !string.Equals(Relative, ".", StringComparison.Ordinal)
			&& !Relative.Equals("..", Comparison)
			&& !Relative.StartsWith(".." + System.IO.Path.DirectorySeparatorChar, Comparison)
			&& !System.IO.Path.IsPathRooted(Relative);
	}

	/// Recognises every non-literal syntax prefix supported by the glob parser.
	private partial bool HasGlob(str Path) {
		return Path.IndexOfAny(['*', '?', '[', '{']) >= 0;
	}

	private partial str GetGlobSearchRoot(str FullPattern, bool IsDirectoryPattern) {
		// This scan is only for choosing an enumeration root; parsing and matching remain library-owned.
		var MagicIndex = FullPattern.IndexOfAny(['*', '?', '[', '{']);
		if (MagicIndex < 0) {
			// Trim here because GetDirectoryName("folder/") denotes folder itself on Windows,
			// while a literal directory pattern must start enumerating at folder's parent.
			var LiteralPath = IsDirectoryPattern
				? System.IO.Path.TrimEndingDirectorySeparator(FullPattern)
				: FullPattern;
			// A literal directory pattern must be relative to its parent so the relative
			// pattern can retain the final slash that selects directory mode.
			return NormalizePath(System.IO.Path.GetDirectoryName(LiteralPath) ?? (IsDirectoryPattern ? LiteralPath : CurrentDirectory));
		}
		// Meziantou rejects patterns that begin with "..". Enumerating from the static parent
		// keeps the library pattern relative without reimplementing glob parsing.
		var StaticPrefix = FullPattern[..MagicIndex];
		return NormalizePath(System.IO.Path.GetDirectoryName(StaticPrefix) ?? System.IO.Path.GetPathRoot(FullPattern)!);
	}

	/// Extracts the static directory prefix used to reject glob copies into their own source tree.
	private partial str GetGlobSourceRoot(str FullPattern) {
		// Everything before the first wildcard is a static source directory. Keeping it
		// separate from the enumeration root avoids rejecting valid sibling destinations.
		var MagicIndex = FullPattern.IndexOfAny(['*', '?', '[', '{']);
		if (MagicIndex < 0)
			return NormalizePath(System.IO.Path.GetDirectoryName(FullPattern) ?? CurrentDirectory);
		var Prefix = FullPattern[..MagicIndex].TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
		return NormalizePath(string.IsNullOrEmpty(Prefix) ? CurrentDirectory : Prefix);
	}

	private partial str ResolveDestinationPath(str SourcePath, str DestinationPath) {
		if (!Directory.Exists(DestinationPath))
			return DestinationPath;
		var SourceName = System.IO.Path.GetFileName(SourcePath.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
		return System.IO.Path.Combine(DestinationPath, SourceName);
	}

	private partial EnumerationOptions MkLsEnumerationOptions(LsOptions? Options) {
		return new() {
			RecurseSubdirectories = Options?.Recursive == true,
			// Shell listing should not silently omit entries merely because they are hidden.
			AttributesToSkip = FileAttributes.None,
			ReturnSpecialDirectories = false,
		};
	}

	private partial void EnsureParentDirectory(str FileSystemPath) {
		var Parent = System.IO.Path.GetDirectoryName(FileSystemPath);
		if (!string.IsNullOrEmpty(Parent))
			Directory.CreateDirectory(Parent);
	}
}
