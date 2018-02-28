using Merq;

using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Xamarin.VisualStudio.Contracts.Commands.Android;
using Xamarin.VisualStudio.Contracts.Model.Android;

namespace Xamarin.Templates.Wizard
{
	public class AndroidViewModel : INotifyPropertyChanged
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

		public AndroidViewModel()
		{
			AndroidFrameworks = GetFrameworks();
			AndroidFramework = AndroidFrameworks.First(f => f.ApiLevel == 21);

			Templates = CreateTemplatesContext();
		}

		public List<ItemViewModel> CreateTemplatesContext()
		{
			var icon = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), @"FormsProject.jpg");
			return new List<ItemViewModel>
			{
				new ItemViewModel { Id = "single-view", Name = "Single View App", Icon = icon, Description = "An Android app with a single Activity and simple AXML layout file. Use this basic template as a starting point for any Android app." },
				new ItemViewModel { Id = "nav-drawer", Name = "Navigation Drawer App", Icon = icon, Description = "An Android app that uses a panel on the left side to present navigation options. Use this as an alternative to tabs if you have lots of navigation targets and want to maximize screen space." },
				new ItemViewModel { Id = "bottom-nav", Name = "Tabbed App", Icon = icon, Description = "An Android app that uses tab icons at the bottom of the screen for navigation. Use this if your app will have few navigation targets that will be frequently switched between." },
				new ItemViewModel { Id = "blank", Name = "Blank App", Icon = icon, Description = "An Android app with an Activity class and empty layout file." }
			};
		}

		static IList<AndroidFramework> GetFrameworks()
		{
			try
			{
				var componentModel = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
				var commandBus = componentModel?.GetService<ICommandBus>();
				var versions = commandBus?.Execute(new GetSdkInfo());
				return versions?.Frameworks.OrderByDescending(f => f.ApiLevel).ToList();
			}
			catch (FileNotFoundException)//this is to avoid a known watson crash
			{
				return new List<AndroidFramework>();
			}
		}

		public IList<AndroidFramework> AndroidFrameworks { get; set; }

		public AndroidFramework AndroidFramework { get; set; }

		public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
	}

}
