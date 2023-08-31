using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using AutoTorrentInspection.Util;

namespace Ude.Tests
{
    [TestClass()]
    public class DetectorTest
    {
        [TestMethod()]
        public void EncodeTest()
        {
            float confindence;
            foreach (var item in Directory.GetFiles(@"..\..\..\..\[Encode Sample]"))
            {
                Console.WriteLine($"{Path.GetFileName(item)}: {EncodingDetector.GetEncoding(item, out confindence)} ({confindence:F3})");
            }
            foreach (var item in Directory.GetFiles(@"..\..\..\..\[Encoding All Star]"))
            {
                Console.WriteLine($"{Path.GetFileName(item)}: {EncodingDetector.GetEncoding(item, out confindence)} ({confindence:F3})");
            }
        }
    }
}