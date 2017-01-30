using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using EndPointProxy;

namespace AndroidPoxyApp
{
    class AndroidConfigurationManager : IConfigurationManager
    {
        public string Url { get { return string.Empty; } }
        public string Key { get { return "qawsedrftgyhujik"; } }
        public string Vector { get { return "1!2@3#4$5%6^7?8*"; } }
    }
}