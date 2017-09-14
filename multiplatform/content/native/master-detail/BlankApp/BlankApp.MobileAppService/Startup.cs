using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(BlankApp.MobileAppService.Startup))]

namespace BlankApp.MobileAppService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}