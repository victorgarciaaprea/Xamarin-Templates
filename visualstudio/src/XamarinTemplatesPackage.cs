using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace Xamarin.Templates
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("Xamarin Templates", "Templates for building iOS, Android, and Windows apps with Xamarin and Xamarin.Forms.",
        ThisAssembly.Git.SemVer.Major + "." +
        ThisAssembly.Git.SemVer.Minor + "." +
        ThisAssembly.Git.SemVer.Patch + ThisAssembly.Git.SemVer.DashLabel + " (" +
        ThisAssembly.Git.Commit + ")")]
    [Guid(PackageGuidString)]
    public class XamarinTemplatesPackage : Package
    {
        public const string PackageGuidString = "4661F042-BFB2-47D0-A4AD-7E1D3002B8F1";

        protected override void Initialize() => base.Initialize();
    }
}
