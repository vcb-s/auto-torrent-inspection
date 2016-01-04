using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    public class FileDescription
    {
        private string FileName { set; get; }
        private string Path     { set; get; }
        private string Ext      { set; get; }
        private long Length     { set; get; }
        public bool InValidFile { private set; get; }
        public bool InValidCue  { private set; get; }

        public static FileDescription CreateWithCheckTorrent(string fileName, string path, string ext, long length)
        {
            var temp = new FileDescription
            {
                FileName = fileName,
                Path = path,
                Ext = ext,
                Length = length
            };
            temp.CheckValidTorrent();
            return temp;
        }

        public static FileDescription CreateWithCheckFile(string fileName, string path, string ext, string fullPath)
        {
            var temp = new FileDescription
            {
                FileName = fileName,
                Path = path,
                Ext = ext,
                Length = fullPath.Length > 256 ? 0L : new FileInfo(fullPath).Length
            };
            if (fullPath.Length > 256)
            {
                temp.InValidCue = false;
            }
            else
            {
                temp.CheckValidFile(fullPath);
            }
            return temp;
        }

        private static readonly Regex AnimePartten  = new Regex(@"^\[[^\[\]]*VCB-S(?:tudio)*[^\[\]]*\] [^\[\]]+ (\[.*\d*\])*\[((?<Ma>Ma10p_1080p)|(?<Hi>(Hi10p|Hi444pp)_(1080|720|480)p)|(?<EIGHT>(1080|720)p))\]\[((?<HEVC-Ma>x265)|(?<AVC-Hi>x264)|(?(EIGHT)x264))_\d*(flac|aac|ac3)\](?<SUB>(\.(sc|tc)|\.(chs|cht))*)\.((?(AVC)(mkv|mka|flac))|(?(HEVC)(mkv|mka|flac)|(?(EIGHT)mp4))|(?(SUB)ass))$");
        private static readonly Regex MusicPartten  = new Regex(@"\.(flac|tak|m4a|cue|log|jpg|jpeg|jp2)");
        private static readonly Regex ExceptPartten = new Regex(@"\.(rar|7z|zip)");

        private void CheckValidTorrent()
        {
            InValidFile = !ExceptPartten.IsMatch(Ext.ToLower()) &&
                          !MusicPartten.IsMatch(FileName.ToLower()) &&
                          !AnimePartten.IsMatch(FileName);
        }

        private void CheckValidFile(string fullPath)
        {
            InValidFile = !ExceptPartten.IsMatch(Ext.ToLower()) &&
                          !MusicPartten.IsMatch(FileName.ToLower()) &&
                          !AnimePartten.IsMatch(FileName);
            if (Ext.ToLower() == ".cue")
            {
                InValidCue = !ConvertMethod.IsUTF8(fullPath);
            }
        }

        public override string ToString() => $"{FileName}, length: {(double)Length / 1024:F3}KB";

        public DataGridViewRow ToRow(DataGridView view)
        {
            var row = new DataGridViewRow();
            row.CreateCells(view, Path, FileName, $"{(double)Length / 1024:F3}KB");
            row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml(InValidFile ? "#FB9966" : "#92AAF3");
            if (InValidCue) row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#113285");
            return row;
        }
    }
}
