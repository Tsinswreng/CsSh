using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tsinswreng.CsSh;

public static partial class Sh{
	public static partial bool Exists(str Path) {
		return Exists(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static partial Task<bool> Exists(str Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		var FileSystemPath = NormalizeFileSystemPath(Path);
		return Task.FromResult(File.Exists(FileSystemPath) || Directory.Exists(FileSystemPath));
	}

	public static partial void Mkdir(str Path) {
		Mkdir(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static partial Task<nil> Mkdir(str Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		Directory.CreateDirectory(NormalizeFileSystemPath(Path));
		return Task.FromResult(NIL);
	}

	public static partial void Rm(str Path) {
		Rm(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static partial Task<nil> Rm(str Path, CT Ct) {
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

	public static partial void Cp(str Source, str Destination, CpOptions? Options) {
		Cp(Source, Destination, Options, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static partial Task<nil> Cp(str Source, str Destination, CT Ct) {
		return Cp(Source, Destination, null, Ct);
	}

	public static partial async Task<nil> Cp(str Source, str Destination, CpOptions? Options, CT Ct) {
		var Overwrite = Options?.Overwrite ?? false;
		var SourcePath = NormalizeFileSystemPath(Source);
		var DestinationPath = NormalizeFileSystemPath(Destination);
		if (File.Exists(SourcePath)) {
			await CopyFile(SourcePath, DestinationPath, Overwrite, Ct).ConfigureAwait(false);
		}
		else if (Directory.Exists(SourcePath)) {
			await CopyDirectory(SourcePath, DestinationPath, Overwrite, Ct).ConfigureAwait(false);
		}
		else {
			throw new FileNotFoundException("Source path does not exist.", SourcePath);
		}
		return NIL;
	}

	public static partial void Mv(str Source, str Destination, MvOptions? Options) {
		Mv(Source, Destination, Options, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static partial Task<nil> Mv(str Source, str Destination, CT Ct) {
		return Mv(Source, Destination, null, Ct);
	}

	public static partial async Task<nil> Mv(str Source, str Destination, MvOptions? Options, CT Ct) {
		var Overwrite = Options?.Overwrite ?? false;
		var SourcePath = NormalizeFileSystemPath(Source);
		var DestinationPath = NormalizeFileSystemPath(Destination);
		if (!File.Exists(SourcePath) && !Directory.Exists(SourcePath)) {
			throw new FileNotFoundException("Source path does not exist.", SourcePath);
		}
		EnsureParentDirectory(DestinationPath);
		if (File.Exists(SourcePath)) {
			File.Move(SourcePath, DestinationPath, Overwrite);
		}
		else {
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

	public static partial IEnumerable<FileSystemEntry> Find(str Pattern) {
		var Regex = GlobToRegex(NormalizePath(Pattern));
		var Root = GlobRoot(Pattern);
		if (!Directory.Exists(Root))
			return [];
		return Directory.EnumerateFileSystemEntries(Root, "*", SearchOption.AllDirectories)
			.Select(MakeEntry)
			.Where(Entry => Regex.IsMatch(Entry.Path));
	}

	public static async partial IAsyncEnumerable<FileSystemEntry> Find(str Pattern, [EnumeratorCancellation] CT Ct) {
		var Regex = GlobToRegex(NormalizePath(Pattern));
		var Root = GlobRoot(Pattern);
		if (!Directory.Exists(Root))
			yield break;
		foreach (var Path in Directory.EnumerateFileSystemEntries(Root, "*", SearchOption.AllDirectories)) {
			Ct.ThrowIfCancellationRequested();
			var Entry = MakeEntry(Path);
			if (Regex.IsMatch(NormalizePath(Path)))
				yield return Entry;
			await Task.Yield();
		}
	}

	public static partial IEnumerable<FileSystemEntry> Ls(str? Path, LsOptions? Options) {
		var Root = NormalizeFileSystemPath(Path ?? ".");
		var Option = Options?.Recursive == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		return Directory.EnumerateFileSystemEntries(Root, "*", Option).Select(MakeEntry);
	}

	public static partial IAsyncEnumerable<FileSystemEntry> Ls(str? Path, CT Ct) {
		return Ls(Path, null, Ct);
	}

	public static async partial IAsyncEnumerable<FileSystemEntry> Ls(str? Path, LsOptions? Options, [EnumeratorCancellation] CT Ct) {
		var Root = NormalizeFileSystemPath(Path ?? ".");
		var Option = Options?.Recursive == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		foreach (var EntryPath in Directory.EnumerateFileSystemEntries(Root, "*", Option)) {
			Ct.ThrowIfCancellationRequested();
			yield return MakeEntry(EntryPath);
			await Task.Yield();
		}
	}

	public static partial Content Read(str Path) {
		return Read(Path, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static partial Task<Content> Read(str Path, CT Ct) {
		Ct.ThrowIfCancellationRequested();
		Stream Result = new FileStream(NormalizeFileSystemPath(Path), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
		return Task.FromResult<Content>(new(Result, new(LeaveOpen: false)));
	}

	public static partial void Write(str Path, Content Source) {
		Write(Path, Source, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static partial async Task<nil> Write(str Path, Content Source, CT Ct) {
		ArgumentNullException.ThrowIfNull(Source);
		var FileSystemPath = NormalizeFileSystemPath(Path);
		EnsureParentDirectory(FileSystemPath);
		await using var Target = new FileStream(FileSystemPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await Source.Stream.CopyToAsync(Target, Ct).ConfigureAwait(false);
		return NIL;
	}

	public static partial void Append(str Path, Content Source) {
		Append(Path, Source, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static partial async Task<nil> Append(str Path, Content Source, CT Ct) {
		ArgumentNullException.ThrowIfNull(Source);
		var FileSystemPath = NormalizeFileSystemPath(Path);
		EnsureParentDirectory(FileSystemPath);
		await using var Target = new FileStream(FileSystemPath, FileMode.Append, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await Source.Stream.CopyToAsync(Target, Ct).ConfigureAwait(false);
		return NIL;
	}

	private static async Task CopyFile(str Source, str Destination, bool Overwrite, CT Ct) {
		EnsureParentDirectory(Destination);
		if (File.Exists(Destination) && !Overwrite)
			throw new IOException("Destination file already exists.");
		await using var Input = new FileStream(Source, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
		await using var Output = new FileStream(Destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await Input.CopyToAsync(Output, Ct).ConfigureAwait(false);
	}

	private static async Task CopyDirectory(str Source, str Destination, bool Overwrite, CT Ct) {
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

	private static FileSystemEntry MakeEntry(str Path) {
		var Attributes = File.GetAttributes(Path);
		var IsDir = Attributes.HasFlag(FileAttributes.Directory);
		var IsLink = Attributes.HasFlag(FileAttributes.ReparsePoint);
		return new(NormalizePath(Path), System.IO.Path.GetFileName(Path), !IsDir, IsDir, IsLink);
	}

	private static str GlobRoot(str Pattern) {
		var NormalizedPattern = NormalizePath(Pattern);
		var MagicIndex = NormalizedPattern.IndexOfAny(['*', '?']);
		if (MagicIndex < 0) {
			var Parent = System.IO.Path.GetDirectoryName(NormalizeFileSystemPath(NormalizedPattern));
			return string.IsNullOrEmpty(Parent) ? Environment.CurrentDirectory : Parent;
		}
		var Prefix = NormalizedPattern[..MagicIndex];
		var Separator = Prefix.LastIndexOf('/');
		var Root = Separator < 0 ? "." : Prefix[..Separator];
		return NormalizeFileSystemPath(string.IsNullOrEmpty(Root) ? "." : Root);
	}

	private static Regex GlobToRegex(str Pattern) {
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

	private static void EnsureParentDirectory(str FileSystemPath) {
		var Parent = System.IO.Path.GetDirectoryName(FileSystemPath);
		if (!string.IsNullOrEmpty(Parent))
			Directory.CreateDirectory(Parent);
	}
}

