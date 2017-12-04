using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using Crc32C;

namespace AutoTorrentInspection.Util.Tests
{
    [TestClass()]
    public class CRC32Test
    {
        [TestMethod()]
        public void FileCRCTest()
        {
            const string path = @"..\..\[CRC Sample]\VIDEO_TS [57FD7F1E].IFO";
            var calCRC = Crc32.NET.Crc32Algorithm.FileCRC(path).Result;
            CRC32.FindCRC(path, out var fileCRC);
            Console.WriteLine($@"{fileCRC} {calCRC}");
            Assert.IsTrue(calCRC == fileCRC);
        }

        [TestMethod()]
        public void CRC32PerformanceTest()
        {
            var data = new byte[65536];
            var random = new Random();
            random.NextBytes(data);

            /*Crc32.Net*/
            long total = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(3))
            {
                Crc32.NET.Crc32Algorithm.Compute(data, 0, data.Length);
                total += data.Length;
            }

            stopwatch.Stop();
            Console.WriteLine($@"Crc32.Net Throughput: {total / stopwatch.Elapsed.TotalSeconds / 1024 / 1024:0.0}MB/s");
            /*Crc32C.Net*/
            total = 0;
            stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(3))
            {
                Crc32CAlgorithm.Compute(data, 0, data.Length);
                total += data.Length;
            }

            stopwatch.Stop();
            Console.WriteLine($@"Crc32C.Net Throughput: {total / stopwatch.Elapsed.TotalSeconds / 1024 / 1024:0.0}MB/s");
        }
    }
}
