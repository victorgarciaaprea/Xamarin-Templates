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
            replacements.Add("$templateid$", "Xamarin.iOS.App.CSharp");

            replacements.Add("$passthrough:kind$", model.SelectedTemplate.Id);
            replacements.Add("$passthrough:MinimumOSVersion", model.MinOSVersion);
            replacements.Add("$passthrough:DeviceFamily$", model.DeviceFamily);

            return replacements;
        }

        internal override IOSViewModel PrefillModel()
        {
            model = new IOSViewModel();

            if (replacements.ContainsKey("MinimumOSVersion"))
            {
                model.MinOSVersion = replacements["MinimumOSVersion"];
            }
            if (replacements.ContainsKey("IsIPhone"))
            {
                model.IsIPhone = bool.Parse(replacements["IsIPhone"]);
            }
            if (replacements.ContainsKey("IsIPad"))
            {
                model.IsIPad = bool.Parse(replacements["IsIPad"]);
            }
            if (replacements.ContainsKey("kind"))
            {
                model.SelectedTemplate = model.Templates.FirstOrDefault(t => t.Id == replacements["kind"]);
            }

            return model;
        }

        protected override string TelemetryPlatform => "iOS";
    }
}
