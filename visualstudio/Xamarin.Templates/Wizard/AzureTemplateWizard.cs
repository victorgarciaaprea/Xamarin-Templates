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

namespace Xamarin.Templates.Wizards
{
    public class AzureTemplateWizard : IWizard
    {
        enum TemplateLanguage { CSharp, FSharp };

        DTE2 dte;
        ServiceProvider serviceProvider;
        Dictionary<string, string> replacements;
        XPlatViewModel model;
        object automationObject;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            replacements = replacementsDictionary;
            dte = automationObject as DTE2;
            this.automationObject = automationObject;
            serviceProvider = new ServiceProvider(automationObject as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

            TryLoadNuGetPackage(serviceProvider);

            var dialog = CreateAzureDialog();
            dialog.SetUWPEnabled(dte);
            dialog.Title = String.Format("{0} - {1}", dialog.Title, SafeProjectName);
            if (!dialog.ShowDialog().GetValueOrDefault())
            {
                throw new WizardBackoutException();
            }

            model = ((XPlatViewModel)dialog.DataContext);
        }

        void TryLoadNuGetPackage(IServiceProvider serviceProvider)
        {
            try
            {
                var packageId = new Guid("5fcc8577-4feb-4d04-ad72-d6c629b083cc");
                var vsShell = (IVsShell)serviceProvider.GetService(typeof(SVsShell));
                var vsPackage = default(IVsPackage);

                vsShell.IsPackageLoaded(ref packageId, out vsPackage);

                if (vsPackage == null)
                    vsShell.LoadPackage(ref packageId, out vsPackage);
            }
            catch { }
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
            var solution = dte.Solution as Solution2;

            if (model.IsAzureSelected)
            {
                await ShowAzureDialog();
            }

            CreateTemplate(model);
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
            replacements.Add("$groupid$", model.SelectedTemplatePath);
            if (model.IsAzureSelected)
                replacements.Add("$passthrough:CreateBackendProject$", "true");
            if (!model.IsSharedSelected)
                replacements.Add("$passthrough:CreateSharedProject$", "false");
            if (!model.IsAndroidSelected)
                replacements.Add("$passthrough:CreateAndroidProject$", "false");
            if (!model.IsIOSSelected)
                replacements.Add("$passthrough:CreateiOSProject$", "false");
            if (!model.IsUWPSelected)
                replacements.Add("$passthrough:CreateUWPProject$", "false");

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
