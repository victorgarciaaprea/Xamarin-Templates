using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using BlankApp.Models;
using BlankApp.ViewModels;

namespace BlankApp.UWP.Views
{
	public sealed partial class AddItems : Page
	{
		ItemsViewModel ViewModel;
		public AddItems()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel = (ItemsViewModel)e.Parameter;
			DataContext = ViewModel;
		}

		private void SaveItem_Click(object sender, RoutedEventArgs e)
		{
			var item = new Item
			{
				Text = txtText.Text,
				Description = txtDesc.Text
			};
			ViewModel.AddItemCommand.Execute(item);

			this.Frame.GoBack();
		}
	}
}