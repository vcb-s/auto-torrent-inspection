using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using AutoTorrentInspection.Util;

namespace NChardet.Tests
{
    [TestClass()]
    public class DetectorTest
    {
        [TestMethod()]
        public void EncodeTest()
        {
            foreach (var item in Directory.GetFiles(@"..\..\[Encode Sample]"))
            {
                Debug.WriteLine($"{Path.GetFileName(item)}: ");
                //Console.WriteLine(EncodingDetector.GetEncoding(item));
                Debug.Write(EncodingDetector.GetEncodingU(item));
                Debug.WriteLine(@"-----------------------------");
            }
        }

        [TestMethod()]
        public void EncodeTestU()
        {
            foreach (var item in Directory.GetFiles(@"..\..\[Encoding All Star]"))
            {
                Debug.WriteLine($"{Path.GetFileName(item)}: ");
                EncodingDetector.GetEncodingU(item);
            }
        }
    }
}