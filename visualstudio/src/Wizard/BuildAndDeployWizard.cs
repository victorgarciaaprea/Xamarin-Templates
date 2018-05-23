using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Templates
{
    public class BuildAndDeployWizard : WizardBase
    {
        public override void RunFinished()
        {
            if (!IsUnfoldingInAnExistingSolution)
            {
                foreach (SolutionConfiguration solutionConfiguration in DTE.Solution.SolutionBuild.SolutionConfigurations)
                    foreach (SolutionContext solutionContext in solutionConfiguration.SolutionContexts)
                        solutionContext.ShouldBuild = solutionContext.ShouldDeploy = true;
            }
        }
    }
}
