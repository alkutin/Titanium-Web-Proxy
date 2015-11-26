using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyLanguage.Models
{
    public class HttpHeader
    {
        public HttpHeader()
        {
            
        }

        public HttpHeader(string name, string value)
        {
            if (string.IsNullOrEmpty(name)) throw new Exception("Name cannot be null");

            Name = name.Trim();
            Value = value.Trim();
        }

        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, Value);
        }
    }

    public static class HeaderHelper
    {
        public static void SetHeader(this List<HttpHeader> headers, string name, string value)
        {
            var item = headers.FirstOrDefault(w => w.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (item == null)
            {
                item = new HttpHeader(name, value);
                headers.Add(item);
            }

            item.Name = name;
            item.Value = value;
        }

        public static string GetHeader(this List<HttpHeader> headers, string name)
        {
            var item = headers.FirstOrDefault(w => w.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            return item == null ? string.Empty : item.Value;
        }
    }
}