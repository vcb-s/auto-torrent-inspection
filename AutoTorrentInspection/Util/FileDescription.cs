using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
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

    [Flags]
    public enum FileState : long
    {
        //universal
        ValidFile         = 0,
        InValidPathLength = 1,
        InValidFile       = 1 << 1,
        InValidFileSignature = 1 << 2,
        //cue
        InValidCue        = 1 << 11,
        InValidEncode     = 1 << 12,
        NonUTF8WBOM       = 1 << 13,
        //flac
        InValidFlacLevel  = 1 << 21,
        HiResAudio        = 1 << 22,
    };

    public class FileDescription
    {
        public string FileName           { get; private set; }
        public string ReletivePath       { get; }//载入文件夹到文件中间的相对路径
        private string BasePath          { get; }//所载入的文件夹的路径
        public string FullPath           { get; }//完整路径，Torrent下为手动拼接
        public List<string> ParentFolder { get; }
        public string Extension          { get; }
        public long Length               { get; }
        public FlacInfo Flac             { get; private set; }

        public FileState State           { get; private set; } = FileState.InValidFile;

        public string Encode             { private set; get; }
        public float Confidence => _confindece;
        private float _confindece;
        public SourceTypeEnum SourceType { private set; get; }

        public override string ToString() => $"{FileName}, length: {(double)Length / 1024:F3}KB";

        private static readonly Regex AnimePattern        = new Regex(GlobalConfiguration.Instance().Naming.Pattern.VCBS);
        private static readonly Regex MenuPngPattern      = new Regex(GlobalConfiguration.Instance().Naming.Pattern.MENU);
        private static readonly Regex FchPattern          = new Regex(GlobalConfiguration.Instance().Naming.Pattern.FCH);
        private static readonly Regex MaWenPattern        = new Regex(GlobalConfiguration.Instance().Naming.Pattern.MAWEN);
        private static readonly Regex AudioExtension      = GlobalConfiguration.Instance().Naming.Extension.AudioExtension;
        private static readonly Regex ImageExtension      = GlobalConfiguration.Instance().Naming.Extension.ImageExtension;
        private static readonly Regex ExceptExtension     = GlobalConfiguration.Instance().Naming.Extension.ExceptExtension;

        private static readonly Color INVALID_FILE          = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_FILE         , System.Globalization.NumberStyles.HexNumber));
        private static readonly Color VALID_FILE            = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.VALID_FILE           , System.Globalization.NumberStyles.HexNumber));
        private static readonly Color INVALID_CUE           = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_CUE          , System.Globalization.NumberStyles.HexNumber));
        private static readonly Color INVALID_ENCODE        = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_ENCODE       , System.Globalization.NumberStyles.HexNumber));
        private static readonly Color INVALID_PATH_LENGTH   = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_PATH_LENGTH  , System.Globalization.NumberStyles.HexNumber));
        private static readonly Color INVALID_FLAC_LEVEL    = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_FLAC_LEVEL   , System.Globalization.NumberStyles.HexNumber));
        private static readonly Color NON_UTF_8_W_BOM       = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.NON_UTF_8_W_BOM      , System.Globalization.NumberStyles.HexNumber));
        private static readonly Color INVALID_FILE_SIGNATUR = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_FILE_SIGNATUR, System.Globalization.NumberStyles.HexNumber));

        private static readonly Dictionary<FileState, Color> StateColor = new Dictionary<FileState, Color>
        {
            [FileState.ValidFile]            = VALID_FILE,
            [FileState.InValidPathLength]    = INVALID_PATH_LENGTH,
            [FileState.InValidFile]          = INVALID_FILE,
            [FileState.InValidCue]           = INVALID_CUE,
            [FileState.InValidEncode]        = INVALID_ENCODE,
            [FileState.InValidFlacLevel]     = INVALID_FLAC_LEVEL,
            [FileState.NonUTF8WBOM]          = NON_UTF_8_W_BOM,
            [FileState.InValidFileSignature] = INVALID_FILE_SIGNATUR
        };

        private const long MaxFilePathLength = 240;

        public FileDescription(MultiFileInfo file, string torrentName)
        {
            BasePath      = torrentName;
            ReletivePath  = file.Path.Take(file.Path.Count - 1).Aggregate("", (current, item) => current + item + "\\").TrimEnd('\\');
            FileName      = file.FileName;
            FullPath      = Path.Combine(BasePath, ReletivePath, FileName);
            ParentFolder = FullPath.Split('\\', '/').Reverse().Skip(1).ToList();
            Extension     = Path.GetExtension(FileName).ToLower();

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
            ParentFolder = FullPath.Split('\\', '/').Reverse().Skip(1).ToList();
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

        private static bool RegexesMatch(string value, params Regex[] regexes)
        {
            return regexes.Any(regex => regex.IsMatch(value));
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
            if (RegexesMatch(Extension, ExceptExtension, ImageExtension, AudioExtension) ||
                RegexesMatch(FileName, AnimePattern, MenuPngPattern, FchPattern, MaWenPattern))
            {
                State = FileState.ValidFile;
            }

            return false;
        }

        private void FileValidation()
        {
            if (BaseValidation()/* || State == FileState.InValidFile*/) return;
            switch (Extension)
            {
                case ".flac":
                {
                    Flac = FlacData.GetMetadataFromFlac(FullPath);
                    _confindece = (float)Flac.CompressRate;
                    FileName += $"[{Flac.CompressRate * 100:00.00}%]";
                    if (Flac.IsHiRes)
                    {
                        FileName += "[HR]";
                    }
                    if (Flac.HasCover) FileName += "[图]";
                    Encode = Flac.Encoder;
                    if (Flac.CompressRate > 0.9) //Maybe a level 0 file
                    {
                        State = FileState.InValidFlacLevel;
                    }
                }
                    break;
                case ".cue":
                    CheckCUE();
                    break;
                default:
                    if (!FileHeader.Check(FullPath))
                    {
                        State = FileState.InValidFileSignature;
                    }
                    break;
            }
        }

        public void CueFileRevalidation(DataGridViewRow row)
        {
            CheckCUE();
            var rowColor = StateColor[State];
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.BackColor = rowColor;
            }
            Application.DoEvents();
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
