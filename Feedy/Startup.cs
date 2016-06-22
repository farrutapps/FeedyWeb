using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Feedy.Startup))]
namespace Feedy
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
