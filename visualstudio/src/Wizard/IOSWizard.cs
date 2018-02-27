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

namespace Xamarin.Templates.Wizard
{
	class IOSTemplateWizard : DialogWizardBase<IOSDialog, IOSViewModel>
	{ 
		//this one needs overriding
		protected override Dictionary<string, string> AddReplacements()
		{
			replacements.Add("$uistyle$", "none");
			replacements.Add("$language$", "CSharp");
			replacements.Add("$groupid$", "Xamarin.iOS.App");

			replacements.Add("$passthrough:kind$", model.SelectedTemplate.Id);
			replacements.Add("$passthrough:MinimumOSVersion", model.MinOSVersion);
			replacements.Add("$passthrough:DeviceFamily$", model.DeviceFamily);

			return replacements;
		}

	}
}
