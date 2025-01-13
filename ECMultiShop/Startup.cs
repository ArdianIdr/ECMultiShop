using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ECMultiShop.Startup))]
namespace ECMultiShop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
