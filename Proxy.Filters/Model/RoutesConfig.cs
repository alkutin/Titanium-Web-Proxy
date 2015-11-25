using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Filters.Model
{
    public class RoutesConfig
    {
        public string[] Forbiden;
        public Dictionary<string, string[]> Proxy;
    }
}
