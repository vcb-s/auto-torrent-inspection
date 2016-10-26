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
            const string path = @"..\..\[CRC Sample]\VIDEO_TS [57FD7F1E].IFO";
            uint calCRC = CRC32.FileCRC(path).Result;
            uint fileCRC;
            CRC32.FindCRC(path, out fileCRC);
            Console.WriteLine($"{fileCRC} {calCRC}");
            Assert.IsTrue(calCRC == fileCRC);
        }
    }
}