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
    class AndroidTemplateWizard : DialogWizardBase<AndroidDialog, AndroidViewModel>
    {
        protected override Dictionary<string, string> AddReplacements()
        {
            replacements.Add("$uistyle$", "none");
            replacements.Add("$language$", "CSharp");
            replacements.Add("$groupid$", "Xamarin.Android.App");

            replacements.Add("$passthrough:kind$", model.SelectedTemplate.Id);
            replacements.Add("$passthrough:MinAndroidAPI$", model.AndroidMinFramework.ApiLevel.ToString());

            if (model.ShouldFallback)
            {
                replacements.Add("$passthrough:AndroidSDKVersion$", model.AndroidTargetFramework.Version);
                replacements.Add("$passthrough:TargetAndroidAPI$", model.AndroidTargetFramework.ApiLevel.ToString());
                replacements.Add("$passthrough:SupportLibVersion$", model.FallbackSupportLibVersion);
            }

            return replacements;
        }

        internal override AndroidViewModel PrefillModel()
        {
            model = new AndroidViewModel();

            if (replacements.ContainsKey("ApiLevel"))
            {
                model.AndroidMinFramework = model.AndroidFrameworks.FirstOrDefault(f => f.ApiLevel == int.Parse(replacements["ApiLevel"]));
            }
            if (replacements.ContainsKey("kind"))
            { 
                model.SelectedTemplate = model.Templates.FirstOrDefault(t => t.Id == replacements["kind"]);
            }

            return model;
        }


        protected override string TelemetryPlatform => "Android";
    }
}
