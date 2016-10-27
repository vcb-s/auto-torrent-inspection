using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using BencodeNET.Torrents;
// ReSharper disable InconsistentNaming

namespace AutoTorrentInspection.Util
{
    public enum SourceTypeEnum
    {
        RealFile,
        Torrent
    }

    public enum FileState
    {
        ValidFile,
        InValidPathLength,
        InValidFile,
        InValidCue,
        InValidEncode
    }

    public class FileDescription
    {
        public string FileName      { get; }
        public string ReletivePath { get; }
        private string BasePath     { get; }
        public string FullPath      { get; }
        public string Extension     { get; }
        public long Length          { get; }

        public FileState State { get; private set; } = FileState.InValidFile;

        public string Encode   { private set; get; }
        public float Confidence => _confindece;
        private float _confindece;
        public SourceTypeEnum SourceType { private set; get; }

        public override string ToString() => $"{FileName}, length: {(double)Length / 1024:F3}KB";

        //private static readonly Regex AnimePattern   = new Regex(@"^\[[^\[\]]*VCB-S(?:tudio)?[^\[\]]*\] [^\[\]]+ (?:\[[^\[\]]*\d*\])?\[(?:(?:(?:(?<Ma>Ma10p)|(?<Hi>(?:Hi(?:10|444p)p)))_(?:2160|1080|720|576|480)p)|(?<EIGHT>(?:1080|576|720)p))\]\[(?:(?<HEVC-Ma>x265)|(?<AVC-Hi>x264)|(?(EIGHT)x264))_\d*(?:flac|aac|ac3)\](?:\.(?:sc|tc|chs|cht))?\.(?:(?(AVC)(?:mkv|mka|flac))|(?(HEVC)(?:mkv|mka|flac))|(?(EIGHT)mp4)|ass)$");
        private static readonly Regex AnimePattern   = new Regex(@"^\[[^\[\]]*VCB\-S(?:tudio)?[^\[\]]*\] [^\[\]]+ (?:\[[^\[\]]*\d*\])?\[(?:(?:(?:(?:Hi10p|Hi444pp)_(?:2160|1080|720|576|480)p\]\[x264)|(?:(?:Ma10p_(?:2160|1080|720|576|480)p\]\[x265)))(?:_\d*(?:flac|aac|ac3))+\](?:\.(?:mkv|mka|flac))?|(?:(?:1080|720|576)p\]\[(?:x264|x265)_(?:aac|ac3)\](?:\.mp4)?))(?:(?<!(?:mkv|mka|mp4))(?:\.(?:[SsTt]c|[Cc]h(?:s|t)|[Jj](?:pn|ap)|[Cc]h(?:s|t)&[Jj](?:pn|ap)))?\.ass)?$"); //implement without Balancing group
        private static readonly Regex MenuPngPattern = new Regex(@"^\[[^\[\]]*VCB\-S(?:tudio)*[^\[\]]*\] [^\[\]]+ \[[^\[\]]*\]\.png$");
        private static readonly Regex MusicPattern   = new Regex(@"\.(flac|tak|m4a|cue|log|jpg|jpeg|jp2|webp)$", RegexOptions.IgnoreCase);
        private static readonly Regex ExceptPattern  = new Regex(@"\.(rar|7z|zip)$", RegexOptions.IgnoreCase);
        private static readonly Regex FchPattern     = new Regex(@"^(?:\[(?:[^\[\]])*philosophy\-raws(?:[^\[\]])*\])\[[^\[\]]+\]\[(?:(?:[^\[\]]+\]\[(?:BDRIP|DVDRIP|BDRemux))|(?:(?:BDRIP|DVDRIP|BDRemux)(?:\]\[[^\[\]]+)?))\]\[(?:(?:(?:HEVC )?Main10P)|(?:(?:AVC )?Hi10P)|Hi444PP|H264) \d*(?:FLAC|AC3)\]\[(?:(?:(?:1920|1440)[Xx]1080)|(?:1280[Xx]720)|(?:1024[Xx]576)|(?:720[Xx]480))\](?:(?:\.(?:sc|tc|chs|cht))?\.ass|(?:\.(?:mkv|mka|flac)))$");
        private static readonly Regex MaWenPattern   = new Regex(@"^[^\[\]]+ \[(?:BD|BluRay|BD\-Remux|SPDVD|DVD) (?:1920x1080p?|1280x720p?|720x480p?|1080p|720p|480p)(?: (?:23\.976|24|25|29\.970|59\.940)fps)?(?: vfr)? (?:(?:(?:AVC|HEVC)\-(?:Lossless-)?(?:yuv420p10|yuv420p8|yuv444p10))|(?:x264(?:-Hi(?:10|444P)P)?|x265-Ma10P))(?: (?:FLAC|AAC|AC3)(?:x\d)?)+(?: (?:Chap|Ordered\-Chap))?\](?: v\d)? - (?:[^\.&]+ ?& ?)*mawen1250(?: ?& ?[^\.&]+)*(?:(?:\.(?:sc|tc|chs|cht))?\.ass|(?:\.(?:mkv|mka|flac)))$");

        private static readonly Color INVALID_FILE        = Color.FromArgb(251, 153, 102);
        private static readonly Color VALID_FILE          = Color.FromArgb(146, 170, 243);
        private static readonly Color INVALID_CUE         = Color.FromArgb(255, 101, 056);
        private static readonly Color INVALID_ENCODE      = Color.FromArgb(078, 079, 151);
        private static readonly Color INVALID_PATH_LENGTH = Color.FromArgb(255, 010, 050);

        private const long MaxFilePathLength = 210;

