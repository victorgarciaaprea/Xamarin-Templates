using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using System.IO;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows;
using System.Windows.Interop;
using Microsoft.VisualStudio;
using System.Reflection;
using Merq;

using AndroidModel = Xamarin.VisualStudio.Contracts.Model.Android;
using AndroidCommands = Xamarin.VisualStudio.Contracts.Commands.Android;
using IOSModel = Xamarin.VisualStudio.Contracts.Model.IOS;
using IOSCommands = Xamarin.VisualStudio.Contracts.Commands.IOS;
using Xamarin.VisualStudio.Contracts.Model.Android;
using Microsoft.VisualStudio.Telemetry;
using Xamarin.VisualStudio.Contracts.Model.IOS;
using System.ComponentModel;

namespace Xamarin.Templates.Wizards
{
    public class CrossPlatformTemplateWizard : IWizard
    {
        enum TemplateLanguage { CSharp, FSharp };

        const string NugetPackage = "5fcc8577-4feb-4d04-ad72-d6c629b083cc";
        const string AndroidPackage = "296e6a4e-2bd5-44b7-a96d-8ee3d9cda2f6";
        const string IOSPackage = "77875fa9-01e7-4fea-8e77-dfe942355ca1";
        const string ShellPackage = "2d510815-1c4e-4210-bd82-3d9d2c56c140";

        const int CurrentAndroidLevel = 27;
        const int FallbackAndroidLevel = 26;
        const string FallbackSupportLibVersion = "26.1.0.1";
        AndroidFramework AndroidTargetFramework;

        DTE2 dte;
        ServiceProvider serviceProvider;
        Dictionary<string, string> replacements;
        XPlatViewModel model;
        object automationObject;

        internal static Version MinWindowsVersion = new Version(10, 0, 16267, 0);

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            try
            {
                replacements = replacementsDictionary;
                dte = automationObject as DTE2;
                this.automationObject = automationObject;
                serviceProvider = new ServiceProvider(automationObject as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

                TryLoadPackage(serviceProvider, NugetPackage);
                TryLoadPackage(serviceProvider, ShellPackage);

                InitializeTemplateEngine();

                if (ShowDialog())
                { 
                    var dialog = CreateCrossPlatformDialog();
                    dialog.Title = String.Format("{0} - {1}", dialog.Title, SafeProjectName);

                    var dialogResult = dialog.ShowDialog().GetValueOrDefault();

                    TelemetryService.DefaultSession.PostEvent(new OpenWizardTelemetryEvent(GetType().Name));

                    if (!dialogResult)
                    {
                        throw new WizardBackoutException();
                    }
                    model = ((XPlatViewModel)dialog.DataContext);

                    if (model.IsAndroidSelected)
                        TryLoadPackage(serviceProvider, AndroidPackage);
                    if (model.IsIOSSelected)
                        TryLoadPackage(serviceProvider, IOSPackage);
                }
                else
                {
                    model = FillModel(replacements);
                }
            }
            catch (WizardBackoutException)
            {
                throw;
            }
            catch
            {
                TelemetryService.DefaultSession.PostEvent(new OpenWizardTelemetryEvent(GetType().Name, true));

                throw;
            }

        }

        private XPlatViewModel FillModel(Dictionary<string, string> replacements)
        {
            var model = new XPlatViewModel();
            model.IsAzureSelected = GetValue(replacements, "IsAzureSelected", false);
            model.IsSharedSelected = false;
            model.IsAndroidSelected = GetValue(replacements, "IsAndroidSelected", false);
            model.IsIOSSelected = GetValue(replacements, "IsIOSSelected", false);

            if (replacements.ContainsKey("kind"))
            {
                model.SelectedTemplate = model.Templates.FirstOrDefault(t => t.Id == replacements["kind"]);
            }

            return model;
        }

        private T GetValue<T>(Dictionary<string, string> replacements, string key, T def)
        {
            if (replacements.Any(r => r.Key == key))
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFrom(replacements.First(r => r.Key == key).Value);
            };

            return def;
        }

        private bool ShowDialog()
        {
            var headless = replacements.FirstOrDefault(r => r.Key == "Headless").Value;
            if (headless != null && bool.TryParse(headless, out var headlessbool) && headlessbool)
                return false;

            return true;
        }

        private void InitializeTemplateEngine()
        {
            try
            {
                var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));

                var initializer = componentModel.DefaultExportProvider.GetExport<object>("Microsoft.VisualStudio.TemplateEngine.Contracts.IEngineInitializer").Value;

