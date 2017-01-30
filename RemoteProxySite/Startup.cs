using System;
using System.Collections.Generic;
using System.Linq;
using EndPointProxy;
using Microsoft.Owin;
using Owin;
using RemoteProxySite.App_Start;

[assembly: OwinStartup(typeof(RemoteProxySite.Startup))]

namespace RemoteProxySite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigurationSingleton.Instance = new WebConfigurationManager();
            ConfigureAuth(app);
        }
    }
}
