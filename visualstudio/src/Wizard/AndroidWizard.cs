﻿using EnvDTE;
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

namespace Xamarin.Templates.Wizard
{
	public class AndroidTemplateWizard  : IWizard
	{
		AndroidViewModel model;
		Dictionary<string, string> replacements;
		object automationObject;

		public void RunFinished()
		{
			CreateTemplate(model);
		}

		private void CreateTemplate(AndroidViewModel model)
		{
			var wizard = CreateTemplatingWizard();
			wizard.RunStarted(automationObject, AddReplacements(model, replacements), WizardRunKind.AsMultiProject, new object[] { });
			wizard.RunFinished();
		}

		private Dictionary<string, string> AddReplacements(AndroidViewModel model, Dictionary<string, string> replacements)
		{
			replacements.Add("$uistyle$", "none");
			replacements.Add("$language$", "CSharp");
			replacements.Add("$groupid$", "Xamarin.Android.App");

			replacements.Add("$passthrough:kind$", model.SelectedTemplate.Id);
			replacements.Add("$passthrough:MinAndroidAPI$", model.AndroidFramework.ApiLevel.ToString());

			return replacements;
		}

		private IWizard CreateTemplatingWizard()
		{
			var assembly = Assembly.Load("Microsoft.VisualStudio.TemplateEngine.Wizard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
			var type = assembly.GetType("Microsoft.VisualStudio.TemplateEngine.Wizard.TemplateEngineWizard", true);
			return (IWizard)Activator.CreateInstance(type);
		}

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			this.automationObject = automationObject;

			replacements = replacementsDictionary;

			var dialog = CreateAndroidDialog();
			dialog.Title = String.Format("{0} - {1}", dialog.Title, SafeProjectName);
			if (!dialog.ShowDialog().GetValueOrDefault())
			{
				throw new WizardBackoutException();
			}
			model = ((AndroidViewModel)dialog.DataContext);
		}

		private AndroidDialog CreateAndroidDialog()
		{
			var dialog = new AndroidDialog();
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

	}
}