        public FileDescription(MultiFileInfo file, string torrentName)
        {
            FileName      = file.FileName;
            Extension     = Path.GetExtension(FileName);
            BasePath      = torrentName;
            Length        = file.FileSize;
            SourceType    = SourceTypeEnum.Torrent;
            ReletivePath  = file.Path.Take(file.Path.Count - 1).Aggregate("", (current, item) => current += $"{item}\\");
            CheckValidTorrent();
        }

        public FileDescription(SingleFileInfo file, string torrentName)
        {
            FileName     = file.FileName;
            Extension    = Path.GetExtension(FileName);
            BasePath     = torrentName;
            Length       = file.FileSize;
            SourceType   = SourceTypeEnum.Torrent;
            ReletivePath = "";
            CheckValidTorrent();
        }

        public FileDescription(string fileName, string reletivePath, string basePath, string fullPath)//file
        {
            FileName     = fileName;
            ReletivePath = reletivePath;
            BasePath     = basePath;
            FullPath     = fullPath;
            Extension    = Path.GetExtension(fileName)?.ToLower();
            //Length       = fullPath.Length > 256 ? -1024*1024L : new FileInfo(fullPath).Length
            Length       = ConvertMethod.GetFile(fullPath).Length;
            SourceType   = SourceTypeEnum.RealFile;
            CheckValidFile();
        }

        private void CheckValidTorrent()
        {
            if (BasePath.Length + ReletivePath.Length + FileName.Length > MaxFilePathLength)
            {
                State = FileState.InValidPathLength;
                return;
            }
            if (ExceptPattern.IsMatch(Extension) || MusicPattern.IsMatch(FileName) || AnimePattern.IsMatch(FileName) ||
                MenuPngPattern.IsMatch(FileName) || FchPattern.IsMatch(FileName) || MaWenPattern.IsMatch(FileName))
            {
                State = FileState.ValidFile;
                return;
            }
            State = FileState.InValidFile;
            if (FileName == "readme about WebP.txt")
            {
                State = FileState.ValidFile;
            }
        }

        public void RecheckCueFile(DataGridViewRow row)
        {
            State = FileState.ValidFile;
            Debug.WriteLine(@"----ReCheck--Begin----");
            Encode = EncodingDetector.GetEncoding(FullPath, out _confindece);
            if (Encode != "UTF-8")
            {
                State = FileState.InValidEncode;
                goto ReSetColor;
            }
            if (!CueCurer.CueMatchCheck(this))
            {
                State = FileState.InValidCue;
            }

            ReSetColor:
            Color rowColor = VALID_FILE;
            switch (State)
            {
                case FileState.InValidCue:
                    rowColor = INVALID_CUE;
                    break;
                case FileState.InValidEncode:
                    rowColor = INVALID_ENCODE;
                    break;
            }

            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.BackColor = rowColor;
            }
            Application.DoEvents();
            Debug.WriteLine(@"----ReCheck--End----");
        }

        private void CheckValidFile()
        {
            //Debug.WriteLine(FullPath.Length);
            if (BasePath.Length + ReletivePath.Length + FileName.Length > MaxFilePathLength)
            {
                State = FileState.InValidPathLength;
                return;
            }
            if (ExceptPattern.IsMatch(Extension) || MusicPattern.IsMatch(FileName) || AnimePattern.IsMatch(FileName) ||
                MenuPngPattern.IsMatch(FileName) || FchPattern.IsMatch(FileName) || MaWenPattern.IsMatch(FileName))
            {
                State = FileState.ValidFile;
            }
            else
            {
                State = FileState.InValidFile;
            }
            if (FileName == "readme about WebP.txt")
            {
                State = FileState.ValidFile;
                return;
            }
            if (Extension != ".cue"/* || FullPath.Length > 256*/) return;

            Encode = EncodingDetector.GetEncoding(FullPath, out _confindece);
            if (Encode != "UTF-8")
            {
                State = FileState.InValidEncode;
                return;
            }
            if (!CueCurer.CueMatchCheck(this))
            {
                State = FileState.InValidCue;
            }
        }

        public DataGridViewRow ToRow()
        {
            var row = new DataGridViewRow {Tag = this};
            row.Cells.Add(new DataGridViewTextBoxCell {Value = ReletivePath});
            row.Cells.Add(new DataGridViewTextBoxCell {Value = FileName});
            row.Cells.Add(new DataGridViewTextBoxCell {Value = FileSize.FileSizeToString(Length)});
            switch (State)
            {
                case FileState.ValidFile:
                    row.DefaultCellStyle.BackColor = VALID_FILE;
                    break;
                case FileState.InValidPathLength:
                    row.DefaultCellStyle.BackColor = INVALID_PATH_LENGTH;
                    break;
                case FileState.InValidFile:
                    row.DefaultCellStyle.BackColor = INVALID_FILE;
                    break;
                case FileState.InValidCue:
                    row.DefaultCellStyle.BackColor = INVALID_CUE;
                    break;
                case FileState.InValidEncode:
                    row.DefaultCellStyle.BackColor = INVALID_ENCODE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return row;
        }
    }


    public class FileSize
    {
        public long Length { get; private set; }

        private static readonly string[] SizeTail = { "B", "KB", "MB", "GB", "TB", "PB" };

        private static string _toString(long length)
        {
            var scale = length == 0 ? 0 : (int)Math.Floor(Math.Log(length, 1024));
            return $"{length / Math.Pow(1024, scale):F3}{SizeTail[scale]}";
        }

        public override string ToString() => _toString(Length);

        public static string FileSizeToString(long length) => _toString(length);

        public FileSize(long length)
        {
            Length = length;
        }
    }
}
