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

namespace Xamarin.Templates.Wizard
{
	abstract class DialogWizardBase<Dialog, Model> : IWizard where Dialog : DialogBase, new() where Model : IViewModel
	{
		protected Model model;
		protected Dictionary<string, string> replacements;
		object automationObject;
		private DTE2 dte;

		public void RunFinished()
		{
			var result = new BaseCreateTemplateResult(SafeProjectName, model.SelectedTemplate, TelemetryPlatform);

			try
			{
				CreateTemplate();

				result.CheckIfSolutionWasSuccessfulyCreated(dte.Solution);

				BasePlatformTelemetry.Events.NewProject.Create.Post(result);
			}
			catch (Exception ex)
			{
				BasePlatformTelemetry.Events.NewProject.Fault.Post(result, ex);
				throw;
			}
		}

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			try
			{
				this.automationObject = automationObject;
				dte = automationObject as DTE2;

				replacements = replacementsDictionary;

				var dialog = CreateDialog();
				dialog.Title = String.Format("{0} - {1}", dialog.Title, SafeProjectName);

				var dialogResult = dialog.ShowDialog().GetValueOrDefault();

				TelemetryService.DefaultSession.PostEvent(new OpenWizardTelemetryEvent(GetType().Name));

				if (!dialogResult)
				{
					throw new WizardBackoutException();
				}
				model = ((Model)dialog.DataContext);
			}
			catch (WizardBackoutException)
			{
				throw;
			}
			catch
			{
				TelemetryService.DefaultSession.PostEvent(new OpenWizardTelemetryEvent(GetType().Name, true));
			}
		}

		private Dialog CreateDialog()
		{
			var dialog = new Dialog();
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

		protected abstract Dictionary<string, string> AddReplacements();

		protected abstract string TelemetryPlatform { get; }
	}
}
