using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsSh.Test.Domains.CsSh;

/// Declares tests for copy and move operations.
public partial class TestCssh{
	/// Registers Cp and Mv cases.
	public partial void RegisterCpAndMv(ITestNode Node);

	/// Verifies recursive copying retains both files and empty directories.
	public partial Task<object?> CpPreservesFilesAndEmptyDirectories(object? O);

	/// Verifies moving builds missing destination parents.
	public partial Task<object?> MvCreatesDestinationParent(object? O);

	/// Verifies concise asynchronous Cp and Mv overloads.
	public partial Task<object?> AsyncCpAndMvNeedNoNullOptions(object? O);

	/// Verifies a source glob copies its matches as destination children rather than nesting the source directory.
	public partial Task<object?> CpGlobCopiesSourceContents(object? O);

	/// Verifies a trailing-slash glob copies only directories once.
	public partial Task<object?> CpDirectoryGlobCopiesDirectories(object? O);

	/// Verifies an existing directory destination receives the source file name.
	public partial Task<object?> CpFileIntoExistingDirectory(object? O);

	/// Verifies an existing directory destination receives the source directory name.
	public partial Task<object?> MvDirectoryIntoExistingDirectory(object? O);

	/// Verifies copying a directory into itself is rejected before recursive enumeration starts.
	public partial Task<object?> CpRejectsDestinationInsideSource(object? O);

	/// Verifies moving a directory onto itself is rejected without deleting the source.
	public partial Task<object?> MvRejectsDestinationInsideSource(object? O);

	/// Verifies glob directory merges respect the default non-overwrite policy.
	public partial Task<object?> CpGlobPreservesConflictingFileWithoutOverwrite(object? O);
}

