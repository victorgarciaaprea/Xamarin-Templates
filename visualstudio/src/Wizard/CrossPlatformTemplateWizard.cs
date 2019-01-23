using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using System.IO;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows;
using System.Windows.Interop;
using System.Reflection;
using Merq;
using AndroidCommands = Xamarin.VisualStudio.Contracts.Commands.Android;
using IOSCommands = Xamarin.VisualStudio.Contracts.Commands.IOS;
using Xamarin.VisualStudio.Contracts.Model.Android;
using Microsoft.VisualStudio.Telemetry;
using Xamarin.VisualStudio.Contracts.Model.IOS;
using System.ComponentModel;
using Microsoft.VisualStudio.RemoteSettings;

namespace Xamarin.Templates.Wizards
{
    public class CrossPlatformTemplateWizard : IWizard
    {
        enum TemplateLanguage { CSharp, FSharp };

        Guid NuGetPackage = new Guid("5fcc8577-4feb-4d04-ad72-d6c629b083cc");
        Guid AndroidPackage = new Guid("296e6a4e-2bd5-44b7-a96d-8ee3d9cda2f6");
        Guid IOSPackage = new Guid("77875fa9-01e7-4fea-8e77-dfe942355ca1");
        Guid ShellPackage = new Guid("2d510815-1c4e-4210-bd82-3d9d2c56c140");

        const int CurrentAndroidLevel = 27;
        const int FallbackAndroidLevel = 26;
        const string FallbackSupportLibVersion = "26.1.0.1";
        AndroidFramework AndroidTargetFramework;

        DTE2 dte;
        ServiceProvider serviceProvider;
        IComponentModel componentModel;
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

                ThreadHelper.ThrowIfNotOnUIThread();
                serviceProvider = new ServiceProvider(automationObject as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
                var shell = serviceProvider.GetService(typeof(SVsShell)) as IVsShell7;

                InitializeTemplateEngine();

                // Always set remote setting value for whether to open the XAML or not.
                if (!replacements.ContainsKey("$passthrough:OpenXaml$"))
                    replacements["$passthrough:OpenXaml$"] = RemoteSettings.Default.GetValue(nameof(Xamarin), "OpenXaml", false).ToString().ToLowerInvariant();
                if (!replacements.ContainsKey("$passthrough:OpenXamlCs$"))
                    replacements["$passthrough:OpenXamlCs$"] = RemoteSettings.Default.GetValue(nameof(Xamarin), "OpenXamlCs", false).ToString().ToLowerInvariant();

                var headless = replacements.TryGetValue("Headless", out var value) && bool.TryParse(value, out var parsed) && parsed;

                if (!headless)
                { 
                    var dialog = CreateCrossPlatformDialog();
                    dialog.Title = String.Format("{0} - {1}", dialog.Title, SafeProjectName);
                    // Let the dialog load and render fast, and schedule the package loading right after
                    dialog.Loaded += (sender, args) =>
                    {
                        // In this case we can't know ahead of time if users will select one or the other, so 
                        // we preload all packages.
                        ThreadHelper.JoinableTaskFactory.StartOnIdle(async () => await shell.LoadPackageAsync(ref NuGetPackage));
                        ThreadHelper.JoinableTaskFactory.StartOnIdle(async () => await shell.LoadPackageAsync(ref ShellPackage));
                        ThreadHelper.JoinableTaskFactory.StartOnIdle(async () => await shell.LoadPackageAsync(ref AndroidPackage));
                        ThreadHelper.JoinableTaskFactory.StartOnIdle(async () => await shell.LoadPackageAsync(ref IOSPackage));
                    };

                    var dialogResult = dialog.ShowDialog().GetValueOrDefault();

                    TelemetryService.DefaultSession.PostEvent(new OpenWizardTelemetryEvent(GetType().Name));

                    if (!dialogResult)
                    {
                        throw new WizardBackoutException();
                    }
                    model = ((XPlatViewModel)dialog.DataContext);
                }
                else
                {
                    model = FillModel(replacements);
                    ThreadHelper.JoinableTaskFactory.StartOnIdle(async () => await shell.LoadPackageAsync(ref NuGetPackage));
                    ThreadHelper.JoinableTaskFactory.StartOnIdle(async () => await shell.LoadPackageAsync(ref ShellPackage));
                    if (model.IsAndroidSelected)
                        ThreadHelper.JoinableTaskFactory.StartOnIdle(async () => await shell.LoadPackageAsync(ref AndroidPackage));
                    if (model.IsIOSSelected)
                        ThreadHelper.JoinableTaskFactory.StartOnIdle(async () => await shell.LoadPackageAsync(ref IOSPackage));
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

        private T GetValue<T>(Dictionary<string, string> replacements, string key, T defaultValue)
        {
            if (replacements.TryGetValue(key, out var value))
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFrom(value);
            };

            return defaultValue;
        }

        private void InitializeTemplateEngine()
        {
            try
            {
                var initializer = componentModel.DefaultExportProvider.GetExport<object>("Microsoft.VisualStudio.TemplateEngine.Contracts.IEngineInitializer").Value;

                initializer.GetType().GetMethod("EnsureInitialized").Invoke(initializer, null);
            }
            catch //initialization may fail if the initializer doesn't exist... we don't really care in that case
            { }
        }

        string GetLatestiOSSDK()
        {
            var commandBus = componentModel?.GetService<ICommandBus>();
            var sdkInfo = commandBus?.Execute(new IOSCommands.GetSdkInfo());

            return sdkInfo == null ? null : $"{sdkInfo.LatestInstalledSdks[SdkType.iOS]}"; //quotes are so the engine understands this as a string
        }

        bool AndroidShouldFallback()
        {
            try
            {
                var commandBus = componentModel?.GetService<ICommandBus>();
                var versions = commandBus?.Execute(new AndroidCommands.GetSdkInfo());
                var frameworks = versions?.Frameworks;

                if (frameworks != null && !frameworks.First(f => f.ApiLevel == CurrentAndroidLevel).IsInstalled)
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
            ThreadHelper.ThrowIfNotOnUIThread();

            var dialog = new CrossPlatformDialog();
            var dialogWindow = dialog as System.Windows.Window;
            if (dialogWindow != null)
            {
                var uiShell = serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;

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
            replacements.Add("$templateid$", "Xamarin.Forms.App.CSharp");

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

        string SafeProjectName => replacements.TryGetValue("$safeprojectname$", out var value) ? value : "";
    }
}
