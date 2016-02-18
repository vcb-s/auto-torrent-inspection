﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            foreach (var item in Directory.GetFiles(@"..\..\[Encode Sample]"))
            {
                Debug.WriteLine($"{Path.GetFileName(item)}: ");
                Debug.Write(EncodingDetector.GetEncodingU(item, out confindence));
                Debug.WriteLine(@"-----------------------------");
            }
        }

        [TestMethod()]
        public void EncodeTestU()
        {
            float confindence;
            foreach (var item in Directory.GetFiles(@"..\..\[Encoding All Star]"))
            {
                Debug.WriteLine($"{Path.GetFileName(item)}: ");
                EncodingDetector.GetEncodingU(item, out confindence);
            }
        }
    }
}