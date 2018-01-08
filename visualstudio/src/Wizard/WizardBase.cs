using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Templates
{
	public class WizardBase : IWizard
	{
		protected DTE DTE { get; private set; }

		protected bool IsUnfoldingInAnExistingSolution { get; private set; }

		public virtual void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public virtual void ProjectFinishedGenerating(Project project)
		{
		}

		public virtual void ProjectItemFinishedGenerating(ProjectItem projectItem)
		{
		}

		public virtual void RunFinished()
		{
		}

		public virtual void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			DTE = (DTE)automationObject;
			IsUnfoldingInAnExistingSolution = DTE.Solution != null && DTE.Solution.IsOpen;
		}

		public virtual bool ShouldAddProjectItem(string filePath) => true;

	}
}

