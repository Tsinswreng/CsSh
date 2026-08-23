using System.Text.RegularExpressions;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Tsinswreng.CsSh.Scripts;

/// Release 的流程實現；所有外部命令均透過 CsSh 執行。
internal static partial class Release{
	/// 執行先驗證後發布的單一流程；任何命令失敗都會直接中止後續步驟。
	internal static async partial Task Main(str Root, CT Ct){
		// step 1: 驗證依賴、Release 編譯與現有測試。
		await Verify(Root, Ct);

		// step 2: 沒有發布 tag 時只完成 CI 驗證，不產生公開版本。
		var Tag = ResolveTag();
		if(Tag is null){
			Echo("Verification succeeded; no release tag was supplied.");
			return;
		}

		// step 3: tag 是唯一版本來源，先解析再建立套件。
		var Version = ParseVersion(Tag);
		var Packages = await Pack(Root, Version, Ct);

		// step 4: NuGet 成功後才建立 GitHub Release，避免發布狀態不一致。
		await PushNuGet(Packages, Ct);
		await PublishGitHubRelease(Tag, Packages, Ct);
	}

	/// 執行還原、Release 建置及測試；命令非零退出時 CsSh 會拋例外並阻止發布。
	private static async partial Task<nil> Verify(str Root, CT Ct){
		var TestProject = Root/"proj/Tsinswreng.CsSh.Test/Tsinswreng.CsSh.Test.csproj";
		await Exe("dotnet", ["restore", TestProject], Ct);
		await Exe("dotnet", ["build", TestProject, "--configuration", "Release", "--no-restore"], Ct);
		await Exe("dotnet", ["run", "--project", TestProject, "--configuration", "Release", "--no-build", "--no-restore"], Ct);
		return NIL;
	}

	/// 清理本次發布目錄並產生 nupkg、snupkg；只使用工作區內的 artifacts 路徑。
	private static async partial Task<IReadOnlyList<Pth>> Pack(str Root, str Version, CT Ct){
		var ArtifactDir = Root/"artifacts/release";
		if(await Exists(ArtifactDir, Ct)){
			await Rm(ArtifactDir, Ct);
		}
		await Mkdir(ArtifactDir, Ct);
		var PackageProject = Root/"proj/Tsinswreng.CsSh/Tsinswreng.CsSh.csproj";
		await Exe("dotnet", ["pack", PackageProject, "--configuration", "Release", "--no-build", "--no-restore", "--output", ArtifactDir, $"-p:PackageVersion={Version}"], Ct);

		var Packages = new List<Pth>();
		foreach(var FilePath in LsFile(ArtifactDir)){
			var Name = FilePath.ToString();
			if(Name.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase) || Name.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase)){
				Packages.Add(FilePath);
			}
		}
		if(Packages.Count == 0){
			throw new InvalidOperationException($"dotnet pack produced no NuGet artifacts in {ArtifactDir}.");
		}
		return Packages;
	}

	/// 依序推送 nupkg 與 snupkg，讓 nuget.org 同時取得主要套件和符號套件。
	private static async partial Task<nil> PushNuGet(IReadOnlyList<Pth> Packages, CT Ct){
		var ApiKey = GetEnv("NUGET_API_KEY");
		if(String.IsNullOrWhiteSpace(ApiKey)){
			throw new InvalidOperationException("NUGET_API_KEY is required for a tagged release.");
		}
		const str NuGetSource = "https://api.nuget.org/v3/index.json";
		foreach(var Package in Packages){
			await Exe("dotnet", ["nuget", "push", Package, "--source", NuGetSource, "--api-key", ApiKey, "--skip-duplicate"], Ct);
		}
		return NIL;
	}

	/// 建立新 Release；若同一 tag 的 NuGet 已成功但 Release 建立中斷，則改為更新既有 Release。
	private static async partial Task<nil> PublishGitHubRelease(str Tag, IReadOnlyList<Pth> Packages, CT Ct){
		var ViewResult = await TryExe("gh", ["release", "view", Tag, "--json", "tagName"], Ct);
		if(ViewResult.IsSuccess){
			var UploadArgs = new List<str>{"release", "upload", Tag, "--clobber"};
			foreach(var Package in Packages){
				UploadArgs.Add(Package);
			}
			await Exe("gh", UploadArgs, Ct);
			return NIL;
		}

		var CreateArgs = new List<str>{"release", "create", Tag, "--verify-tag", "--generate-notes", "--title", Tag};
		foreach(var Package in Packages){
			CreateArgs.Add(Package);
		}
		await Exe("gh", CreateArgs, Ct);
		return NIL;
	}

	/// 讀取 GitHub runner 提供的 GITHUB_REF_NAME；非 tag 事件不會發布。
	private static partial str? ResolveTag(){
		var EnvironmentTag = GetEnv("GITHUB_REF_NAME");
		if(String.Equals(GetEnv("GITHUB_REF_TYPE"), "tag", StringComparison.OrdinalIgnoreCase) && !String.IsNullOrWhiteSpace(EnvironmentTag)){
			return EnvironmentTag;
		}
		return null;
	}

	/// 僅接受 vMAJOR.MINOR.PATCH 加可選 prerelease/build 文字的發布 tag。
	private static partial str ParseVersion(str Tag){
		if(!Regex.IsMatch(Tag, "^v[0-9]+\\.[0-9]+\\.[0-9]+(-[0-9A-Za-z][0-9A-Za-z.-]*)?$", RegexOptions.CultureInvariant)){
			throw new ArgumentException("Release tag must match vMAJOR.MINOR.PATCH with an optional prerelease suffix.", nameof(Tag));
		}
		return Tag[1..];
	}
}
