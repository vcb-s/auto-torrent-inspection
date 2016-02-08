using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            foreach (var item in Directory.GetFiles(@"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Encode Sample]"))
            {
                Console.WriteLine($"{Path.GetFileName(item)}: ");
                Console.WriteLine(EncodingDetector.GetEncoding(item));
                Console.WriteLine(@"-----------------------------");
            }
        }
    }
}