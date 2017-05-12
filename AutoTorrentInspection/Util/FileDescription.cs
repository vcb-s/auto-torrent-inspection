using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
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
        InValidEncode,
        InValidFlacLevel,
        NonUTF8WBOM
    }

    public class FileDescription
    {
        public string FileName           { get; private set; }
        public string ReletivePath       { get; }//载入文件夹到文件中间的相对路径
        private string BasePath          { get; }//所载入的文件夹的路径
        public string FullPath           { get; }//完整路径，Torrent下为手动拼接
        public string Extension          { get; }
        public long Length               { get; }
        public FlacInfo Flac             { get; private set; }

        public FileState State           { get; private set; } = FileState.InValidFile;

        public string Encode             { private set; get; }
        public float Confidence => _confindece;
        private float _confindece;
        public SourceTypeEnum SourceType { private set; get; }

        public override string ToString() => $"{FileName}, length: {(double)Length / 1024:F3}KB";

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
        private static readonly Color INVALID_FLAC_LEVEL  = Color.FromArgb(207, 216, 220);
        private static readonly Color NON_UTF_8_W_BOM     = Color.FromArgb(251, 188, 005);

        private static readonly Dictionary<FileState, Color> StateColor = new Dictionary<FileState, Color>
        {
            [FileState.ValidFile]         = VALID_FILE,
            [FileState.InValidPathLength] = INVALID_PATH_LENGTH,
            [FileState.InValidFile]       = INVALID_FILE,
            [FileState.InValidCue]        = INVALID_CUE,
            [FileState.InValidEncode]     = INVALID_ENCODE,
            [FileState.InValidFlacLevel]  = INVALID_FLAC_LEVEL,
            [FileState.NonUTF8WBOM]       = NON_UTF_8_W_BOM
        };

        private const long MaxFilePathLength = 240;

        public FileDescription(MultiFileInfo file, string torrentName)
        {
            BasePath      = torrentName;
            ReletivePath  = file.Path.Take(file.Path.Count - 1).Aggregate("", (current, item) => current += $"{item}\\").TrimEnd('\\');
            FileName      = file.FileName;
            FullPath      = Path.Combine(BasePath, ReletivePath, FileName);
            Extension     = Path.GetExtension(FileName)?.ToLower();

            Length        = file.FileSize;
            SourceType    = SourceTypeEnum.Torrent;
            BaseValidation();
        }

        public FileDescription(SingleFileInfo file, string torrentName)
        {
            BasePath     = torrentName;
            ReletivePath = "";
            FileName     = file.FileName;
            FullPath     = Path.Combine(BasePath, ReletivePath, FileName);
            Extension    = Path.GetExtension(FileName).ToLower();

            Length       = file.FileSize;
            SourceType   = SourceTypeEnum.Torrent;
            BaseValidation();
        }

        public FileDescription(string fileName, string reletivePath, string basePath)//file
        {
            BasePath     = basePath;
            ReletivePath = reletivePath.TrimEnd('\\');
            FileName     = fileName;
            FullPath     = Path.Combine(BasePath, ReletivePath, FileName);
            Extension    = Path.GetExtension(fileName)?.ToLower();

            Length       = ConvertMethod.GetFile(FullPath).Length;
            SourceType   = SourceTypeEnum.RealFile;
            FileValidation();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>true代表无需进一步的检查</returns>
        private bool BaseValidation()
        {
            if (FullPath.Length > MaxFilePathLength)
            {
                State = FileState.InValidPathLength;
                return true;
            }

            if (FileName == "readme about WebP.txt")
            {
                State = FileState.ValidFile;
                return true;
            }

            State = FileState.InValidFile;
            if (ExceptPattern.IsMatch(Extension) || MusicPattern.IsMatch(FileName) || AnimePattern.IsMatch(FileName) ||
                MenuPngPattern.IsMatch(FileName) || FchPattern.IsMatch(FileName)   || MaWenPattern.IsMatch(FileName))
            {
                State = FileState.ValidFile;
            }

            return false;
        }

        private void FileValidation()
        {
            if (BaseValidation() || State == FileState.InValidFile) return;
            if (Extension == ".flac")
            {
                Flac = FlacData.GetMetadataFromFlac(FullPath);
                _confindece = (float)Flac.CompressRate;
                FileName += $"[{Flac.CompressRate * 100:00.00}%]";
                if (Flac.HasCover) FileName += "[图]";
                Encode = Flac.Encoder;
                if (Flac.CompressRate > 0.9) //Maybe a level 0 file
                {
                    State = FileState.InValidFlacLevel;
                }
            }
            if (Extension != ".cue") return;
            CheckCUE();
        }

        public void CueFileRevalidation(DataGridViewRow row)
        {
            Debug.WriteLine(@"----ReCheck--Begin----");
            CheckCUE();
            var rowColor = StateColor[State];
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.BackColor = rowColor;
            }
            Application.DoEvents();
            Debug.WriteLine(@"----ReCheck--End----");
        }


        private bool CheckCUE()
        {
            State       = FileState.ValidFile;
            Encode     = EncodingDetector.GetEncoding(FullPath, out _confindece);
            if (Encode != "UTF-8")
            {
                State = FileState.InValidEncode;
                return false;
            }
            using (var fs = File.OpenRead(FullPath))
            {
                var buffer = new byte[3];
                fs.Read(buffer, 0, 3);
                if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                {
                    if (!CueCurer.CueMatchCheck(this))
                    {
                        State = FileState.InValidCue;
                    }
                    return true;
                }
            }
            State = FileState.NonUTF8WBOM;
            return false;
        }


        public DataGridViewRow ToRow()
        {
            var row = new DataGridViewRow {Tag = this};
            row.Cells.Add(new DataGridViewTextBoxCell {Value = ReletivePath});
            row.Cells.Add(new DataGridViewTextBoxCell {Value = FileName});
            row.Cells.Add(new DataGridViewTextBoxCell {Value = FileSize.FileSizeToString(Length)});
            row.DefaultCellStyle.BackColor = StateColor[State];
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
