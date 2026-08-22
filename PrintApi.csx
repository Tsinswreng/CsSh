#r "nuget: Tsinswreng.CsSh, 0.1.0-alpha"
#nullable enable
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;
using CT = System.Threading.CancellationToken;
CT Ct = default;

var Root = CsxDir();
var CsProjDir = Root/"proj"/"Tsinswreng.CsSh";

var TargetFile = Root/"Api.txt";
await Write(TargetFile, "", Ct);
foreach(var Decl in Glob(CsProjDir/"*.cs")){
	// Glob now returns Pth directly, so no FileInfo projection is needed before reading.
	if (((string)Decl).EndsWith(".Impl.cs", StringComparison.Ordinal)) {
		continue;
	}
	var Content = await Read(Decl, Ct);
	var ToWrite = await Content.Text(Ct);
	{
		var Rewritten = ToWrite;
		Rewritten = Rewritten.Replace("public static partial ", "static ");
		Rewritten = Rewritten.Replace("public partial ", "");
		ToWrite = Rewritten;
	}
	await Append(TargetFile, ToWrite, Ct);
}
