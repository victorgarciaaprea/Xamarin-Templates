using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Telemetry;

namespace Xamarin.Templates.Wizards
{
	class OpenWizardTelemetryEvent : TelemetryEvent
	{
		const string EventName = "vs/xamarin/newproject/open-wizard";
		const string EventNameFault = EventName + "/fault";
		const string EventNamespace = "VS.Xamarin.NewProject.OpenWizard";

		public OpenWizardTelemetryEvent(string wizardName, bool fault = false)
			: base(fault ? EventNameFault : EventName)
		{
			WizardName = wizardName;
			TemplatesVersion = ThisAssembly.InformationalVersion;
			XamarinVersion = TelemetryShared.GetXVSVersion();
		}

		public string WizardName
		{
			get => GetPropertyValue();
			set => SetPropertyValue(value);
		}

		string TemplatesVersion
		{
			get => GetPropertyValue();
			set => SetPropertyValue(value);
		}

		string XamarinVersion
		{
			get => GetPropertyValue();
			set => SetPropertyValue(value);
		}

		void SetPropertyValue(string value, [CallerMemberName] string memberName = null) =>
			Properties.Add($"{EventNamespace}.{memberName}", value);

		string GetPropertyValue([CallerMemberName] string memberName = null) =>
			Properties[memberName] as string;

	}
}