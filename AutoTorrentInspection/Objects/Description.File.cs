using System.IO;
using System.Windows.Forms;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection.Objects
{
    public partial class FileDescription
    {
        public FlacInfo Flac { get; private set; }
        public string Encode { private set; get; }
        public float Confidence => _confidence;
        private float _confidence;

        public FileDescription(string fileName, string relativePath, string basePath) : this()
        {
            SourceType = SourceTypeEnum.RealFile;
            BasePath = basePath;
            RelativePath = relativePath.TrimEnd('\\');
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
                    if (!GlobalConfiguration.Instance().InspectionOptions.FLACCompressRate) goto SKIP_FLAC_COMPRESS_RATE;
                    Flac = FlacData.GetMetadataFromFlac(FullPath);
                    // _confidence = (float)Flac.CompressRate;
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
                    SKIP_FLAC_COMPRESS_RATE:
                    break;
                case ".cue":
                    if (!GlobalConfiguration.Instance().InspectionOptions.CUEEncoding) break;
                    CheckCUE();
                    break;
                case ".log":
                {
                    if (!GlobalConfiguration.Instance().InspectionOptions.LogValidation) break;
                    Logger.Log(Logger.Level.Info, $"Log check for '{FullPath}'");
                    Encode = EncodingDetector.GetEncoding(FullPath, out var confidence);
                    if (confidence < 0.9) break;
                    var text = File.ReadAllText(FullPath, System.Text.Encoding.GetEncoding(Encode));
                    var index = 1;
                    foreach (var (version, oldSignature, actualSignature) in LogChecker.Core.eac_verify(text))
                    {
                        if (oldSignature == "")
                        {
                            Logger.Log(Logger.Level.Debug, $"No signature found, it could be '{actualSignature}'");
                            continue;
                        }
                        if (oldSignature != actualSignature)
                        {
                            Logger.Log(Logger.Level.Debug, $"Expect signature '{actualSignature}', but get '{oldSignature}'");
                            State = FileState.TamperedLog;
                        }
                        else
                        {
                            Logger.Log(Logger.Level.Fine, $"{index++}. Log entry is fine!");
                        }
                    }
                    break;
                }
                default:
                    if (!GlobalConfiguration.Instance().InspectionOptions.FileHeader) break;
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
            State = FileState.ValidFile;
            Encode = EncodingDetector.GetEncoding(FullPath, out _confidence);
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
