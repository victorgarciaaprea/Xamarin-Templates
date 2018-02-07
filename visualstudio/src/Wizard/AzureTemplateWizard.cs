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

namespace Xamarin.Templates.Wizards
{
    public class AzureTemplateWizard : IWizard
    {
        enum TemplateLanguage { CSharp, FSharp };

        const string NugetPackage = "5fcc8577-4feb-4d04-ad72-d6c629b083cc";
        const string AndroidPackage = "296e6a4e-2bd5-44b7-a96d-8ee3d9cda2f6";
        const string IOSPackage = "77875fa9-01e7-4fea-8e77-dfe942355ca1";


        DTE2 dte;
        ServiceProvider serviceProvider;
        Dictionary<string, string> replacements;
        XPlatViewModel model;
        object automationObject;

        internal static Version MinWindowsVersion = new Version(10, 0, 16267, 0);
        string latestWindowSdk;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            replacements = replacementsDictionary;
            dte = automationObject as DTE2;
            this.automationObject = automationObject;
            serviceProvider = new ServiceProvider(automationObject as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

            TryLoadPackage(serviceProvider, NugetPackage); 

            System.Threading.Tasks.Task.Run(() => InitializeTemplateEngine());

            latestWindowSdk = GetLatestWindowsSDK();
            
            var dialog = CreateAzureDialog();
            dialog.SetUWPEnabled(dte, latestWindowSdk);
            dialog.Title = String.Format("{0} - {1}", dialog.Title, SafeProjectName);
            if (!dialog.ShowDialog().GetValueOrDefault())
            {
                throw new WizardBackoutException();
            }
            model = ((XPlatViewModel)dialog.DataContext);
        }

        private void InitializeTemplateEngine()
        {
            try
            {
                var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));

                var initializer = componentModel.DefaultExportProvider.GetExport<object>("Microsoft.VisualStudio.TemplateEngine.Contracts.IEngineInitializer").Value;

                initializer.GetType().GetMethod("EnsureInitialized").Invoke(initializer, null);

                //we need these two to get sdk information, so initialize them if possible to speed up the template
                TryLoadPackage(serviceProvider, AndroidPackage);
                TryLoadPackage(serviceProvider, IOSPackage);
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

        string GetLatestWindowsSDK()
        {
            var sdks = Microsoft.Build.Utilities.ToolLocationHelper.GetPlatformsForSDK("Windows", new Version(10, 0))
                       .Where(s => s.StartsWith("UAP")).Select(s => new Version(s.Substring(13))).Where(v => v >= MinWindowsVersion); //the value is of the form "UAP, Version=x.x.x.x"
            
            return sdks.Count() > 0 ? $"{sdks.First()}": string.Empty;
        }

