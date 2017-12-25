using System;
using System.Linq;
using System.Text;
using AutoTorrentInspection.Objects;
using AutoTorrentInspection.Util;
using BencodeNET.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoTorrentInspection.Test.Util
{
    [TestClass()]
    public class TorrentDataTest
    {
        private TorrentData _torrent;

        private void PrintTorrentInfo()
        {
            Console.WriteLine($@"AnnounceURL: {_torrent.GetAnnounceList().First()}");
            Console.WriteLine($@"Comment: {_torrent.Comment}");
            Console.WriteLine($@"CreatedBy: {_torrent.CreatedBy}");
            Console.WriteLine($@"CreationDate: {_torrent.CreationDate}");
            Console.WriteLine($@"IsPrivate: {_torrent.IsPrivate}");
            Console.WriteLine($@"Source: {_torrent.Source}");
            Console.WriteLine($@"TorrentName: {_torrent.TorrentName}");
        }

        [TestMethod()]
        public void TestLoadTorrent1()
        {
            const string torrentPath = @"..\..\[Torrent Sample]\Comment.torrent";
            _torrent = new TorrentData(torrentPath);
            PrintTorrentInfo();
            Assert.IsTrue(_torrent.GetAnnounceList().First() == "http://tracker.dmhy.org/announce?secure=securecode");
            Assert.IsTrue(_torrent.Comment == "Ripped And Scanned By imi415@U2");
            Assert.IsTrue(_torrent.CreatedBy == "uTorrent/3.4.2");
            Assert.IsTrue(_torrent.CreationDate == (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(1415247690)).Add(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)));
            Assert.IsTrue(_torrent.IsPrivate);
            Assert.IsTrue(_torrent.Source == "[u2.dmhy.org] U2分享園@動漫花園");
            Assert.IsTrue(_torrent.TorrentName == "南條愛乃 - あなたの愛した世界");
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Count == 2);
            Assert.IsTrue(fileList["root"].Count == 4);
            Assert.IsTrue(fileList["スキャン"].Count == 12);
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.State == FileState.InValidFile)) == 14);
        }

        [TestMethod()]
        public void TestLoadTorrent2()
        {
            const string torrentPath = @"..\..\[Torrent Sample]\SingleFile.torrent";
            _torrent = new TorrentData(torrentPath);
            PrintTorrentInfo();
            Assert.IsTrue(_torrent.GetAnnounceList().First() == "http://tracker.dmhy.org/announce?secure=securecode");
            Assert.IsTrue(string.IsNullOrEmpty(_torrent.Comment));
            Assert.IsTrue(_torrent.CreatedBy == "uTorrent/3220");
            Assert.IsTrue(_torrent.CreationDate == (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(1414723640)).Add(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)));
            Assert.IsTrue(_torrent.IsPrivate);
            Assert.IsTrue(_torrent.Source == "[u2.dmhy.org] U2分享園@動漫花園");
            Assert.IsTrue(_torrent.TorrentName == "[FLsnow][Tamako_love_story][MOVIE][外挂结构].rar");
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Count == 1);
            Assert.IsTrue(fileList["single"].Count == 1);
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.State == FileState.InValidFile)) == 0);
        }

        [TestMethod()]
        public void TestLoadTorrent3()
        {
            const string torrentPath = @"..\..\[Torrent Sample]\Martian.torrent";
            _torrent = new TorrentData(torrentPath);
            PrintTorrentInfo();
            Assert.IsTrue(_torrent.GetAnnounceList().First() == "http://tracker.hdtime.org/announce.php?passkey=passkey");
            Assert.IsTrue(string.IsNullOrEmpty(_torrent.Comment));
            Assert.IsTrue(_torrent.CreatedBy == null);
            Assert.IsTrue(_torrent.CreationDate == new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now)));
            Assert.IsFalse(_torrent.IsPrivate);
            Assert.IsTrue(_torrent.Source == "[hdtime.org] HDTIME");
            Assert.IsTrue(_torrent.TorrentName == "The Martian 2015 HD-VOD HC x264 AC3-CPG");
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Count == 1);
            Assert.IsTrue(fileList["root"].Count == 2);
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.State == FileState.InValidFile)) == 2);
        }

        [TestMethod()]
        public void TestLoadTorrent4()
        {
            const string torrentPath = @"..\..\[Torrent Sample]\USO.torrent";
            _torrent = new TorrentData(torrentPath);
            PrintTorrentInfo();
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.State == FileState.InValidFile)) == 33);
        }

        [TestMethod()]
        public void TestLoadTorrent5()
        {
            const string torrentPath = @"..\..\[Torrent Sample]\FZ.torrent";
            _torrent = new TorrentData(torrentPath);
            PrintTorrentInfo();
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList.Keys.Sum(category => fileList[category].Count(item => item.State == FileState.InValidFile)) == 1);
        }

        [TestMethod()]
        public void TestLoadTorrent6()
        {
            const string torrentPath = @"..\..\[Torrent Sample]\Padding_file.torrent";
            _torrent = new TorrentData(torrentPath);
            PrintTorrentInfo();
            var fileList = _torrent.GetFileList();
            Assert.IsTrue(fileList["root"].Count == 12);
        }

        private static string ToUTF8(string str ,Encoding from, Encoding to)
        {
            var bytSrc = from.GetBytes(str);
            var bytDestination = Encoding.Convert(from, to, bytSrc);
            var strTo = to.GetString(bytDestination);
            var tmp = new BString(str, from).ToString();
            return strTo;
        }

        [TestMethod()]
        public void TestNonUTF8Encode()
        {
            const string torrentPath = @"..\..\[Torrent Sample]\GBK.torrent";
            _torrent = new TorrentData(torrentPath);
            Assert.AreEqual("[2DJGAME] [2010.03.10] 映画「時をかける少女」主題歌「ノスタルシ゛ア」&挿入歌「時をかける少女」(320k+cover).rar",
                _torrent.TorrentName);
        }
    }
}
