using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proxy.Encoding;

namespace Tests.ProxyServices
{
    [TestClass]
    public class EncoderTests
    {
        [TestMethod]
        public void TestEncrypt()
        {
            var data = new EncoderTestObject
            {
                Item1 = Guid.NewGuid(),
                Item2 = Guid.NewGuid().ToString(),
                Item3 = new Random().NextDouble()
            };
            var key = System.Text.Encoding.ASCII.GetBytes(Guid.NewGuid().ToString("N").Substring(0, 16).PadRight(Encoder.DefaultBlockSize / 8));
            var iv = System.Text.Encoding.ASCII.GetBytes(Guid.NewGuid().ToString("N").Substring(0, 16).PadRight(Encoder.DefaultBlockSize / 8));
            var encodedData = new Encoder(key, iv).Encode(data);
            var decodedData = new Encoder(key, iv).Decode<EncoderTestObject>(encodedData);

            Assert.AreEqual(data.Item1, decodedData.Item1);
            Assert.AreEqual(data.Item2, decodedData.Item2);
            Assert.AreEqual(data.Item3, decodedData.Item3);
        }
    }
}
