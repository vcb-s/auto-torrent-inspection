using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoTorrentInspection.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTorrentInspection.Util.Tests
{
    [TestClass()]
    public class AssemblyCueTest
    {
        [TestMethod()]
        public void GetCueTest()
        {
            const string takPath  = @"D:\Video\[VCB-S&philosophy-raws][K-ON! The Movie]\CDs\[EAC][111207] 映画「けいおん!」OP「Unmei♪wa♪Endless！」／放課後ティータイム\Unmei♪wa♪Endless！.tak";
            const string flacPath = @"D:\Code\@AutoTorrentInspection\GNCA-0330.Flac";
            Console.WriteLine(AssemblyCue.GetCueFromTak(takPath));
            Console.WriteLine(AssemblyCue.GetCueFromFlac(flacPath));
        }
    }
}