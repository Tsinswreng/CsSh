// CsSh 版技能同步腳本，對應倉庫根目錄的 SyncSkills.sh。
// #r "path/to/Tsinswreng.CsSh.dll"

using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

var Root = System.IO.Path.GetFullPath(CsxDir() / "../..").Replace('\\', '/');
var SkillsRepoDir = Root / ".Tsinswreng/Skills";
var AgentsSkillsDir = Root / ".agents/skills";
var GitHubBaseUrl = "https://github.com/Tsinswreng";
using var CtSource = new CancellationTokenSource();
var Ct = CtSource.Token;

// 將一個技能倉庫同步到 .agents/skills。clone 與 pull 的失敗均中止單項模式。
async Task SyncOne(string SkillName){
	var RepoName = "tsinswreng-" + SkillName;
	var RepoDir = SkillsRepoDir / RepoName;
	var RepoUrl = GitHubBaseUrl + "/skill-" + SkillName + ".git";

	if (await Exists(RepoDir, Ct)) {
		await Echo("[pull] " + RepoName, Ct);
		await Exe("git", ["-C", RepoDir, "pull"], Ct);
	}
	else {
		await Echo("[clone] " + RepoName + " <- " + RepoUrl, Ct);
		await Exe("git", ["clone", RepoUrl, RepoDir], Ct);
	}

	await SyncSkillContent(RepoDir, RepoName);
}

// 用倉庫名定位其內層技能目錄，並覆蓋 .agents 中的舊副本。
async Task SyncSkillContent(string RepoDir, string RepoName){
	var Source = RepoDir / RepoName;
	var Destination = AgentsSkillsDir / RepoName;

	if (!await Exists(Source, Ct)) {
		await Echo("[warn] skill inner directory not found: " + Source + ", skipping copy", Ct);
		return;
	}

	await Mkdir(AgentsSkillsDir, Ct);
	await Echo("[sync] " + Source + " -> " + Destination, Ct);
	await Rm(Destination, Ct);
	await Cp(Source, Destination, Ct);
}

await Mkdir(SkillsRepoDir, Ct);

var ScriptArgs = Args;
if (ScriptArgs.Count >= 1) {
	// 單項模式：dotnet script SyncSkills.csx <skill-short-name>
	await SyncOne(ScriptArgs[0]);
}
else {
	await Echo("[sync-all] pulling and syncing all skills in " + SkillsRepoDir, Ct);
	var FoundAny = false;

	await foreach (var Repo in Ls(SkillsRepoDir, Ct)) {
		if (Repo is not DirectoryInfo || !Repo.Name.StartsWith("tsinswreng-") || !await Exists(Repo.FullName / ".git", Ct))
			continue;

		await Echo("[pull] " + Repo.Name, Ct);
		var PullSucceeded = (await TryExe("git", ["-C", Repo.FullName, "pull"], Ct)).IsSuccess;
		if (!PullSucceeded)
			await Echo("[warn] pull failed for " + Repo.Name + ", continuing...", Ct);

		await SyncSkillContent(Repo.FullName, Repo.Name);
		FoundAny = true;
	}

	if (!FoundAny)
		await Echo("[warn] no skill repos found in " + SkillsRepoDir, Ct);
}

await Echo("[done]", Ct);

