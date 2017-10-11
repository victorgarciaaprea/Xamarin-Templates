using System;

using BlankApp.Views;
#if (CreateBackendProject)
using BlankApp.Services;
#endif
using Xamarin.Forms;

namespace BlankApp
{
	public partial class App : Application
	{
		#if (CreateBackendProject)
		public static bool UseMockDataStore = true;
		public static string AzureMobileAppUrl = "https://[CONFIGURE-THIS-URL].azurewebsites.net";
		#endif		

		public App ()
		{
			InitializeComponent();

			#if (CreateBackendProject)
			if (UseMockDataStore)
				DependencyService.Register<MockDataStore>();
			else
				DependencyService.Register<AzureDataStore>();
			#endif

            MainPage = new MainPage();
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
