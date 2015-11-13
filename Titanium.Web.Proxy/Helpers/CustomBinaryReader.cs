using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Titanium.Web.Proxy.Helpers
{
    internal class CustomBinaryReader : BinaryReader
    {
        internal CustomBinaryReader(Stream stream, Encoding encoding)
            : base(stream, encoding)
        {
        }

        internal string ReadLine()
        {
            var buf = new char[1];
            var readBuffer = new StringBuilder();
            try
            {
                var lastChar = new char();

                while ((Read(buf, 0, 1)) > 0)
                {
                    if (lastChar == '\r' && buf[0] == '\n')
                    {
                        var rnRez = readBuffer.Remove(readBuffer.Length - 1, 1).ToString();
                        Debug.WriteLine("Read: " + rnRez);
                        return rnRez;
                    }
                    if (buf[0] == '\0')
                    {
                        var zRez = readBuffer.ToString();
                        Debug.WriteLine("Read: " + zRez);
                        return zRez;
                    }
                    readBuffer.Append(buf[0]);

                    lastChar = buf[0];
                }
                var rez = readBuffer.ToString();
                Debug.WriteLine("Read: " + rez);
                return rez;
            }
            catch (IOException)
            {
                var eRez = readBuffer.ToString();
                Debug.WriteLine("Read: " + eRez);
                return eRez;
            }
        }


        internal List<string> ReadAllLines()
        {
            string tmpLine;
            var requestLines = new List<string>();
            while (!string.IsNullOrEmpty(tmpLine = ReadLine()))
            {
                requestLines.Add(tmpLine);
            }
            return requestLines;
        }
    }
}