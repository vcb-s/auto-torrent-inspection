using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    public class FileDescription
    {
        private string FileName     {         set; get; }
        private string ReletivePath {         set; get; }
        public string FullPath      { private set; get; }
        public string Extension     { private set; get; }
        private long Length         {         set; get; }
        public bool InValidFile     { private set; get; }
        public bool InValidEncode   { private set; get; }
        public bool InValidCue      { private set; get; }
        public string Encode        {         set; get; }

        public override string ToString() => $"{FileName}, length: {(double)Length / 1024:F3}KB";

        private static readonly Regex AnimePartten  = new Regex(@"^\[[^\[\]]*VCB-S(?:tudio)*[^\[\]]*\] [^\[\]]+ (\[.*\d*\])*\[((?<Ma>Ma10p_1080p)|(?<Hi>(Hi10p|Hi444pp)_(1080|720|480)p)|(?<EIGHT>(1080|720)p))\]\[((?<HEVC-Ma>x265)|(?<AVC-Hi>x264)|(?(EIGHT)x264))_\d*(flac|aac|ac3)\](?<SUB>(\.(sc|tc)|\.(chs|cht))*)\.((?(AVC)(mkv|mka|flac))|(?(HEVC)(mkv|mka|flac)|(?(EIGHT)mp4))|(?(SUB)ass))$");
        private static readonly Regex MusicPartten  = new Regex(@"\.(flac|tak|m4a|cue|log|jpg|jpeg|jp2)");
        private static readonly Regex ExceptPartten = new Regex(@"\.(rar|7z|zip)");

        public static FileDescription CreateWithCheckTorrent(string fileName, string reletivePath, long length)
        {
            var temp = new FileDescription
            {
                FileName     = fileName,
                ReletivePath = reletivePath,
                Extension    = Path.GetExtension(fileName)?.ToLower(),
                Length       = length
            };
            temp.CheckValidTorrent();
            return temp;
        }

        public static FileDescription CreateWithCheckFile(string fileName, string reletivePath, string fullPath)
        {
            var temp = new FileDescription
            {
                FileName     = fileName,
                ReletivePath = reletivePath,
                FullPath     = fullPath,
                Extension    = Path.GetExtension(fileName)?.ToLower(),
                //Length       = fullPath.Length > 256 ? -1024*1024L : new FileInfo(fullPath).Length
                Length       = ConvertMethod.GetFile(fullPath).Length
            };
            temp.CheckValidFile();
            return temp;
        }

        private void CheckValidTorrent()
        {
            InValidFile = !ExceptPartten.IsMatch(Extension) &&
                          !MusicPartten.IsMatch(FileName.ToLower()) &&
                          !AnimePartten.IsMatch(FileName);
        }

        private void CheckValidFile()
        {
            InValidFile = !ExceptPartten.IsMatch(Extension) &&
                          !MusicPartten.IsMatch(FileName.ToLower()) &&
                          !AnimePartten.IsMatch(FileName);
            if (Extension != ".cue" || FullPath.Length > 256) return;

            InValidEncode = !ConvertMethod.IsUTF8(FullPath);
            InValidCue    = !ConvertMethod.CueMatchCheck(this, !InValidEncode);

        }

        private readonly string[] _sizeTail = {"B", "KB", "MB", "GB", "TB", "PB"};
        public DataGridViewRow ToRow(DataGridView view)
        {
            var row = new DataGridViewRow {Tag = this};
            var scale = Length == 0 ? 0 : (int) Math.Floor(Math.Log(Length, 1024));
            row.CreateCells(view, ReletivePath, FileName, $"{Length / Math.Pow(1024, scale):F3}{_sizeTail[scale]}");
            row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml(InValidFile ? "#FB9966" : "#92AAF3");
            if (InValidCue)    row.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#FF6538");//orange
            if (InValidEncode) row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#4E4F97");//blue
            return row;
        }
    }
}
