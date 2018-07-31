using System.IO;
using System.Windows.Forms;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection.Objects
{
    public partial class FileDescription
    {
        public FlacInfo Flac { get; private set; }
        public string Encode { private set; get; }
        public float Confidence => _confindece;
        private float _confindece;

        public FileDescription(string fileName, string reletivePath, string basePath) : this()
        {
            SourceType = SourceTypeEnum.RealFile;
            BasePath = basePath;
            ReletivePath = reletivePath.TrimEnd('\\');
            FileName = fileName;
            Length = ConvertMethod.GetFile(FullPath).Length;
            FileValidation();
        }

        private void FileValidation()
        {
            if (BaseValidation()/* || State == FileState.InValidFile*/) return;
            switch (Extension)
            {
                case ".flac":
                {
                    if (!GlobalConfiguration.Instance().InspectionOptions.FLACCompressRate) goto SKIP_FLAC_COMRESS_RATE;
                    Flac = FlacData.GetMetadataFromFlac(FullPath);
                    // _confindece = (float)Flac.CompressRate;
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
                    SKIP_FLAC_COMRESS_RATE:
                    break;
                case ".cue":
                    if (!GlobalConfiguration.Instance().InspectionOptions.CUEEncoding) goto SKIP_CUE_ENCODING;
                    CheckCUE();
                    SKIP_CUE_ENCODING:
                    break;
                case ".log":
                {
                    Encode = EncodingDetector.GetEncoding(FullPath, out var confidence);
                    if (confidence < 0.9) break;
                    var text = File.ReadAllText(FullPath);
                    var (version, old_signature, actual_signature) = LogChecker.Core.eac_verify(text);
                    if (old_signature == "") break;
                    if (old_signature != actual_signature)
                    {
                        State = FileState.TamperedLog;
                    }
                }
                    break;
                default:
                    if (!GlobalConfiguration.Instance().InspectionOptions.FileHeader) goto SKIP_FILE_HEADER;
                    if (!FileHeader.Check(FullPath))
                    {
                        State = FileState.InValidFileSignature;
                    }
                    SKIP_FILE_HEADER:
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
            State = FileState.ValidFile;
            Encode = EncodingDetector.GetEncoding(FullPath, out _confindece);
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
    }
}
