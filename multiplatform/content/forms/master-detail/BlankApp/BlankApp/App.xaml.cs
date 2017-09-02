using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

using BlankApp.View;

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

            if (Device.RuntimePlatform == Device.iOS)
                MainPage = new MainPage();
            else
                MainPage = new NavigationPage(new MainPage());
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
