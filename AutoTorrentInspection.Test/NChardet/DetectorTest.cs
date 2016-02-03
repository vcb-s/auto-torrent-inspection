using Microsoft.VisualStudio.TestTools.UnitTesting;
using NChardet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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