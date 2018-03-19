using Merq;
using Microsoft.VisualStudio.ComponentModelHost;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Xamarin.Templates.Properties;
using Xamarin.VisualStudio.Contracts.Commands.IOS;

namespace Xamarin.Templates.Wizard
{
    class IOSViewModel : IViewModel, INotifyPropertyChanged
	{
		public List<ItemViewModel> Templates { get; private set; }

		ItemViewModel selectedTemplate;
		public ItemViewModel SelectedTemplate
		{
			get { return selectedTemplate ?? Templates.First(); }
			set
			{
				selectedTemplate = value;

				PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedTemplate)));
			}
		}

		public IOSViewModel()
		{
			Templates = CreateTemplatesContext();
			Frameworks = GetFrameworks();

			IsUniversal = true;
		}
		
		static IList<string> GetFrameworks()
		{
			try
			{
				var componentModel = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
				var commandBus = componentModel?.GetService<ICommandBus>();
				var versions = commandBus?.Execute(new GetSdkInfo());
				var ret = versions?.Versions.Select(f => f.Version).ToList();
				if (ret != null && ret.Count() > 0)
					return ret;
				else 
					return new List<string>
						{ "11.2", "11.1", "11.0", "10.3", "10.2", "10.1", "10.0", "9.3", "9.2", "9.1", "9.0", "8.4", "8.3", "8.2", "8.1", "8.0" };
			}
			catch (FileNotFoundException)//this is to avoid a known watson crash
			{
				return new List<string>();
			}
		}

		public IList<string> Frameworks { get; set; }

		public List<ItemViewModel> CreateTemplatesContext()
		{
			var icon = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"FormsProject.jpg");
			return new List<ItemViewModel>
			{
				new ItemViewModel { Id = "single-view", Name = Resources.IOSViewModel_SingleViewApp_Name, Icon = icon, Description = Resources.IOSViewModel_SingleViewApp_Description },
				new ItemViewModel { Id = "master-detail", Name = Resources.IOSViewModel_MasterDetailApp_Name, Icon = icon, Description = Resources.IOSViewModel_MasterDetailApp_Description },
				new ItemViewModel { Id = "tabbed", Name = Resources.IOSViewModel_TabbedApp_Name, Icon = icon, Description = Resources.IOSViewModel_TabbedApp_Description },
				new ItemViewModel { Id = "blank", Name = Resources.IOSViewModel_BlankApp_Name, Icon = icon, Description = Resources.IOSViewModel_BlankApp_Description }
			};
		}

		public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
		
		public string DeviceFamily
		{
			get
			{
				if (IsIPad)
					return "ipad";
				if (IsIPhone)
					return "iphone";
				return "universal";
			}
		}

		public string MinOSVersion { get; set; }

		public bool IsUniversal { get; set; }
		public bool IsIPhone { get; set; }
		public bool IsIPad { get; set; }
	}

	public class ItemViewModel
	{
		public string Id { get; set; }
		public string Icon { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}

