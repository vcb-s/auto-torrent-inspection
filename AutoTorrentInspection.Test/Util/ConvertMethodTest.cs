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
            Directory.GetFiles( @"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Encode Sample]") .ToList()
                .ForEach(item => Console.WriteLine($"{Path.GetFileName(item)}: {ConvertMethod.IsUTF8(item)}"));
        }

        [TestMethod()]
        public void GetFileListTest()
        {
            var result = ConvertMethod.GetFileList(@"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Folder Sample]");
            Assert.IsTrue(result.Count              == 3);
            Assert.IsTrue(result["root"].Count      == 1);
            Assert.IsTrue(result["[folder1]"].Count == 3);
            Assert.IsTrue(result["[folder2]"].Count == 3);
        }

        [TestMethod()]
        public void CueMatchCheckTest()
        {
            var result = ConvertMethod.GetFileList(@"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Match Sample]");
            var cueFiles = new List<FileDescription>();
            foreach (var file in result.Values)
            {
                cueFiles.AddRange(file.Where(item => item.Extension.ToLower() == ".cue"));
            }
            foreach (var cue in cueFiles)
            {
                Console.WriteLine(cue);
                Assert.IsTrue(ConvertMethod.CueMatchCheck(cue));
            }
        }
    }
}