        string GetLatestAndroidSDK()
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));

            var commandBus = componentModel.GetService<ICommandBus>();
            var sdkInfo = commandBus.Execute<AndroidModel.SdkInfo>(new AndroidCommands.GetSdkInfo());
            
			if (sdkInfo.LatestInstalledFramework == null)
				return string.Empty;

            return $"{sdkInfo.LatestInstalledFramework.Version}"; //quotes are so the engine understands this as a string
        }

        string GetLatestiOSSDK()
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));

            var commandBus = componentModel.GetService<ICommandBus>();
            var sdkInfo = commandBus.Execute<IOSModel.SdkInfo>(new IOSCommands.GetSdkInfo());

            return $"{sdkInfo.LatestInstalledIOSSdk}"; //quotes are so the engine understands this as a string
        }

        private AzureDialog CreateAzureDialog()
        {
            var dialog = new AzureDialog();
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

        public async void RunFinished()
        {
            var result = new CreateTemplateResult(SafeProjectName, model);

            try
            {
                if (model.IsAzureSelected)
                    await ShowAzureDialog();

                CreateTemplate(model);

                result.CheckIfSolutionWasSuccessfulyCreated(dte.Solution);
                
                Telemetry.Events.NewProject.Create.Post(result);
            } catch (Exception ex) {
                Telemetry.Events.NewProject.Fault.Post(result, ex);
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
                replacements.Add("$passthrough:CreateBackendProject$", "true");
            if (!model.IsSharedSelected)
                replacements.Add("$passthrough:CreateSharedProject$", "false");

            if (model.IsAndroidSelected)
            {
                var androidSdk = GetLatestAndroidSDK();
                if (!string.IsNullOrEmpty(androidSdk))
                    replacements.Add("$passthrough:AndroidSdk$", androidSdk);
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

            if (model.IsUWPSelected)
            {
                replacements.Add("$passthrough:WindowsSdk$", latestWindowSdk);
            }
            else
            {
                replacements.Add("$passthrough:CreateUWPProject$", "false");
            }

            return replacements;
        }

        private IWizard CreateTemplatingWizard()
        {
            var assembly = Assembly.Load("Microsoft.VisualStudio.TemplateEngine.Wizard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var type = assembly.GetType("Microsoft.VisualStudio.TemplateEngine.Wizard.TemplateEngineWizard", true);
            return (IWizard)Activator.CreateInstance(type);
        }

        private async System.Threading.Tasks.Task ShowAzureDialog()
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));

            var iAzureShoppingCartDeploymentDialogFactoryType = Type.GetType($"Microsoft.VisualStudio.Web.WindowsAzure.CommonContracts.IAzureShoppingCartDeploymentDialogFactory, Microsoft.VisualStudio.Web.WindowsAzure.CommonContracts, Version={dte.Version}.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false);
            var iAzureShoppingCartDeploymentDialogType = Type.GetType($"Microsoft.VisualStudio.Web.WindowsAzure.CommonContracts.IAzureShoppingCartDeploymentDialog, Microsoft.VisualStudio.Web.WindowsAzure.CommonContracts, Version={dte.Version}.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false);
            object provisioningFactory = typeof(IComponentModel).GetMethod("GetService").MakeGenericMethod(iAzureShoppingCartDeploymentDialogFactoryType).Invoke(componentModel, null);

            var provisioningDialogTask = (System.Threading.Tasks.Task)iAzureShoppingCartDeploymentDialogFactoryType.GetMethod("CreateAsync")
            .Invoke(provisioningFactory,
            new object[] {
                    "Microsoft.Web/sites",
                    new Dictionary<string, object>
                    {
                        { "ProjectName" , SafeProjectName },
                        {"AppServiceKind", "MobileApp"},
                    }});
            await provisioningDialogTask;
            object provisioningDialog = typeof(Task<>).MakeGenericType(iAzureShoppingCartDeploymentDialogType)
                .GetProperty("Result").GetValue(provisioningDialogTask);
            var showModalMethod = iAzureShoppingCartDeploymentDialogType.GetMethod("ShowModal");
            var primaryEntityProperty = iAzureShoppingCartDeploymentDialogType.GetProperty("PrimaryEntity");
            bool? result = (bool?)showModalMethod.Invoke(provisioningDialog, null);

            if (result.GetValueOrDefault())
            {
                var entity = primaryEntityProperty.GetValue(provisioningDialog);
                var name = (entity != null) ? (string)entity.GetType().GetProperty("Name").GetValue(entity) : null;
                //AzureMobileProjectWizard.AzureMobileAppName = name ?? "[CONFIGURE-THIS-URL]";
            }
        }

        public bool ShouldAddProjectItem(string filePath) => true; //filter mobile app when no azure

        string SafeProjectName
        {
            get { return GetReplacementValue("$safeprojectname$"); }
        }

        string GetReplacementValue(string key)
        {
            string value;
            replacements.TryGetValue(key, out value);
            return value;
        }

        string SolutionPath
        {
            get { return GetReplacementValue("$destinationdirectory$"); }
        }
    }
}
