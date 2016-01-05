using System.IO;
using System.Linq;
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
            var utfCount = Directory.GetFiles( @"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Encode Sample]")
                    .Sum(textFile => ConvertMethod.IsUTF8(textFile) ? 1 : 0);
            Assert.IsTrue(utfCount == 3);
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
    }
}
