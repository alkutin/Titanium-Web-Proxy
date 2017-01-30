using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EndPointProxy;

namespace Titanium.Web.Proxy.Test
{
    class AppSettingsConfigurationManager : IConfigurationManager
    {
        public string Url { get { return ConfigurationManager.AppSettings["ApiUrl"]; } }

        public string Key { get { return ConfigurationManager.AppSettings["Key"]; } }
        public string Vector { get { return ConfigurationManager.AppSettings["Vector"]; } }
    }
}
