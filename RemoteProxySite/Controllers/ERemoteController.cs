using Proxy.Encoding;
using RemoteProxySite.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using RemoteProxySite.Handlers;

namespace RemoteProxySite.Controllers
{        
    public class ERemoteController : Controller
    {        
        
        public ERemoteController()
        {
            
        }

        public ActionResult Index()
        {
            return View(ERemote.GetSessions());
        }
    }
}
