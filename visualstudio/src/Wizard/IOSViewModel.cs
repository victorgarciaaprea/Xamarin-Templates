using Merq;

using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Xamarin.VisualStudio.Contracts.Model.IOS;
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
				new ItemViewModel { Id = "single-view", Name = "Single View App", Icon = icon, Description = "An iOS app with a Storyboard file and single UIViewController. Use this basic template as a starting point for any iOS app." },
				new ItemViewModel { Id = "master-detail", Name = "Master-Detail App", Icon = icon, Description = "An iOS app that uses a split view and the master-detail navigation pattern. Use this if your app will display a list of data that will show more detail when an item is selected." },
				new ItemViewModel { Id = "tabbed", Name = "Tabbed App", Icon = icon, Description = "An iOS app that uses tab icons at the bottom of the screen for navigation. Use this if your app will have different content categories that will be frequently switched between." },
				new ItemViewModel { Id = "blank", Name = "Blank App", Icon = icon, Description = "An iOS app with an empty UIViewController and no Storyboard file." }
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

