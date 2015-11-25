using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Encoding
{
    public static class MD5Helper
    {
        public static string GetMD5(this byte[] source)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(source);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
