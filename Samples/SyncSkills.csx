// Cssh 目標 API 展示稿：對應倉庫根目錄的 SyncSkills.sh。
// 只依賴 Cssh 的宣告層，尚無實現，故暫不能執行。
// #r "path/to/Tsinswreng.Cssh.dll"

using static Tsinswreng.Cssh.Sh;

using var CtSource = new CancellationTokenSource();
var Ct = CtSource.Token;
var Root = ScriptDir();
var SkillsRepoDir = Root + "/.Tsinswreng/Skills";
var AgentsSkillsDir = Root + "/.agents/skills";
var GitHubBaseUrl = "https://github.com/Tsinswreng";

// 將一個技能倉庫同步到 .agents/skills。clone 與 pull 的失敗均中止單項模式。
async Task XTerm(string Text){
	await using var Command = X(Text, Ct: Ct);
	await Task.WhenAll(
		Write(Stdout, Command.Result.Stdout, Ct),
		Write(Stderr, Command.Result.Stderr, Ct),
		Command.Done);
}

async Task<bool> TryXTerm(string Text){
	await using var Command = TryX(Text, Ct: Ct);
	await Task.WhenAll(
		Write(Stdout, Command.Result.Stdout, Ct),
		Write(Stderr, Command.Result.Stderr, Ct));
	return (await Command.Done).IsSuccess;
}

async Task SyncOne(string SkillName){
	var RepoName = "tsinswreng-" + SkillName;
	var RepoDir = SkillsRepoDir + "/" + RepoName;
	var RepoUrl = GitHubBaseUrl + "/skill-" + SkillName + ".git";

	if (await Exists(RepoDir, Ct)) {
		await Echo("[pull] " + RepoName, Ct);
		await XTerm($"git -C \"{RepoDir}\" pull");
	}
	else {
		await Echo("[clone] " + RepoName + " <- " + RepoUrl, Ct);
		await XTerm($"git clone \"{RepoUrl}\" \"{RepoDir}\"");
	}

	await SyncSkillContent(RepoDir, RepoName);
}

// 用倉庫名定位其內層技能目錄，並覆蓋 .agents 中的舊副本。
async Task SyncSkillContent(string RepoDir, string RepoName){
	var Source = RepoDir + "/" + RepoName;
	var Destination = AgentsSkillsDir + "/" + RepoName;

	if (!await Exists(Source, Ct)) {
		await Echo("[warn] skill inner directory not found: " + Source + ", skipping copy", Ct);
		return;
	}

	await Mkdir(AgentsSkillsDir, Ct);
	await Echo("[sync] " + Source + " -> " + Destination, Ct);
	await Rm(Destination, Force: true, Ct);
	await Cp(Source, Destination, Ct: Ct);
}

await Mkdir(SkillsRepoDir, Ct);

var ScriptArgs = Args();
if (ScriptArgs.Count >= 1) {
	// 單項模式：dotnet script SyncSkills.csx <skill-short-name>
	await SyncOne(ScriptArgs[0]);
}
else {
	await Echo("[sync-all] pulling and syncing all skills in " + SkillsRepoDir, Ct);
	var FoundAny = false;

	await foreach (var Repo in Ls(SkillsRepoDir, Ct: Ct)) {
		if (!Repo.IsDir || !Repo.Name.StartsWith("tsinswreng-") || !await Exists(Repo.Path + "/.git", Ct))
			continue;

		await Echo("[pull] " + Repo.Name, Ct);
		var PullSucceeded = await TryXTerm($"git -C \"{Repo.Path}\" pull");
		if (!PullSucceeded)
			await Echo("[warn] pull failed for " + Repo.Name + ", continuing...", Ct);

		await SyncSkillContent(Repo.Path, Repo.Name);
		FoundAny = true;
	}

	if (!FoundAny)
		await Echo("[warn] no skill repos found in " + SkillsRepoDir, Ct);
}

await Echo("[done]", Ct);
