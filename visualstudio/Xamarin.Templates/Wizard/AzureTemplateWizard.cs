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

namespace Xamarin.Templates.Wizards
{
    public class AzureTemplateWizard : IWizard
    {
        enum TemplateLanguage { CSharp, FSharp };

        DTE2 dte;
        ServiceProvider serviceProvider;
        Dictionary<string, string> replacements;
        private bool useAzure;
        private string templateId;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            replacements = replacementsDictionary;
            dte = automationObject as DTE2;
            serviceProvider = new ServiceProvider(automationObject as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

            TryLoadNuGetPackage(serviceProvider);

            var dialog = CreateAzureDialog();
            dialog.Title = String.Format("{0} - {1}", dialog.Title, SafeProjectName);
            if (!dialog.ShowDialog().GetValueOrDefault())
            {
                throw new WizardBackoutException();
            }

            templateId = dialog.GetTemplatePath();
            useAzure = ((XPlatViewModel)dialog.DataContext).IsAzureSelected;
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

            if (useAzure)
            {
                await ShowAzureDialog();
            }

            string templatePath = solution.GetProjectTemplate(templateId, "CSharp");
            dte.Solution.AddFromTemplate(templatePath, SolutionPath, SafeProjectName);
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

        string GetTemplatesPath()
        {
            return Path.Combine(Path.GetDirectoryName(new Uri(typeof(AzureTemplateWizard).Assembly.CodeBase).LocalPath), "T");
        }

        string GetProjectTemplateFile(string templateName, TemplateLanguage language = TemplateLanguage.CSharp)
        {
            return Path.Combine(GetTemplatesPath(), "P", language.ToString(), "Cross-Platform", templateName + ".zip");
        }

        string SolutionPath
        {
            get { return GetReplacementValue("$destinationdirectory$"); }
        }
    }
}
