using Xamarin.Forms;

namespace BlankApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new BlankAppPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
