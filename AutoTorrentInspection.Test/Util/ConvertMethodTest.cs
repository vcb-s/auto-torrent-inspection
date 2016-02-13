using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
            foreach (var item in Directory.GetFiles(@"..\..\[Encode Sample]"))
            {
                string encode = EncodingDetector.GetEncoding(item);
                Console.WriteLine($"{Path.GetFileName(item)}: {encode == "UTF-8"}");
            }
        }

        [TestMethod()]
        public void GetFileListTest()
        {
            var result = ConvertMethod.GetFileList(@"..\..\[Folder Sample]");
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
            foreach (var folder in ConvertMethod.GetFileList(@"..\..\[Match Sample]").Values)
            {
                cueFiles.AddRange(folder.Where(file => file.Extension.ToLower() == ".cue"));
            }

            foreach (var cue in cueFiles)
            {
                Console.WriteLine(cue);
                Assert.IsTrue(CueCurer.CueMatchCheck(cue));
            }
        }
    }
}
