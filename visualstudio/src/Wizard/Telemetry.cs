using Merq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;
using Xamarin.VisualStudio.Contracts.Commands;
using Xamarin.VisualStudio.Contracts.Model;

namespace Xamarin.Templates.Wizards
{
    internal static class Telemetry
    {
        public static class Events
        {
            static string GetXVSVersion()
            {
                var componentModel = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
                var commandBus = componentModel?.GetService<ICommandBus>();
                var versions = commandBus?.Execute(new GetVersions());
                return versions?.XVSInformationalVersion;
            }

            public static class NewProject
            {
                public const string CodeSharingStrategy = "CodeSharingStrategy"; // Name of the selected code sharing strategy.Example values include "SharedProject" or "NetStandard". 
                public const string ProjectTemplate = "ProjectTemplate"; // Name of the selected project template.Example values include "Blank" or "MasterDetail". 
                public const string Success = "Success"; // Was the wizard successful without any exceptions?
                public const string TargetPlatforms = "TargetPlatforms"; // iOS, Android, and/or Windows. See "Piping Data with Multiple Values Per Property" section below. 
                public const string FailedTargetPlatforms = "FailedTargetPlatforms"; // iOS, Android, and/or Windows. See "Piping Data with Multiple Values Per Property" section below. 
                public const string UIStrategy = "UIStrategy"; // Name of the selected UI strategy. Example values include "native" or "xamarinforms". 
                public const string XamarinTemplatesVersion = "XamarinTemplatesVersion"; // Xamarin.Forms template extension version
                public const string XamarinVersion = "XamarinVersion"; // Xamarin extension for Visual Studio version
                
                static void SetNewProjectInfo(TelemetryEvent telemetryEvent, CreateTemplateResult createTemplateResult)
                {
                    telemetryEvent.Properties.Add(XamarinVersion, GetXVSVersion());
                    telemetryEvent.Properties.Add(XamarinTemplatesVersion, ThisAssembly.InformationalVersion);
                    telemetryEvent.Properties.Add(CodeSharingStrategy, createTemplateResult.IsSharedSelected ? "SharedProject" : "NetStandard");
                    telemetryEvent.Properties.Add(ProjectTemplate, createTemplateResult.SelectedTemplateName);
                    telemetryEvent.Properties.Add(TargetPlatforms, string.Join("|", createTemplateResult.Platforms));
                    telemetryEvent.Properties.Add(UIStrategy, createTemplateResult.IsNativeSelected ? "native" : "xamarinforms");
                    telemetryEvent.Properties.Add(Success, createTemplateResult.Success);
                }
                
                public static class Create
                {
                    public const string Id = "vs/xamarin/newproject/create";

                    public static void Post(CreateTemplateResult createTemplateResult)
                    {
                        if (createTemplateResult.Success)
                        {
                            var telemetryEvent = new TelemetryEvent(Id);
                            SetNewProjectInfo(telemetryEvent, createTemplateResult);

                            TelemetryService.DefaultSession.PostEvent(telemetryEvent);
                        } else
                        {
                            Fault.Post(createTemplateResult);
                        }
                    }
                }

                public static class Fault
                {
                    public const string Id = "vs/xamarin/newproject/create/fault";
                    public const string FailedTargetPlatforms = "FailedTargetPlatforms"; // iOS, Android, and/or Windows. See "Piping Data with Multiple Values Per Property" section below. 
                    public const string TemplateException = "TemplateException"; // iOS, Android, and/or Windows. See "Piping Data with Multiple Values Per Property" section below. 

                    public static void Post(CreateTemplateResult createTemplateResult)
                    {
                        var telemetryEvent = new TelemetryEvent(Id);
                        SetNewProjectInfo(telemetryEvent, createTemplateResult);

                        telemetryEvent.Properties.Add(FailedTargetPlatforms, string.Join("|", createTemplateResult.FailedPlatforms));

                        TelemetryService.DefaultSession.PostEvent(telemetryEvent);
                    }

                    public static void Post(CreateTemplateResult createTemplateResult, Exception exception)
                    {
                        var telemetryEvent = new TelemetryEvent(Id);
                        SetNewProjectInfo(telemetryEvent, createTemplateResult);

                        telemetryEvent.Properties.Add(TemplateException, exception.Message);

                        TelemetryService.DefaultSession.PostEvent(telemetryEvent);
                    }
                }
            }
        }
    }
}