// Cssh 目標 API 展示稿：對應倉庫根目錄的 UpdSln.sh。
// 只依賴 Cssh 的宣告層，尚無實現，故暫不能執行。
// #r "path/to/Tsinswreng.Cssh.dll"

using static Tsinswreng.Cssh.Sh;

// 每個目錄都依原 Bash 腳本的順序掃描；Find 保持惰性，不會先將所有專案載入記憶體。
var ProjectRoots = new[]{
	"Ngan.Dict/Ngan.Dict.Core/proj",
	"Ngan.Dict/Ngan.Dict.Backend/proj",
	"Ngan.Dict/Ngan.Dict.Doc/proj",
	"Ngan.Dict/Ngan.Dict.Test/proj",
	"Ngan.Dict/Ngan.Dict.Frontend/proj",
	"Ngan.Dict/Ngan.Dict.Server/proj",
	"Ngan.Ime/Ngan.Ime.Core/proj",
	"Ngan.Ime/Ngan.Ime.Doc/proj",
	"Ngan.Ime/Ngan.Ime.Frontend/proj",
	"Ngan.Ime/Ngan.Ime.Rime/proj",
	"AvlnImeDemo",
	"CsRimeApi/proj",
	"CsRimeLua/proj",
	"CsRimeLua/proj/TestLuaLib/proj",
	"RimeTools/proj",
	"RimeTts/proj",
	"Tsinswreng.CsInterop/proj",
	"Tsinswreng.CsLua/proj",
	"Tsinswreng.CsRingBuffer/proj",
	"Tsinswreng.CsSqlHelper/proj",
	"Tsinswreng.CsUlid/proj",
	"Tsinswreng.CsCore/proj",
	"Tsinswreng.CsCtx/proj",
	"Tsinswreng.CsTools/proj",
	"Tsinswreng.CsU128Id/proj",
	"Tsinswreng.CsI18n/proj",
	"Tsinswreng.CsErr/proj",
	"Tsinswreng.CsJson/proj",
	"Tsinswreng.CsSrcGenTools/proj",
	"Tsinswreng.CsFactoryMkr/proj",
	"Tsinswreng.CsDictMapper/proj",
	"Tsinswreng.Srefl/proj",
	"Tsinswreng.CsIfaceGen/proj",
	"Tsinswreng.CsDecl/proj",
	"Tsinswreng.CsSql/proj",
	"Tsinswreng.CsPage/proj",
	"Tsinswreng.CsLog/proj",
	"Tsinswreng.CsCfg/proj",
	"Tsinswreng.CsTempus/proj",
	"Tsinswreng.AvlnTools/proj",
	"Tsinswreng.Avln.Dsl/proj",
	"Tsinswreng.Avln.Grid/proj",
	"Tsinswreng.Avln.Navi/proj",
	"Tsinswreng.CsYamlMd/proj",
	"Tsinswreng.CsTextWithBlob/proj",
	"Tsinswreng.CsTreeTest/proj",
	"Tsinswreng.CsTreeTest/proj/Samples",
	"Tsinswreng.Cssh/proj",
	"Tsinswreng.Avln.StrokeText/proj",
	"Thesis/proj",
	"Tsinswreng.OpenXmlTools/proj",
};

foreach (var ProjectRoot in ProjectRoots) {
	foreach (var Project in Find("*.csproj", Under: ProjectRoot))
		X($"dotnet sln add {Project}");
}
