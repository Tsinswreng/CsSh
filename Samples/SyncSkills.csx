// Cssh 版技能同步腳本，對應倉庫根目錄的 SyncSkills.sh。
// #r "path/to/Tsinswreng.CsSh.dll"

using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

var Root = ScriptDir();
var SkillsRepoDir = Root / ".Tsinswreng/Skills";
var AgentsSkillsDir = Root / ".agents/skills";
var GitHubBaseUrl = "https://github.com/Tsinswreng";

// 將一個技能倉庫同步到 .agents/skills。clone 與 pull 的失敗均中止單項模式。
void XTerm(string Text){
	using var Command = X(Text);
	Task.WhenAll(
		Task.Run(() => Write(Stdout, Command.Result.Stdout)),
		Task.Run(() => Write(Stderr, Command.Result.Stderr)),
		Command.Done).GetAwaiter().GetResult();
}

bool TryXTerm(string Text){
	using var Command = TryX(Text);
	Task.WhenAll(
		Task.Run(() => Write(Stdout, Command.Result.Stdout)),
		Task.Run(() => Write(Stderr, Command.Result.Stderr)),
		Command.Done).GetAwaiter().GetResult();
	return Command.Done.GetAwaiter().GetResult().IsSuccess;
}

void SyncOne(string SkillName){
	var RepoName = "tsinswreng-" + SkillName;
	var RepoDir = SkillsRepoDir / RepoName;
	var RepoUrl = GitHubBaseUrl + "/skill-" + SkillName + ".git";

	if (Exists(RepoDir)) {
		Echo("[pull] " + RepoName);
		XTerm($"git -C \"{RepoDir}\" pull");
	}
	else {
		Echo("[clone] " + RepoName + " <- " + RepoUrl);
		XTerm($"git clone \"{RepoUrl}\" \"{RepoDir}\"");
	}

	SyncSkillContent(RepoDir, RepoName);
}

// 用倉庫名定位其內層技能目錄，並覆蓋 .agents 中的舊副本。
void SyncSkillContent(string RepoDir, string RepoName){
	var Source = RepoDir / RepoName;
	var Destination = AgentsSkillsDir / RepoName;

	if (!Exists(Source)) {
		Echo("[warn] skill inner directory not found: " + Source + ", skipping copy");
		return;
	}

	Mkdir(AgentsSkillsDir);
	Echo("[sync] " + Source + " -> " + Destination);
	Rm(Destination);
	Cp(Source, Destination);
}

Mkdir(SkillsRepoDir);

var ScriptArgs = Args();
if (ScriptArgs.Count >= 1) {
	// 單項模式：dotnet script SyncSkills.csx <skill-short-name>
	SyncOne(ScriptArgs[0]);
}
else {
	Echo("[sync-all] pulling and syncing all skills in " + SkillsRepoDir);
	var FoundAny = false;

	foreach (var Repo in Ls(SkillsRepoDir)) {
		if (!Repo.IsDir || !Repo.Name.StartsWith("tsinswreng-") || !Exists(Repo.Path / ".git"))
			continue;

		Echo("[pull] " + Repo.Name);
		var PullSucceeded = TryXTerm($"git -C \"{Repo.Path}\" pull");
		if (!PullSucceeded)
			Echo("[warn] pull failed for " + Repo.Name + ", continuing...");

		SyncSkillContent(Repo.Path, Repo.Name);
		FoundAny = true;
	}

	if (!FoundAny)
		Echo("[warn] no skill repos found in " + SkillsRepoDir);
}

Echo("[done]");

