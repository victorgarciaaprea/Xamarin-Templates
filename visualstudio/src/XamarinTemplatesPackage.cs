using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace Xamarin.Templates
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(PackageGuidString)]
    public class XamarinTemplatesPackage : Package
    {
        public const string PackageGuidString = "4661F042-BFB2-47D0-A4AD-7E1D3002B8F1";

        protected override void Initialize() => base.Initialize();
    }
}
