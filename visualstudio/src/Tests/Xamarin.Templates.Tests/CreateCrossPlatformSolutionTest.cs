using System;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace Xamarin.Templates.Tests
{
    public class CreateCrossPlatformSolutionTest
    {

        [VsTheory(Version = "2017", ReuseInstance = false, UIThread = true)]
        [InlineData("Android", "Headless=true|IsIOSSelected=false|IsAndroidSelected=true", 2)]
        [InlineData("IOS", "Headless=true|IsIOSSelected=true|IsAndroidSelected=false", 2)]
        [InlineData("AndroidAndIOS", "Headless=true|IsIOSSelected=true|IsAndroidSelected=true", 3)]
        public void when_unfolding_cross_platform_template_then_projects_are_created(string name, string templateParams, int projectCount)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;

            UnfoldTemplate(name, templateParams);

            Assert.Equal(projectCount, dte.Solution.Projects.Count);
            dte.Solution.Close(true);
        }

        void UnfoldTemplate(string name, string templateParams, string templateId = "Microsoft.CSharp.Xamarin.Forms.App", string lang = "CSharp")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            var sln2 = dte.Solution as Solution2;
            var templatePath = sln2.GetProjectTemplate(templateId, lang);
            var destination = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), name);

            if (!string.IsNullOrEmpty(templateParams))
                templatePath += "|" + templateParams;

            sln2.AddFromTemplate(templatePath, destination, name);
        }
    }
}