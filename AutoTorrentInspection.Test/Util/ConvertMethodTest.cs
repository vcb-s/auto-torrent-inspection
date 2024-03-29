﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AutoTorrentInspection.Objects;
using AutoTorrentInspection.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoTorrentInspection.Test.Util
{
    [TestClass()]
    public class ConvertMethodTest
    {
        [TestMethod()]
        public void IsUTF8Test()
        {
            foreach (var item in Directory.GetFiles(@"..\..\..\..\[Encode Sample]"))
            {
                float confindence;
                string encode = EncodingDetector.GetEncoding(item, out confindence);
                Console.WriteLine($"{Path.GetFileName(item)}: {encode == "UTF-8"} confidence: {confindence:F3}");
            }
        }

        [TestMethod()]
        public void GetFileListTest()
        {
            var result = ConvertMethod.GetFileList(@"..\..\..\..\[Folder Sample]");
            result.Values.ToList().ForEach(category => category.ForEach(Console.WriteLine));
            Assert.IsTrue(result.Count              == 3);
            Assert.IsTrue(result["root"].Count      == 1);
            Assert.IsTrue(result["[folder1]"].Count == 3);
            Assert.IsTrue(result["[folder2]"].Count == 3);
        }

        [TestMethod()]
        public void CueMatchCheckTest()
        {
            var cueFiles = new List<FileDescription>();
            foreach (var folder in ConvertMethod.GetFileList(@"..\..\..\..\[Match Sample]").Values)
            {
                cueFiles.AddRange(folder.Where(file => file.Extension.ToLower() == ".cue"));
            }

            foreach (var cue in cueFiles)
            {
                Console.WriteLine(cue);
                Assert.IsTrue(CueCurer.CueMatchCheck(cue));
            }
        }

        [TestMethod()]
        public void GetDiffTest()
        {
            const string ghost1 = @"..\..\..\..\[Torrent Sample]\Ghost in the Shell：STAND ALONE COMPLEX.v1.torrent";
            const string ghost2 = @"..\..\..\..\[Torrent Sample]\Ghost in the Shell：STAND ALONE COMPLEX.v2.torrent";
            var ret = ConvertMethod.GetDiffNode(new TorrentData(ghost1), new TorrentData(ghost2));

            Console.WriteLine(@"in a not in b:");
            Console.WriteLine(ret.inANotInB.Json);
            Console.WriteLine(@"in b not in a:");
            Console.WriteLine(ret.inBNotInA.Json);
        }

        [TestMethod()]
        public void GetRawFolderFileListWithAttributeTest()
        {
            var ret = ConvertMethod.GetRawFolderFileListWithAttribute(@"..\..\..\");
            Node node1 = new Node(ret);
            Console.WriteLine(node1.Json);
        }
    }
}
