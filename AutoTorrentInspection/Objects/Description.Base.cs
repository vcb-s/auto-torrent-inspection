using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection.Objects
{
    [Flags]
    public enum FileState : long
    {
        //universal
        ValidFile = 0,
        InValidPathLength = 1,
        InValidFile = 1 << 1,
        InValidFileSignature = 1 << 2,
        InValidFileNameCharacter = 1 << 3,
        EmptyFile = 1 << 4,
        InValidDir = 1 << 5,
        //cue
        InValidCue = 1 << 11,
        InValidEncode = 1 << 12,
        NonUTF8WBOM = 1 << 13,
        //flac
        InValidFlacLevel = 1 << 21,
        HiResAudio = 1 << 22,
        //log
        TamperedLog = 1 << 30,
    }

    public enum SourceTypeEnum
    {
        RealFile,
        TorrentFile
    }

    public partial class FileDescription
    {
        public string FileName { get; protected set; }

        public string Suffix { get; protected set; } = "";
        public string RelativePath { get; protected set; }
        public string BasePath { get; protected set; }
        public string FullPath => Path.Combine(BasePath, RelativePath, FileName);
        public string Extension => Path.GetExtension(FileName)?.ToLower();
        public long Length { get; protected set; }
        public FileState State { get; protected set; } = FileState.InValidFile;
        public SourceTypeEnum SourceType { get; protected set; }

        protected static readonly Regex VcbsNormalPattern = new Regex(GlobalConfiguration.Instance().Naming.Pattern.VCBS_NORMAL);
        protected static readonly Regex VcbsSpecialPattern = new Regex(GlobalConfiguration.Instance().Naming.Pattern.VCBS_SPECIAL);
        protected static readonly Regex FchPattern = new Regex(GlobalConfiguration.Instance().Naming.Pattern.FCH);
        protected static readonly Regex MaWenPattern = new Regex(GlobalConfiguration.Instance().Naming.Pattern.MAWEN);
        protected static readonly Regex VideoExtension = GlobalConfiguration.Instance().Naming.Extension.VideoExtension;
        protected static readonly Regex AudioExtension = GlobalConfiguration.Instance().Naming.Extension.AudioExtension;
        protected static readonly Regex ImageExtension = GlobalConfiguration.Instance().Naming.Extension.ImageExtension;
        protected static readonly Regex ExceptExtension = GlobalConfiguration.Instance().Naming.Extension.ExceptExtension;

        protected static readonly Color INVALID_FILE = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_FILE, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color VALID_FILE = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.VALID_FILE, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color INVALID_DIR = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_DIR, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color INVALID_CUE = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_CUE, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color INVALID_ENCODE = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_ENCODE, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color INVALID_PATH_LENGTH = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_PATH_LENGTH, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color INVALID_FLAC_LEVEL = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_FLAC_LEVEL, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color NON_UTF_8_W_BOM = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.NON_UTF_8_W_BOM, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color INVALID_FILE_SIGNATURE = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_FILE_SIGNATURE, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color TAMPERED_LOG = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.TAMPERED_LOG, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color INVALID_FILE_NAME_CHAR = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_FILE_NAME_CHAR, System.Globalization.NumberStyles.HexNumber));
        protected static readonly Color EMPTY_FILE = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.EMPTY_FILE, System.Globalization.NumberStyles.HexNumber));

        public static readonly Dictionary<FileState, Color> StateColor = new Dictionary<FileState, Color>
        {
            [FileState.ValidFile] = VALID_FILE,
            [FileState.InValidPathLength] = INVALID_PATH_LENGTH,
            [FileState.InValidFile] = INVALID_FILE,
            [FileState.InValidDir] = INVALID_DIR,
            [FileState.InValidCue] = INVALID_CUE,
            [FileState.InValidEncode] = INVALID_ENCODE,
            [FileState.InValidFlacLevel] = INVALID_FLAC_LEVEL,
            [FileState.NonUTF8WBOM] = NON_UTF_8_W_BOM,
            [FileState.InValidFileSignature] = INVALID_FILE_SIGNATURE,
            [FileState.TamperedLog] = TAMPERED_LOG,
            [FileState.InValidFileNameCharacter] = INVALID_FILE_NAME_CHAR,
            [FileState.EmptyFile] = EMPTY_FILE,
        };

        protected const long MaxFilePathLength = 240;

        protected FileDescription()
        {
            BasePath = string.Empty;
            RelativePath = string.Empty;
            FileName = string.Empty;
            Length = 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>true代表无需进一步的检查</returns>
        protected bool BaseValidation()
        {
            if (FullPath.Length > MaxFilePathLength)
            {
                State = FileState.InValidPathLength;
                Logger.Log(Logger.Level.Info, $"'{FullPath}': ${State}");
                return true;
            }

            if (GlobalConfiguration.Instance().Naming.UnexpectedCharacters.Any(character => FullPath.Contains(character)))
            {
                State = FileState.InValidFileNameCharacter;
                Logger.Log(Logger.Level.Info, $"'{FullPath}': COMBINING CHARACTER!");
                return true;
            }

            if (FileName == "readme about WebP.txt")
            {
                State = FileState.ValidFile;
                return true;
            }

            State = FileState.InValidFile;
            string[] filters = {"CDs", "Scans"};
            bool isInCDsDir = RelativePath == filters[0] || RelativePath.Contains(filters[0] + "\\");
            bool isInScansDir = RelativePath == filters[1] || RelativePath.Contains(filters[1] + "\\") || RelativePath.Contains("\\" + filters[1]);
            if (isInCDsDir && RegexesMatch(Extension, VideoExtension, AudioExtension, ImageExtension) ||
                isInScansDir && RegexesMatch(Extension, ImageExtension, ExceptExtension) ||
                !(isInCDsDir || isInScansDir) && RegexesMatch(FileName, VcbsNormalPattern, VcbsSpecialPattern, FchPattern, MaWenPattern))
            {
                State = FileState.ValidFile;
            }

            if (Length == 0)
            {
                State = FileState.EmptyFile;
                return true;
            }

            return false;
        }

        private static bool RegexesMatch(string value, params Regex[] regexes)
        {
            return regexes.Any(regex => regex.IsMatch(value));
        }

        public DataGridViewRow ToRow()
        {
            var row = new DataGridViewRow { Tag = this };
            row.Cells.Add(new DataGridViewTextBoxCell { Value = RelativePath });
            row.Cells.Add(new DataGridViewTextBoxCell { Value = FileName + Suffix });
            row.Cells.Add(new DataGridViewTextBoxCell { Value = FileSize.FileSizeToString(Length) });
            row.DefaultCellStyle.BackColor = StateColor[State];
            return row;
        }
    }
}
