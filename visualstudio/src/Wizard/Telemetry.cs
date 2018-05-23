using Merq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Templates.Wizard;
using Xamarin.VisualStudio.Contracts.Commands;
using Xamarin.VisualStudio.Contracts.Model;

namespace Xamarin.Templates.Wizards
{
    internal static class BasePlatformTelemetry
    {
        public static class Events
        {
            public static class NewProject
            {

                public const string ProjectTemplate = "ProjectTemplate"; // Name of the selected project template.Example values include "Blank" or "MasterDetail".
                public const string Success = "Success"; // Was the wizard successful without any exceptions?
                public const string TargetPlatform = "TargetPlatform"; // iOS, Android, and/or Windows. See "Piping Data with Multiple Values Per Property" section below.
                public const string XamarinTemplatesVersion = "XamarinTemplatesVersion"; // Xamarin.Forms template extension version
                public const string XamarinVersion = "XamarinVersion"; // Xamarin extension for Visual Studio version

                static TelemetryEvent CreateEvent(string id, string eventNamespace, BaseCreateTemplateResult createTemplateResult)
                {
                    var telemetryEvent = new TelemetryEvent(id);
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, XamarinVersion), TelemetryShared.GetXVSVersion());
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, XamarinTemplatesVersion), ThisAssembly.InformationalVersion);
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, ProjectTemplate), createTemplateResult.SelectedTemplateId);
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, TargetPlatform), createTemplateResult.Platform);
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, Success), createTemplateResult.Success);
                    return telemetryEvent;
                }


                public static class Create
                {
                    public const string EventNamespace = "VS.Xamarin.NewProject.Create";
                    public const string Id = "vs/xamarin/newproject/create";

                    public static void Post(BaseCreateTemplateResult createTemplateResult)
                    {
                        if (createTemplateResult.Success)
                        {
                            var telemetryEvent = CreateEvent(Id, EventNamespace, createTemplateResult);

                            TelemetryService.DefaultSession.PostEvent(telemetryEvent);
                        }
                        else
                        {
                            Fault.Post(createTemplateResult);
                        }
                    }
                }

                public static class Fault
                {
                    public const string EventNamespace = "VS.Xamarin.NewProject.Create.Fault";
                    public const string Id = "vs/xamarin/newproject/create/fault";
                    public const string TemplateException = "VS.Xamarin.NewProject.Create.Fault.TemplateException"; // iOS, Android, and/or Windows. See "Piping Data with Multiple Values Per Property" section below.

                    public static void Post(BaseCreateTemplateResult createTemplateResult)
                    {
                        var telemetryEvent = CreateEvent(Id, EventNamespace, createTemplateResult);

                        TelemetryService.DefaultSession.PostEvent(telemetryEvent);
                    }

                    public static void Post(BaseCreateTemplateResult createTemplateResult, Exception exception)
                    {
                        var telemetryEvent = CreateEvent(Id, EventNamespace, createTemplateResult);
                        telemetryEvent.Properties.Add(TemplateException, exception.Message);

                        TelemetryService.DefaultSession.PostEvent(telemetryEvent);
                    }
                }
            }
        }
    }

    internal static class CrossPlatformTelemetry
    {
        public static class Events
        {
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

                static TelemetryEvent CreateEvent(string id, string eventNamespace, CreateTemplateResult createTemplateResult)
                {
                    var telemetryEvent = new TelemetryEvent(id);
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, XamarinVersion), TelemetryShared.GetXVSVersion());
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, XamarinTemplatesVersion), ThisAssembly.InformationalVersion);
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, CodeSharingStrategy), createTemplateResult.IsSharedSelected ? "SharedProject" : "NetStandard");
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, ProjectTemplate), createTemplateResult.SelectedTemplateName);
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, TargetPlatforms), string.Join("|", createTemplateResult.Platforms));
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, UIStrategy), createTemplateResult.IsNativeSelected ? "native" : "xamarinforms");
                    telemetryEvent.Properties.Add(string.Format("{0}.{1}", eventNamespace, Success), createTemplateResult.Success);
                    return telemetryEvent;
                }

                public static class Create
                {
                    public const string EventNamespace = "VS.Xamarin.NewProject.Create";
                    public const string Id = "vs/xamarin/newproject/create";

                    public static void Post(CreateTemplateResult createTemplateResult)
                    {
                        if (createTemplateResult.Success)
                        {
                            var telemetryEvent = CreateEvent(Id, EventNamespace, createTemplateResult);

                            TelemetryService.DefaultSession.PostEvent(telemetryEvent);
                        }
                        else
                        {
                            Fault.Post(createTemplateResult);
                        }
                    }
                }

                public static class Fault
                {
                    public const string EventNamespace = "VS.Xamarin.NewProject.Create.Fault";
                    public const string Id = "vs/xamarin/newproject/create/fault";
                    public const string FailedTargetPlatforms = "VS.Xamarin.NewProject.Create.Fault.FailedTargetPlatforms"; // iOS, Android, and/or Windows. See "Piping Data with Multiple Values Per Property" section below.
                    public const string TemplateException = "VS.Xamarin.NewProject.Create.Fault.TemplateException"; // iOS, Android, and/or Windows. See "Piping Data with Multiple Values Per Property" section below.

                    public static void Post(CreateTemplateResult createTemplateResult)
                    {
                        var telemetryEvent = CreateEvent(Id, EventNamespace, createTemplateResult);
                        telemetryEvent.Properties.Add(FailedTargetPlatforms, string.Join("|", createTemplateResult.FailedPlatforms));

                        TelemetryService.DefaultSession.PostEvent(telemetryEvent);
                    }

                    public static void Post(CreateTemplateResult createTemplateResult, Exception exception)
                    {
                        var telemetryEvent = CreateEvent(Id, EventNamespace, createTemplateResult);
                        telemetryEvent.Properties.Add(TemplateException, exception.Message);

                        TelemetryService.DefaultSession.PostEvent(telemetryEvent);
                    }
                }
            }
        }
    }

    public static class TelemetryShared
    {
        public static string GetXVSVersion()
        {
            try
            {
                var componentModel = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
                var commandBus = componentModel?.GetService<ICommandBus>();
                var versions = commandBus?.Execute(new GetVersions());
                return versions?.XVSInformationalVersion;
            }
            catch (FileNotFoundException)//this is to avoid a known watson crash
            {
                return string.Empty;
            }
        }
    }
}
