using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Xamarin.Templates")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Xamarin.Templates")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

partial class ThisAssembly
{
    /// <summary>
    /// Simple release-like version number, like 4.0.1 for a cycle 5, SR1 build.
    /// </summary>
    public const string SimpleVersion = Git.BaseVersion.Major + "." + Git.BaseVersion.Minor + "." + Git.BaseVersion.Patch;

    /// <summary>
    /// Full version, including commits since base version file, like 4.0.1.598
    /// </summary>
    public const string Version = SimpleVersion + "." + Git.Commits;

    /// <summary>
    /// Full version, plus branch and commit short sha.
    /// </summary>
    public const string InformationalVersion = Version + "-" + Git.Branch + "+" + Git.Commit;
}