                initializer.GetType().GetMethod("EnsureInitialized").Invoke(initializer, null);
            }
            catch //initialization may fail if the initializer doesn't exist... we don't really care in that case
            { }
        }

        void TryLoadPackage(IServiceProvider serviceProvider, string packageGuid)
        {
            try
            {
                var packageId = new Guid(packageGuid);
                var vsShell = (IVsShell)serviceProvider.GetService(typeof(SVsShell));
                var vsPackage = default(IVsPackage);

                vsShell.IsPackageLoaded(ref packageId, out vsPackage);

                if (vsPackage == null)
                    vsShell.LoadPackage(ref packageId, out vsPackage);
            }
            catch { }
        }

        string GetLatestiOSSDK()
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));

            var commandBus = componentModel.GetService<ICommandBus>();
            var sdkInfo = commandBus.Execute(new IOSCommands.GetSdkInfo());

            return $"{sdkInfo.LatestInstalledSdks[SdkType.iOS]}"; //quotes are so the engine understands this as a string
        }

        bool AndroidShouldFallback()
        {
            try
            {
                var componentModel = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
                var commandBus = componentModel?.GetService<ICommandBus>();
                var versions = commandBus?.Execute(new AndroidCommands.GetSdkInfo());
                var frameworks = versions?.Frameworks;

                if (!frameworks.First(f => f.ApiLevel == CurrentAndroidLevel).IsInstalled)
                {
                    AndroidTargetFramework = frameworks.First(f => f.ApiLevel == FallbackAndroidLevel);
                    return true;
                }
                else
                    return false;
            }
            catch (FileNotFoundException)//this is to avoid a known watson crash
            {
                return false;
            }
        }

        private CrossPlatformDialog CreateCrossPlatformDialog()
        {
            var dialog = new CrossPlatformDialog();
            var dialogWindow = dialog as System.Windows.Window;
            if (dialogWindow != null)
            {
                var uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;

                IntPtr owner;
                uiShell.GetDialogOwnerHwnd(out owner);
                new WindowInteropHelper(dialogWindow).Owner = owner;
                dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialogWindow.ShowInTaskbar = false;
                // This would not set the right owner.
                //dialogWindow.Owner = Application.Current.MainWindow;
            }

            return dialog;
        }

        public void BeforeOpeningFile(ProjectItem projectItem) { }

        public void ProjectFinishedGenerating(Project project) { }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }

        public void RunFinished()
        {
            var result = new CreateTemplateResult(SafeProjectName, model);

            try
            {
                CreateTemplate(model);

                result.CheckIfSolutionWasSuccessfulyCreated(dte.Solution);

                CrossPlatformTelemetry.Events.NewProject.Create.Post(result);
            } catch (Exception ex) {
                CrossPlatformTelemetry.Events.NewProject.Fault.Post(result, ex);
                throw;
            }
        }

        private void CreateTemplate(XPlatViewModel model)
        {
            var wizard = CreateTemplatingWizard();
            wizard.RunStarted(automationObject, AddReplacements(model, replacements), WizardRunKind.AsMultiProject, new object[]{ });
            wizard.RunFinished();
        }

        private Dictionary<string, string> AddReplacements(XPlatViewModel model, Dictionary<string, string> replacements)
        {
            replacements.Add("$uistyle$", "none");
            replacements.Add("$language$", "CSharp");
            replacements.Add("$groupid$", "Xamarin.Forms.App");

            replacements.Add("$passthrough:kind$", model.SelectedTemplatePath);

            if (model.IsAzureSelected)
            {
                replacements.Add("$passthrough:CreateBackendProject$", "true");
                replacements.Add("$passthrough:IncludeXamarinEssentials$", "true");
            }
            if (!model.IsSharedSelected)
                replacements.Add("$passthrough:CreateSharedProject$", "false");

            if (model.IsAndroidSelected)
            {
                if (AndroidShouldFallback())
                {
                    replacements.Add("$passthrough:AndroidSDKVersion$", AndroidTargetFramework.Version);
                    replacements.Add("$passthrough:TargetAndroidAPI$", AndroidTargetFramework.ApiLevel.ToString());
                    replacements.Add("$passthrough:SupportLibVersion$", FallbackSupportLibVersion);
                }
            }
            else
            {
                replacements.Add("$passthrough:CreateAndroidProject$", "false");
            }

            if (model.IsIOSSelected)
            {
                var iosSdk = GetLatestiOSSDK();
                if (!string.IsNullOrEmpty(iosSdk))
                    replacements.Add("$passthrough:MinimumOSVersion$", iosSdk);
                replacements.Add("$passthrough:AppIdentifier$", $"com.companyname.{replacements["$safeprojectname$"]}");
            }
            else
            {
                replacements.Add("$passthrough:CreateiOSProject$", "false");
            }

            replacements.Add("$passthrough:CreateUWPProject$", "false");

            return replacements;
        }

        private IWizard CreateTemplatingWizard()
        {
            var assembly = Assembly.Load("Microsoft.VisualStudio.TemplateEngine.Wizard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var type = assembly.GetType("Microsoft.VisualStudio.TemplateEngine.Wizard.TemplateEngineWizard", true);
            return (IWizard)Activator.CreateInstance(type);
        }

        public bool ShouldAddProjectItem(string filePath) => true; //filter mobile app when no azure

        string SafeProjectName => GetReplacementValue("$safeprojectname$");

        string GetReplacementValue(string key)
        {
            string value;
            replacements.TryGetValue(key, out value);
            return value;
        }
    }
}
