using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;
using Xamarin.Templates.Wizards;
using EnvDTE80;
using Microsoft.VisualStudio.Telemetry;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.ComponentModelHost;
using Merq;
using Xamarin.VisualStudio.Contracts.Commands.IOS;
using Xamarin.VisualStudio.Contracts.Model.IOS;

namespace Xamarin.Templates.Wizard
{
    internal class SetiOSMinimumOSVersionWizard : IWizard
    {
        protected Dictionary<string, string> replacements;
        object automationObject;
        private DTE2 dte;

        public void RunFinished()
        {
            CreateTemplate();
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            try
            {
                this.automationObject = automationObject;
                dte = automationObject as DTE2;

                replacements = replacementsDictionary;

            }
            catch (WizardBackoutException)
            {
                throw;
            }
        }

        protected string SafeProjectName
        {
            get { return GetReplacementValue("$safeprojectname$"); }
        }

        protected string GetReplacementValue(string key)
        {
            string value;
            replacements.TryGetValue(key, out value);
            return value;
        }

        protected string SolutionPath
        {
            get { return GetReplacementValue("$destinationdirectory$"); }
        }

        public bool ShouldAddProjectItem(string filePath) => true;

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        private IWizard CreateTemplatingWizard()
        {
            var assembly = Assembly.Load("Microsoft.VisualStudio.TemplateEngine.Wizard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var type = assembly.GetType("Microsoft.VisualStudio.TemplateEngine.Wizard.TemplateEngineWizard", true);
            return (IWizard)Activator.CreateInstance(type);
        }
        protected void CreateTemplate()
        {
            var wizard = CreateTemplatingWizard();
            wizard.RunStarted(automationObject, AddReplacements(), WizardRunKind.AsMultiProject, new object[] { });
            wizard.RunFinished();
        }

        protected Dictionary<string, string> AddReplacements()
        {
            var sdkType = GetSdkType(replacements["$wizarddata$"]);
            var componentModel = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            var commandBus = componentModel?.GetService<ICommandBus>();
            var versions = commandBus?.Execute(new GetSdkInfo());
            var minimumOSVersion = versions?.LatestInstalledSdks[sdkType];

            if (minimumOSVersion == null)
                minimumOSVersion = GetDefaultVersion(sdkType);

            replacements.Add("$uistyle$", "none");
            replacements.Add("$language$", "CSharp");
            replacements.Add("$templateid$", $"Xamarin.{sdkType.ToString()}.App.CSharp");
            replacements.Add("$passthrough:MinimumOSVersion", minimumOSVersion.ToString());

            return replacements;
        }

        private static string GetDefaultVersion(SdkType sdkType)
        {
            switch (sdkType)
            {
                case SdkType.iOS:
                    return "11.3";
                case SdkType.tvOS:
                    return "11.3";
                case SdkType.watchOS:
                    return "4.3";
                default:
                    return string.Empty;
            }
        }

        private static SdkType GetSdkType(string wizardData)
        {
            var stringSDK = string.Empty;

            using (var xmlReader = XmlReader.Create(new StringReader(wizardData), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment }))
            {
                if (xmlReader.ReadToNextSibling("MinimumOSVersion"))
                {
                    var element = XElement.Load(xmlReader.ReadSubtree());
                    var ns = XNamespace.Get("http://schemas.microsoft.com/developer/vstemplate/2005");
                    stringSDK = element.Element(ns + "SdkType").Value;
                }
            }

            if (Enum.TryParse(stringSDK, out SdkType sdkType))
                return sdkType;
            else
                return SdkType.iOS;
        }
    }
}
