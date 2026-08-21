using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

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

	public partial async Task<nil> Cp(Pth Source, Pth Destination, CpOptions? Options, CT Ct) {
		var Overwrite = Options?.Overwrite ?? false;
		if (HasGlob(Source)) {
			await CopyMatches(Source, Destination, Overwrite, Ct).ConfigureAwait(false);
			return NIL;
		}
		var SourcePath = (str)FullPath(Source);
		var DestinationPath = (str)FullPath(Destination);
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

	public partial void Mv(Pth Source, Pth Destination, MvOptions? Options) {
		Mv(Source, Destination, Options, CancellationToken.None).GetAwaiter().GetResult();
	}

	public partial Task<nil> Mv(Pth Source, Pth Destination, CT Ct) {
		return Mv(Source, Destination, null, Ct);
	}

	public partial async Task<nil> Mv(Pth Source, Pth Destination, MvOptions? Options, CT Ct) {
		var Overwrite = Options?.Overwrite ?? false;
		var SourcePath = (str)FullPath(Source);
		var DestinationPath = (str)FullPath(Destination);
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

	public partial IEnumerable<FileSystemInfo> Glob(Pth Pattern) {
		var Regex = GlobToRegex(FullPath(Pattern));
		var Root = GlobRoot(Pattern);
		if (!Directory.Exists(Root))
			return [];
		return new DirectoryInfo(Root).EnumerateFileSystemInfos("*", SearchOption.AllDirectories)
			.Where(Entry => Regex.IsMatch(ToShellPath(Entry.FullName)));
	}

	public async partial IAsyncEnumerable<FileSystemInfo> Glob(Pth Pattern, [EnumeratorCancellation] CT Ct) {
		var Regex = GlobToRegex(FullPath(Pattern));
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

	public partial IEnumerable<FileSystemInfo> Ls(Pth? Path, LsOptions? Options) {
		var Root = (str)FullPath(Path ?? new("."));
		var Option = Options?.Recursive == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		return new DirectoryInfo(Root).EnumerateFileSystemInfos("*", Option);
	}

	public partial IAsyncEnumerable<FileSystemInfo> Ls(Pth? Path, CT Ct) {
		return Ls(Path, null, Ct);
	}

	public async partial IAsyncEnumerable<FileSystemInfo> Ls(Pth? Path, LsOptions? Options, [EnumeratorCancellation] CT Ct) {
		var Root = (str)FullPath(Path ?? new("."));
		var Option = Options?.Recursive == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		foreach (var Entry in new DirectoryInfo(Root).EnumerateFileSystemInfos("*", Option)) {
			Ct.ThrowIfCancellationRequested();
			yield return Entry;
			await Task.Yield();
		}
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
			var Relative = System.IO.Path.GetRelativePath(Source, SourceDirectory);
			Directory.CreateDirectory(System.IO.Path.Combine(Destination, Relative));
		}
		foreach (var SourceFile in Directory.EnumerateFiles(Source, "*", SearchOption.AllDirectories)) {
			var Relative = System.IO.Path.GetRelativePath(Source, SourceFile);
			await CopyFile(SourceFile, System.IO.Path.Combine(Destination, Relative), false, Ct).ConfigureAwait(false);
		}
	}

	/// Copies every glob match as a direct child of Destination, preserving Bash's source/* shape.
	private async Task CopyMatches(str Source, str Destination, bool Overwrite, CT Ct) {
		var DestinationPath = (str)FullPath(Destination);
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
			await CopyFile(Entry.FullName, FullPath(Target), Overwrite, Ct).ConfigureAwait(false);
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
			var Relative = System.IO.Path.GetRelativePath(Source, SourceDirectory);
			Directory.CreateDirectory(System.IO.Path.Combine(Destination, Relative));
		}
		foreach (var SourceFile in Directory.EnumerateFiles(Source, "*", SearchOption.AllDirectories)) {
			var Relative = System.IO.Path.GetRelativePath(Source, SourceFile);
			await CopyFile(SourceFile, System.IO.Path.Combine(Destination, Relative), true, Ct).ConfigureAwait(false);
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
			var Parent = System.IO.Path.GetDirectoryName(FullPath(NormalizedPattern));
			return string.IsNullOrEmpty(Parent) ? FullPath(".") : Parent;
		}
		var Prefix = NormalizedPattern[..MagicIndex];
		var Separator = Prefix.LastIndexOf('/');
		var Root = Separator < 0 ? "." : Prefix[..Separator];
		return FullPath(string.IsNullOrEmpty(Root) ? "." : Root);
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
