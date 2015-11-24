using System;
using System.Configuration;
using System.Runtime.InteropServices;

namespace Titanium.Web.Proxy.Test
{
    public class Program
    {
        private static readonly ProxyTestController Controller = new ProxyTestController();

        public static void Main(string[] args)
        {
            //On Console exit make sure we also exit the proxy
            NativeMethods.Handler = ConsoleEventCallback;
            NativeMethods.SetConsoleCtrlHandler(NativeMethods.Handler, true);
            
            Controller.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["MonitorHTTPS"]);
            Console.WriteLine("Monitor HTTPS: {0}", Controller.EnableSsl);

            Controller.SetAsSystemProxy = bool.Parse(ConfigurationManager.AppSettings["SystemProxy"]);
            Console.WriteLine("Set this as a System Proxy: {0}", Controller.SetAsSystemProxy);

            //Start proxy controller
            Controller.StartProxy();

            Console.WriteLine("Hit any key to exit..");
            Console.WriteLine();
            Console.Read();

            Controller.Stop();
        }


        private static bool ConsoleEventCallback(int eventType)
        {
            if (eventType != 2) return false;
            try
            {
                Controller.Stop();
            }
            catch
            {
                // ignored
            }
            return false;
        }
    }

    internal static class NativeMethods
    {
        // Keeps it from getting garbage collected
        internal static ConsoleEventDelegate Handler;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        // Pinvoke
        internal delegate bool ConsoleEventDelegate(int eventType);
    }
}