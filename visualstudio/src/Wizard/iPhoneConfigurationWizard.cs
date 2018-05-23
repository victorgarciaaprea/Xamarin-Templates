using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Templates
{
    public class iPhoneConfigurationWizard : WizardBase
    {
        public override void RunFinished()
        {
            if (!IsUnfoldingInAnExistingSolution)
            {
                var activeConfiguration = DTE.Solution.SolutionBuild.ActiveConfiguration as SolutionConfiguration2;

                if (activeConfiguration != null && activeConfiguration.PlatformName != "iPhone")
                {
                    foreach (SolutionContext context in activeConfiguration.SolutionContexts)
                        if (context.PlatformName == "iPhone")
                            context.ConfigurationName = $"{context.ConfigurationName}|iPhoneSimulator";
                }
            }
        }
    }
}
