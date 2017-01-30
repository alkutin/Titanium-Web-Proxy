using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using EndPointProxy;

namespace RemoteProxySite.App_Start
{
    public class WebConfigurationManager : IConfigurationManager
    {
        public string Url { get { return ConfigurationManager.AppSettings["ApiUrl"]; } }

        public string Key { get { return ConfigurationManager.AppSettings["Key"]; } }
        public string Vector { get { return ConfigurationManager.AppSettings["Vector"]; } }
    }
}