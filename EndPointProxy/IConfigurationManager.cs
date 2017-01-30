using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndPointProxy
{
    public interface IConfigurationManager
    {
        string Url { get; }
        string Key { get; }
        string Vector { get; }
    }

    public static class ConfigurationSingleton
    {
        public static IConfigurationManager Instance { get; set; }
    }
}
