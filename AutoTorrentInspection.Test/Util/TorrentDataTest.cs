using System;
using System.Linq;
using AutoTorrentInspection.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoTorrentInspection.Test.Util
{
    [TestClass()]
    public class TorrentDataTest
    {
        private TorrentData _torrent;

        [TestMethod()]
        public void TestLoadTorrent1()
        {
            const string torrentPath = @"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Torrent Sample]\Comment.torrent";
            _torrent = new TorrentData(torrentPath);
            Assert.IsTrue(_torrent.GetAnnounceList().First() == "http://tracker.dmhy.org/announce?secure=securecode");
            Assert.IsTrue(_torrent.Comment == "Ripped And Scanned By imi415@U2");
            Assert.IsTrue(_torrent.CreatedBy == "uTorrent/3.4.2");
            Assert.IsTrue(_torrent.CreationDate == new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(1415247690));
            Assert.IsTrue(_torrent.IsPrivate);
            Assert.IsTrue(_torrent.Source == "[u2.dmhy.org] U2分享園@動漫花園");
            Assert.IsTrue(_torrent.TorrentName == "南條愛乃 - あなたの愛した世界");
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Count == 2);
            Assert.IsTrue(fileList["root"].Count == 4);
            Assert.IsTrue(fileList["スキャン"].Count == 12);
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.InValidFile)) == 14);
        }

        [TestMethod()]
        public void TestLoadTorrent2()
        {
            const string torrentPath = @"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Torrent Sample]\SingleFile.torrent";
            _torrent = new TorrentData(torrentPath);
            Assert.IsTrue(_torrent.GetAnnounceList().First() == "http://tracker.dmhy.org/announce?secure=securecode");
            Assert.IsTrue(_torrent.Comment == "");
            Assert.IsTrue(_torrent.CreatedBy == "uTorrent/3220");
            Assert.IsTrue(_torrent.CreationDate == new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(1414723640));
            Assert.IsTrue(_torrent.IsPrivate);
            Assert.IsTrue(_torrent.Source == "[u2.dmhy.org] U2分享園@動漫花園");
            Assert.IsTrue(_torrent.TorrentName == "[FLsnow][Tamako_love_story][MOVIE][外挂结构].rar");
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Count == 1);
            Assert.IsTrue(fileList["single"].Count == 1);
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.InValidFile)) == 0);
        }

        [TestMethod()]
        public void TestLoadTorrent3()
        {
            const string torrentPath = @"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Torrent Sample]\Martian.torrent";
            _torrent = new TorrentData(torrentPath);
            Assert.IsTrue(_torrent.GetAnnounceList().First() == "http://tracker.hdtime.org/announce.php?passkey=passkey");
            Assert.IsTrue(_torrent.Comment == "");
            Assert.IsTrue(_torrent.CreatedBy == null);
            Assert.IsTrue(_torrent.CreationDate == new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(0));
            Assert.IsTrue(_torrent.IsPrivate);
            Assert.IsTrue(_torrent.Source == "[hdtime.org] HDTIME");
            Assert.IsTrue(_torrent.TorrentName == "The Martian 2015 HD-VOD HC x264 AC3-CPG");
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Count == 1);
            Assert.IsTrue(fileList["root"].Count == 2);
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.InValidFile)) == 2);
        }

        [TestMethod()]
        public void TestLoadTorrent4()
        {
            const string torrentPath = @"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Torrent Sample]\USO.torrent";
            _torrent = new TorrentData(torrentPath);
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.InValidFile)) == 33);
        }

        [TestMethod()]
        public void TestLoadTorrent5()
        {
            const string torrentPath = @"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Torrent Sample]\FZ.torrent";
            _torrent = new TorrentData(torrentPath);
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.InValidFile)) == 1);
        }

        [TestMethod()]
        public void TestLoadTorrent6()
        {
            const string torrentPath = @"C:\Users\TautCony\Documents\auto-torrent-inspection\AutoTorrentInspection.Test\[Torrent Sample]\Padding_file.torrent";
            _torrent = new TorrentData(torrentPath);
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList["root"].Count == 12);
        }
    }
}
