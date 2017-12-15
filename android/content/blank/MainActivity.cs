using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;

namespace NewApp
{
    [Activity(Label = "Blank", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
        }
    }
}

