using Xamarin.UITest;

using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace NewApp.UITests
{
    public class ItemsPage : BasePage
    {
        readonly Query addToolbarButton;

        public ItemsPage(IApp app, Platform platform) : base(app, platform, "Browse")
        {
            if (OniOS)
                addToolbarButton = x => x.Class("UIButtonLabel").Index(0);
            else
                addToolbarButton = x => x.Class("android.support.v7.view.menu.ActionMenuItemView").Index(0);
        }

        public void TapAddToolbarButton()
        {
            app.Tap(addToolbarButton);

            app.Screenshot("Toolbar Item Tapped");
        }
    }
}