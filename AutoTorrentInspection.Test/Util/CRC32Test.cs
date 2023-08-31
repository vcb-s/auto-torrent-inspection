using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Force.Crc32;
using System.IO;

namespace AutoTorrentInspection.Util.Tests
{
    [TestClass()]
    public class CRC32Test
    {
        [TestMethod()]
        public void FileCRCTest()
        {
            const string path = @"..\..\..\..\[CRC Sample]\VIDEO_TS [57FD7F1E].IFO";
            var hash = new Crc32Algorithm();
            using (var file = File.OpenRead(path))
            {
                var crcByte = hash.ComputeHash(file);
                var calCRC = (uint)crcByte[0] << 24 | (uint)crcByte[1] << 16 | (uint)crcByte[2] << 8 | crcByte[3];
                CRC32.FindCRC(path, out var fileCRC);
                Console.WriteLine($@"{fileCRC} {calCRC}");
                Assert.IsTrue(calCRC == fileCRC);
            }
        }
    }
}
