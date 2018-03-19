using Merq;
using Microsoft.VisualStudio.ComponentModelHost;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Xamarin.Templates.Properties;
using Xamarin.VisualStudio.Contracts.Commands.Android;
using Xamarin.VisualStudio.Contracts.Model.Android;

namespace Xamarin.Templates.Wizard
{
    public class AndroidViewModel : IViewModel, INotifyPropertyChanged
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
				new ItemViewModel { Id = "single-view", Name = Resources.AndroidViewModel_SingleViewApp_Name, Icon = icon, Description = Resources.AndroidViewModel_SingleViewApp_Description },
				new ItemViewModel { Id = "nav-drawer", Name = Resources.AndroidViewModel_NavigationDrawerApp_Name, Icon = icon, Description = Resources.AndroidViewModel_NavigationDrawerApp_Description },
				new ItemViewModel { Id = "bottom-nav", Name = Resources.AndroidViewModel_TabbedApp_Name, Icon = icon, Description = Resources.AndroidViewModel_TabbedApp_Description },
				new ItemViewModel { Id = "blank", Name = Resources.AndroidViewModel_BlankApp_Name, Icon = icon, Description = Resources.AndroidViewModel_BlankApp_Description }
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
