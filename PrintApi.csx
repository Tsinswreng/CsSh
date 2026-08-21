#r "nuget: Tsinswreng.CsSh, 0.1.0-alpha"
#nullable enable
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;
using CT = System.Threading.CancellationToken;
CT Ct = default;

var Root = CsxDir();
var CsProjDir = Root/"proj"/"Tsinswreng.CsSh";
var SrcFiles = Glob(CsProjDir/"*", Ct);
//FileSystemInfo
var Decls = SrcFiles
.Where(
	x=>x is FileInfo f
	&& f.FullName.EndsWith("cs")
	&& !f.FullName.EndsWith(".Impl.cs")
);

var TargetFile = Root/"Api.txt";
await Write(TargetFile, "", Ct);
await foreach(var decl in Decls){
	var content = await Read(decl.FullName, Ct);
	var toWrite = await content.Text(Ct);
	{
		var a = toWrite;
		a = a.Replace("public static partial ", "static ");
		a = a.Replace("public partial ", "");
		toWrite = a;
	}
	await Append(TargetFile, toWrite, Ct);
}

