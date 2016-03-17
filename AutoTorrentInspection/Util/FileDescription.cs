using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    public enum SourceTypeEnum
    {
        RealFile,
        Torrent
    }

    public class FileDescription
    {
        private string FileName     { get; }
        private string ReletivePath { get; }
        public string FullPath      { get; }
        public string Extension     { get; }
        private long Length         { get; }
        public bool InValidFile     { private set; get; }
        public bool InValidEncode   { private set; get; }
        public bool InValidCue      { private set; get; }
        public string Encode        { private set; get; }
        public float Confidence => _confindece;
        private float _confindece;
        public SourceTypeEnum SourceType { private set; get; }

        public override string ToString() => $"{FileName}, length: {(double)Length / 1024:F3}KB";

        private static readonly Regex AnimePartten  = new Regex(@"^\[[^\[\]]*VCB-S(?:tudio)*[^\[\]]*\] [^\[\]]+ (\[.*\d*\])*\[((((?<Ma>Ma10p)|(?<Hi>(Hi(10|444p)p)))_(2160|1080|720|480)p)|(?<EIGHT>(1080|720)p))\]\[((?<HEVC-Ma>x265)|(?<AVC-Hi>x264)|(?(EIGHT)x264))_\d*(flac|aac|ac3)\](\.(sc|tc|chs|cht))*\.((?(AVC)(mkv|mka|flac))|(?(HEVC)(mkv|mka|flac)|(?(EIGHT)mp4))|ass)$");
        private static readonly Regex MusicPartten  = new Regex(@"\.(flac|tak|m4a|cue|log|jpg|jpeg|jp2)$", RegexOptions.IgnoreCase);
        private static readonly Regex ExceptPartten = new Regex(@"\.(rar|7z|zip)$", RegexOptions.IgnoreCase);

        private readonly Color INVALID_FILE   = Color.FromArgb(251, 153, 102);
        private readonly Color VALID_FILE     = Color.FromArgb(146, 170, 243);
        private readonly Color INVALID_CUE    = Color.FromArgb(255, 101, 056);
        private readonly Color INVALID_ENCODE = Color.FromArgb(078, 079, 151);

        public FileDescription(string fileName, string reletivePath, long length)//Torrent
        {
            FileName     = fileName;
            ReletivePath = reletivePath;
            Extension    = Path.GetExtension(fileName)?.ToLower();
            Length       = length;
            SourceType   = SourceTypeEnum.Torrent;
            CheckValidTorrent();
        }

        public FileDescription(string fileName, string reletivePath, string fullPath)//file
        {
            FileName     = fileName;
            ReletivePath = reletivePath;
            FullPath     = fullPath;
            Extension    = Path.GetExtension(fileName)?.ToLower();
            //Length       = fullPath.Length > 256 ? -1024*1024L : new FileInfo(fullPath).Length
            Length       = ConvertMethod.GetFile(fullPath).Length;
            SourceType   = SourceTypeEnum.RealFile;
            CheckValidFile();
        }

        private void CheckValidTorrent()
        {
            InValidFile = !ExceptPartten.IsMatch(Extension) &&
                          !MusicPartten.IsMatch(FileName) &&
                          !AnimePartten.IsMatch(FileName);
        }

        public void RecheckCueFile(DataGridViewRow row)
        {
            Debug.WriteLine(@"----ReCheck--Begin--");
            InValidCue = !CueCurer.CueMatchCheck(this);
            //Encode = EncodingDetector.GetEncodingN(FullPath);
            Encode = EncodingDetector.GetEncodingU(FullPath, out _confindece);

            InValidEncode = Encode != "UTF-8";
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.ForeColor = InValidCue ? INVALID_CUE : Color.Black;
            }
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.BackColor = InValidEncode ? INVALID_ENCODE: VALID_FILE;
            }
            Application.DoEvents();
            Debug.WriteLine(@"----ReCheck--End--");
        }

        private void CheckValidFile()
        {
            InValidFile = !ExceptPartten.IsMatch(Extension) &&
                          !MusicPartten.IsMatch(FileName.ToLower()) &&
                          !AnimePartten.IsMatch(FileName);
            if (Extension != ".cue" || FullPath.Length > 256) return;

            //InValidEncode = !ConvertMethod.IsUTF8(FullPath);
            //Encode = EncodingDetector.GetEncodingN(FullPath);
            Encode = EncodingDetector.GetEncodingU(FullPath, out _confindece);
            InValidEncode = Encode != "UTF-8";

            InValidCue = !CueCurer.CueMatchCheck(this);

        }

        private readonly string[] _sizeTail = {"B", "KB", "MB", "GB", "TB", "PB"};
        public DataGridViewRow ToRow()
        {
            var row = new DataGridViewRow {Tag = this};
            var scale = Length == 0 ? 0 : (int) Math.Floor(Math.Log(Length, 1024));
            row.Cells.Add(new DataGridViewTextBoxCell {Value = ReletivePath});
            row.Cells.Add(new DataGridViewTextBoxCell {Value = FileName});
            row.Cells.Add(new DataGridViewTextBoxCell {Value = $"{Length/Math.Pow(1024, scale):F3}{_sizeTail[scale]}"});
            row.DefaultCellStyle.BackColor = InValidFile ? INVALID_FILE : VALID_FILE;
            if (InValidCue) row.DefaultCellStyle.ForeColor = INVALID_CUE;
            if (InValidEncode) row.DefaultCellStyle.BackColor = INVALID_ENCODE;
            return row;
        }
    }
}
