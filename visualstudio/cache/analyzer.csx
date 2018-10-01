#r "System.Xml.dll"
#r "System.Xml.Linq.dll"
#load "obj\analyzer\imports.csx"

using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

string[] excludedPackages = { "Microsoft" };

// Set current dir to the directory of the script file.
var scriptFile = Environment.GetCommandLineArgs().First(x => x.EndsWith(".csx"));
if (!Path.IsPathRooted(scriptFile))
    scriptFile = Path.Combine(Directory.GetCurrentDirectory(), scriptFile);

Directory.SetCurrentDirectory(Path.GetDirectoryName(scriptFile));

var templatesDir = Environment.GetCommandLineArgs().Last();
// We may have not received a templates dir at all, default to one.
if (templatesDir.EndsWith(".csx"))
    templatesDir = @"..\..\multiplatform";

// We must lookup references by matching strings rather than XML because some are in commented 
// nodes for the conditional template expansion mechanism.
var packageRefExpr = new Regex("PackageReference Include=\"(?<id>.+)\" Version=\"(?<version>.+)\"", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
var targetFramework = new Regex(@"(?<=TargetFramework\>).+?(?=\<)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
var frameworkVersion = new Regex(@"(?<=TargetFrameworkVersion\>v).+?(?=\<)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

var replacements = Directory.EnumerateFiles(templatesDir, "template.json", SearchOption.AllDirectories)
    .Select(x => JObject.Parse(File.ReadAllText(x)))
    .SelectMany(x => x.SelectTokens("$.symbols.*").Cast<dynamic>())
    .Select(x => new { Replaces = (string)x.replaces, DefaultValue = (string)x.defaultValue })
    .Where(x => x.Replaces != null && x.DefaultValue != null)
    .GroupBy(x => x.Replaces)
    .ToDictionary(x => x.Key, x => x.First().DefaultValue);

var packages = from file in Directory.EnumerateFiles(templatesDir, "*.csproj", SearchOption.AllDirectories)
               let lines = File.ReadAllLines(file)
               let iOS = lines.Any(l => l.Contains("Xamarin.iOS"))
               let android = lines.Any(l => l.Contains("Mono.Android"))
               let uwp = lines.Any(l => l.Contains("UAP"))
               let mac = lines.Any(l => l.Contains("Xamarin.Mac"))
               let tfv = lines.Select(l => frameworkVersion.Match(l)).FirstOrDefault(m => m.Success)?.Value.Replace(".", "")
               let tf = lines.Select(l => targetFramework.Match(l)).FirstOrDefault(m => m.Success)
               let framework = iOS ? "xamarinios10" : 
                    android ? $"monoandroid{replacements["AndroidSDKVersion"].Substring(1).Replace(".", "")}" :
                    uwp ? "uap10.0" :
                    mac ? $"xamarinmac{tfv}" :
                    tf?.Success == true ? tf.Value : $"net{tfv}"
               from line in lines
               let match = packageRefExpr.Match(line)
               where match.Success
               let reference = Tuple.Create(
                   framework,
                   match.Groups["id"].Value,
                   (replacements.ContainsKey(match.Groups["version"].Value) ? replacements[match.Groups["version"].Value] : match.Groups["version"].Value))
               orderby framework
               select reference;
			   
packages = packages.Distinct();

var dupes = packages.GroupBy(p => p.Item1 + '-' + p.Item2).Where(g => g.Count() > 1).ToList();
if (dupes.Count != 0)
{
	Console.Error.WriteLine("Found multiple versions of the same packages in use: ");
	foreach (var dupe in dupes)
	{
		Console.Error.WriteLine("\t{0}: {1}", dupe.First().Item2, string.Join(", ", dupe.Select(d => d.Item3)));
	}
}

new XDocument(
	new XElement("Project", 
		new XElement("PropertyGroup", 
			packages.Any() ? 
				new XElement("TargetFrameworks", string.Join(";", packages.GroupBy(x => x.Item1).Select(x => x.Key))) : 
				new XElement("TargetFramework", "netstandard2.0")
		),
		new XElement("ItemGroup",
			packages.Select(p => new XElement("PackageReference",
				new XAttribute("Condition", $"'$(TargetFramework)' == '{p.Item1}'"),
				new XAttribute("Include", p.Item2), 
				new XAttribute("Version", p.Item3))
			)
		)
	)
).Save(Path.Combine(Path.GetDirectoryName(scriptFile), "packages.targets"));