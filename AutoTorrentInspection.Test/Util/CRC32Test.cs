using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoTorrentInspection.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTorrentInspection.Util.Tests
{
    [TestClass()]
    public class CRC32Test
    {
        [TestMethod()]
        public void FileCRCTest()
        {
            string path = @"E:\AVS\embrace\VTS_01_1 [B2708D30].VOB";
            Console.WriteLine(CRC32.FileCRC(path));
            uint crc;
            CRC32.FindCRC(path,out crc);
            Console.WriteLine(crc);
        }
    }
}