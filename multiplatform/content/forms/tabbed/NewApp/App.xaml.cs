using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
#if (CreateBackendProject)
using NewApp.Services;
#endif
using NewApp.Views;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace NewApp
{
    public partial class App : Application
    {
        #if (CreateBackendProject)
        //TODO: Replace with *.azurewebsites.net url after deploying backend to Azure
                public static string AzureBackendUrl = "http://localhost:5000";
                public static bool UseMockDataStore = true;
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
