// Cssh 目標 API 展示稿：對應倉庫根目錄的 SyncSkills.sh。
// 只依賴 Cssh 的宣告層，尚無實現，故暫不能執行。
// #r "path/to/Tsinswreng.Cssh.dll"

using static Tsinswreng.Cssh.Sh;

var Root = ScriptDir();
var SkillsRepoDir = Root + "/.Tsinswreng/Skills";
var AgentsSkillsDir = Root + "/.agents/skills";
var GitHubBaseUrl = "https://github.com/Tsinswreng";

// 將一個技能倉庫同步到 .agents/skills。clone 與 pull 的失敗均中止單項模式。
void SyncOne(str SkillName){
	var RepoName = "tsinswreng-" + SkillName;
	var RepoDir = SkillsRepoDir + "/" + RepoName;
	var RepoUrl = GitHubBaseUrl + "/skill-" + SkillName + ".git";

	if (Exists(RepoDir)) {
		Console.WriteLine("[pull] " + RepoName);
		X(new("git", ["-C", RepoDir, "pull"]));
	}
	else {
		Console.WriteLine("[clone] " + RepoName + " <- " + RepoUrl);
		X(new("git", ["clone", RepoUrl, RepoDir]));
	}

	SyncSkillContent(RepoDir, RepoName);
}

// 用倉庫名定位其內層技能目錄，並覆蓋 .agents 中的舊副本。
void SyncSkillContent(str RepoDir, str RepoName){
	var Source = RepoDir + "/" + RepoName;
	var Destination = AgentsSkillsDir + "/" + RepoName;

	if (!Exists(Source)) {
		Console.WriteLine("[warn] skill inner directory not found: " + Source + ", skipping copy");
		return;
	}

	Mkdir(AgentsSkillsDir);
	Console.WriteLine("[sync] " + Source + " -> " + Destination);
	Rm(Destination, Force: true);
	Cp(Source, Destination);
}

Mkdir(SkillsRepoDir);

var ScriptArgs = Args();
if (ScriptArgs.Count >= 1) {
	// 單項模式：dotnet script SyncSkills.csx <skill-short-name>
	SyncOne(ScriptArgs[0]);
}
else {
	Console.WriteLine("[sync-all] pulling and syncing all skills in " + SkillsRepoDir);
	var FoundAny = false;

	foreach (var RepoDir in Dirs(SkillsRepoDir)) {
		var RepoName = RepoDir.Split(['/', '\\']).Last();
		if (!RepoName.StartsWith("tsinswreng-") || !Exists(RepoDir + "/.git"))
			continue;

		Console.WriteLine("[pull] " + RepoName);
		var PullResult = TryX(new("git", ["-C", RepoDir, "pull"]));
		if (!PullResult.IsSuccess)
			Console.WriteLine("[warn] pull failed for " + RepoName + ", continuing...");

		SyncSkillContent(RepoDir, RepoName);
		FoundAny = true;
	}

	if (!FoundAny)
		Console.WriteLine("[warn] no skill repos found in " + SkillsRepoDir);
}

Console.WriteLine("[done]");
