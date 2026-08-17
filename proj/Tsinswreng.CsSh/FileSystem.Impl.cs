using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tsinswreng.CsSh;

public partial class Sh{
	public partial bool Exists(str Path) {
		return Exists(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<bool> Exists(str Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		var FileSystemPath = NormalizeFileSystemPath(Path);
		return Task.FromResult(File.Exists(FileSystemPath) || Directory.Exists(FileSystemPath));
	}

	public partial void Mkdir(str Path) {
		Mkdir(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<nil> Mkdir(str Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		Directory.CreateDirectory(NormalizeFileSystemPath(Path));
		return Task.FromResult(NIL);
	}

	public partial void Rm(str Path) {
		Rm(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<nil> Rm(str Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		var FileSystemPath = NormalizeFileSystemPath(Path);
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

	public partial void Cp(str Source, str Destination, CpOptions? Options) {
		Cp(Source, Destination, Options, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<nil> Cp(str Source, str Destination, CT Ct) {
		return Cp(Source, Destination, null, Ct);
	}

	public partial async Task<nil> Cp(str Source, str Destination, CpOptions? Options, CT Ct) {
		var Overwrite = Options?.Overwrite ?? false;
		if (HasGlob(Source)) {
			await CopyMatches(Source, Destination, Overwrite, Ct).ConfigureAwait(false);
			return NIL;
		}
		var SourcePath = NormalizeFileSystemPath(Source);
		var DestinationPath = NormalizeFileSystemPath(Destination);
		if (File.Exists(SourcePath)) {
			DestinationPath = ResolveDestinationPath(SourcePath, DestinationPath);
			await CopyFile(SourcePath, DestinationPath, Overwrite, Ct).ConfigureAwait(false);
		}
		else if (Directory.Exists(SourcePath)) {
			DestinationPath = ResolveDestinationPath(SourcePath, DestinationPath);
			await CopyDirectory(SourcePath, DestinationPath, Overwrite, Ct).ConfigureAwait(false);
		}
		else {
			throw new FileNotFoundException("Source path does not exist.", SourcePath);
		}
		return NIL;
	}

	public partial void Mv(str Source, str Destination, MvOptions? Options) {
		Mv(Source, Destination, Options, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<nil> Mv(str Source, str Destination, CT Ct) {
		return Mv(Source, Destination, null, Ct);
	}

	public partial async Task<nil> Mv(str Source, str Destination, MvOptions? Options, CT Ct) {
		var Overwrite = Options?.Overwrite ?? false;
		var SourcePath = NormalizeFileSystemPath(Source);
		var DestinationPath = NormalizeFileSystemPath(Destination);
		if (!File.Exists(SourcePath) && !Directory.Exists(SourcePath)) {
			throw new FileNotFoundException("Source path does not exist.", SourcePath);
		}
		if (File.Exists(SourcePath)) {
			DestinationPath = ResolveDestinationPath(SourcePath, DestinationPath);
			EnsureParentDirectory(DestinationPath);
			File.Move(SourcePath, DestinationPath, Overwrite);
		}
		else {
			DestinationPath = ResolveDestinationPath(SourcePath, DestinationPath);
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

	public partial IEnumerable<FileSystemInfo> Glob(str Pattern) {
		var Regex = GlobToRegex(NormalizePath(NormalizeFileSystemPath(Pattern)));
		var Root = GlobRoot(Pattern);
		if (!Directory.Exists(Root))
			return [];
		return new DirectoryInfo(Root).EnumerateFileSystemInfos("*", SearchOption.AllDirectories)
			.Where(Entry => Regex.IsMatch(ToShellPath(Entry.FullName)));
	}

	public async partial IAsyncEnumerable<FileSystemInfo> Glob(str Pattern, [EnumeratorCancellation] CT Ct) {
		var Regex = GlobToRegex(NormalizePath(NormalizeFileSystemPath(Pattern)));
		var Root = GlobRoot(Pattern);
		if (!Directory.Exists(Root))
			yield break;
		foreach (var Entry in new DirectoryInfo(Root).EnumerateFileSystemInfos("*", SearchOption.AllDirectories)) {
			Ct.ThrowIfCancellationRequested();
			if (Regex.IsMatch(ToShellPath(Entry.FullName)))
				yield return Entry;
			await Task.Yield();
		}
	}

	public partial IEnumerable<FileSystemInfo> Ls(str? Path, LsOptions? Options) {
		var Root = NormalizeFileSystemPath(Path ?? ".");
		var Option = Options?.Recursive == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		return new DirectoryInfo(Root).EnumerateFileSystemInfos("*", Option);
	}

	public partial IAsyncEnumerable<FileSystemInfo> Ls(str? Path, CT Ct) {
		return Ls(Path, null, Ct);
	}

	public async partial IAsyncEnumerable<FileSystemInfo> Ls(str? Path, LsOptions? Options, [EnumeratorCancellation] CT Ct) {
		var Root = NormalizeFileSystemPath(Path ?? ".");
		var Option = Options?.Recursive == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		foreach (var Entry in new DirectoryInfo(Root).EnumerateFileSystemInfos("*", Option)) {
			Ct.ThrowIfCancellationRequested();
			yield return Entry;
			await Task.Yield();
		}
	}

	public partial Content Read(str Path) {
		return Read(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<Content> Read(str Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		Stream Result = new FileStream(NormalizeFileSystemPath(Path), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
		return Task.FromResult<Content>(new(Result, new(LeaveOpen: false)));
	}

	public partial void Write(str Path, Content Source) {
		Write(Path, Source, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial async Task<nil> Write(str Path, Content Source, CT Ct) {
		ArgumentNullException.ThrowIfNull(Source);
		var FileSystemPath = NormalizeFileSystemPath(Path);
		EnsureParentDirectory(FileSystemPath);
		await using var Target = new FileStream(FileSystemPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await Source.Stream.CopyToAsync(Target, Ct).ConfigureAwait(false);
		return NIL;
	}

	public partial void Append(str Path, Content Source) {
		Append(Path, Source, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial async Task<nil> Append(str Path, Content Source, CT Ct) {
		ArgumentNullException.ThrowIfNull(Source);
		var FileSystemPath = NormalizeFileSystemPath(Path);
		EnsureParentDirectory(FileSystemPath);
		await using var Target = new FileStream(FileSystemPath, FileMode.Append, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await Source.Stream.CopyToAsync(Target, Ct).ConfigureAwait(false);
		return NIL;
	}

	private async Task CopyFile(str Source, str Destination, bool Overwrite, CT Ct) {
		EnsureParentDirectory(Destination);
		if (File.Exists(Destination) && !Overwrite)
			throw new IOException("Destination file already exists.");
		await using var Input = new FileStream(Source, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
		await using var Output = new FileStream(Destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await Input.CopyToAsync(Output, Ct).ConfigureAwait(false);
	}

	private async Task CopyDirectory(str Source, str Destination, bool Overwrite, CT Ct) {
		if (Directory.Exists(Destination)) {
			if (!Overwrite)
				throw new IOException("Destination directory already exists.");
			Directory.Delete(Destination, recursive: true);
		}
		Directory.CreateDirectory(Destination);
		// Copy empty directories too: copying only files would silently alter a directory tree.
		foreach (var SourceDirectory in Directory.EnumerateDirectories(Source, "*", SearchOption.AllDirectories)) {
			var Relative = Path.GetRelativePath(Source, SourceDirectory);
			Directory.CreateDirectory(Path.Combine(Destination, Relative));
		}
		foreach (var SourceFile in Directory.EnumerateFiles(Source, "*", SearchOption.AllDirectories)) {
			var Relative = Path.GetRelativePath(Source, SourceFile);
			await CopyFile(SourceFile, Path.Combine(Destination, Relative), false, Ct).ConfigureAwait(false);
		}
	}

	/// Copies every glob match as a direct child of Destination, preserving Bash's source/* shape.
	private async Task CopyMatches(str Source, str Destination, bool Overwrite, CT Ct) {
		var DestinationPath = NormalizeFileSystemPath(Destination);
		Directory.CreateDirectory(DestinationPath);
		var FoundAny = false;
		await foreach (var Entry in Glob(Source, Ct)) {
			FoundAny = true;
			var Target = Destination / Entry.Name;
			if (Entry is DirectoryInfo) {
				// Bash's cp -r source/* destination merges a matching directory in destination.
				// It must not reject a second resource tree that extends the same folder.
				await CopyDirectoryMerge(Entry.FullName, Target, Ct).ConfigureAwait(false);
			}
			else {
				// A matched file is the direct source child: Bash replaces an existing counterpart.
				await CopyFile(Entry.FullName, NormalizeFileSystemPath(Target), Overwrite, Ct).ConfigureAwait(false);
			}
		}
		if (!FoundAny)
			throw new FileNotFoundException("Source glob did not match any file-system entries.", Source);
	}

	/// Recursively merges Source into Destination for glob-copy semantics.
	private async Task CopyDirectoryMerge(str Source, str Destination, CT Ct) {
		Directory.CreateDirectory(Destination);
		foreach (var SourceDirectory in Directory.EnumerateDirectories(Source, "*", SearchOption.AllDirectories)) {
			Ct.ThrowIfCancellationRequested();
			var Relative = Path.GetRelativePath(Source, SourceDirectory);
			Directory.CreateDirectory(Path.Combine(Destination, Relative));
		}
		foreach (var SourceFile in Directory.EnumerateFiles(Source, "*", SearchOption.AllDirectories)) {
			var Relative = Path.GetRelativePath(Source, SourceFile);
			await CopyFile(SourceFile, Path.Combine(Destination, Relative), true, Ct).ConfigureAwait(false);
		}
	}

	/// Glob detection is deliberately limited to the same wildcard characters supported by Find.
	private bool HasGlob(str Path) {
		return Path.IndexOfAny(['*', '?']) >= 0;
	}

	private str GlobRoot(str Pattern) {
		var NormalizedPattern = NormalizePath(Pattern);
		var MagicIndex = NormalizedPattern.IndexOfAny(['*', '?']);
		if (MagicIndex < 0) {
			var Parent = System.IO.Path.GetDirectoryName(NormalizeFileSystemPath(NormalizedPattern));
			return string.IsNullOrEmpty(Parent) ? NormalizeFileSystemPath(".") : Parent;
		}
		var Prefix = NormalizedPattern[..MagicIndex];
		var Separator = Prefix.LastIndexOf('/');
		var Root = Separator < 0 ? "." : Prefix[..Separator];
		return NormalizeFileSystemPath(string.IsNullOrEmpty(Root) ? "." : Root);
	}

	private str ResolveDestinationPath(str SourcePath, str DestinationPath) {
		if (!Directory.Exists(DestinationPath))
			return DestinationPath;
		var SourceName = System.IO.Path.GetFileName(SourcePath.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
		return System.IO.Path.Combine(DestinationPath, SourceName);
	}

	private Regex GlobToRegex(str Pattern) {
		var Builder = new StringBuilder("^");
		for (var Index = 0; Index < Pattern.Length; Index++) {
			var Character = Pattern[Index];
			if (Character == '*' && Index + 1 < Pattern.Length && Pattern[Index + 1] == '*') {
				Index++;
				if (Index + 1 < Pattern.Length && Pattern[Index + 1] == '/') {
					Index++;
					Builder.Append("(?:.*/)?");
				}
				else {
					Builder.Append(".*");
				}
			}
			else if (Character == '*') {
				Builder.Append("[^/]*");
			}
			else if (Character == '?') {
				Builder.Append("[^/]");
			}
			else {
				Builder.Append(Regex.Escape(Character.ToString()));
			}
		}
		Builder.Append('$');
		return new(Builder.ToString(), OperatingSystem.IsWindows() ? RegexOptions.IgnoreCase : RegexOptions.None);
	}

	private str ToShellPath(str FileSystemPath) {
		return NormalizePath(System.IO.Path.GetFullPath(FileSystemPath));
	}

	private void EnsureParentDirectory(str FileSystemPath) {
		var Parent = System.IO.Path.GetDirectoryName(FileSystemPath);
		if (!string.IsNullOrEmpty(Parent))
			Directory.CreateDirectory(Parent);
	}
}

