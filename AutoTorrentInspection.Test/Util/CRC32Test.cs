using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoTorrentInspection.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AutoTorrentInspection.Util.Crc32.NET;

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
            Console.WriteLine($@"{fileCRC} {calCRC}");
            Assert.IsTrue(calCRC == fileCRC);
        }

        [TestMethod()]
        public void CRC32PerformanceTest()
        {
            var data = new byte[65536];
            var random = new Random();
            random.NextBytes(data);
            long total = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(3))
            {
                CRC32.GetCRC(data);
                total += data.Length;
            }

            stopwatch.Stop();
            Console.WriteLine($@"CRC32 Throughput: {total/stopwatch.Elapsed.TotalSeconds/1024/1024:0.0}MB/s");

            total = 0;
            stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(3))
            {
                Crc32Algorithm.Compute(data, 0, data.Length);
                total += data.Length;
            }

            stopwatch.Stop();
            Console.WriteLine($@"Crc32Net Throughput: {total / stopwatch.Elapsed.TotalSeconds / 1024 / 1024:0.0}MB/s");
        }
    }
}
