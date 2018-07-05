using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Templates.Wizard;

namespace Xamarin.Templates.Wizards
{
    class CreateTemplateResult
    {
        string safeProjectName;
        XPlatViewModel model;

        public CreateTemplateResult(string safeProjectName, XPlatViewModel model)
        {
            this.safeProjectName = safeProjectName;
            this.model = model;
            this.FailedPlatforms = new List<string>();
        }

        public bool IsAndroidSelected { get { return model.IsAndroidSelected; } }

        public bool IsIOSSelected { get { return model.IsIOSSelected; } }

        public bool IsUWPSelected { get { return model.IsUWPSelected; } }

        public bool IncludesMobileBackend => model.IsAzureSelected;

        public IEnumerable<string> FailedPlatforms { get; internal set; }

        public IEnumerable<string> Platforms
        {
            get
            {
                var platforms = new List<string>();

                if (IsAndroidSelected)
                    platforms.Add("Android");

                if (IsIOSSelected)
                    platforms.Add("iOS");

                if (IsUWPSelected)
                    platforms.Add("UWP");

                return platforms;
            }
        }

        public string TargetPlatform => model.SelectedTemplate.TargetPlatform;

        public bool IsSharedSelected { get { return model.IsSharedSelected; } }
        public object SelectedTemplateName { get { return model.SelectedTemplate.Id; } }
        public bool IsNativeSelected { get { return model.IsNativeSelected; } }
        public bool Success { get { return this.Platforms.Any() && !this.FailedPlatforms.Any(); } }

        public void CheckIfSolutionWasSuccessfulyCreated(Solution solution)
        {
            var expectedProjectNames = new List<string>();

            if (model.IsAndroidSelected)
                expectedProjectNames.Add(string.Format("{0}.Android", safeProjectName));

            if (model.IsUWPSelected)
                expectedProjectNames.Add(string.Format("{0}.UWP", safeProjectName));

            if (model.IsIOSSelected)
                expectedProjectNames.Add(string.Format("{0}.iOS", safeProjectName));

            //expected projects
            //if(model.IsAzureSelected)
            //if(model.IsFormsSelected)
            //if (model.IsNativeSelected)
            //if(model.IsPCLSelected)
            //if(model.IsSharedSelected)

            if (expectedProjectNames.Count == 0)
                return;

            foreach (var project in solution.Projects.OfType<Project>())
                expectedProjectNames.RemoveAll(x => x == project.Name);

            var failedPlatforms = expectedProjectNames
                .Select(x => x.Replace(string.Format("{0}.", safeProjectName), string.Empty))
                .ToList();

            FailedPlatforms = failedPlatforms;
        }
    }

    class BaseCreateTemplateResult
    {
        ItemViewModel model;
        string safeProjectName;

        public string Platform { get; }

        public BaseCreateTemplateResult(string safeProjectName, ItemViewModel model, string platform)
        {
            this.model = model;
            this.safeProjectName = safeProjectName;
            this.Platform = platform;
        }

        public bool Success { get; private set; }

        public void CheckIfSolutionWasSuccessfulyCreated(Solution solution)
        {
            Success = solution.Projects.Count > 0;
        }

        public string SelectedTemplateId => model.Id;
    }
}