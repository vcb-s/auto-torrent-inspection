using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    public class FileDescription
    {
        private string FileName   {         set; get; }
        private string Path       {         set; get; }
        public string FullPath    { private set; get; }
        public string Ext         { private set; get; }
        private long Length       {         set; get; }
        public bool InValidFile   { private set; get; }
        public bool InValidEncode { private set; get; }
        public bool InValidCue    { private set; get; }

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
                FullPath = fullPath,
                Ext = ext,
                Length = fullPath.Length > 256 ? 0L : new FileInfo(fullPath).Length
            };
            if (fullPath.Length > 256)
            {
                temp.InValidCue = false;
            }
            else
            {
                temp.CheckValidFile(temp);
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

        private void CheckValidFile(FileDescription file)
        {
            InValidFile = !ExceptPartten.IsMatch(Ext.ToLower()) &&
                          !MusicPartten.IsMatch(FileName.ToLower()) &&
                          !AnimePartten.IsMatch(FileName);
            if (Ext.ToLower() != ".cue") return;
            InValidCue = !ConvertMethod.CueMatchCheck(file);
            InValidEncode = !ConvertMethod.IsUTF8(file.FullPath);
        }

        public override string ToString() => $"{FileName}, length: {(double)Length / 1024:F3}KB";

        public DataGridViewRow ToRow(DataGridView view)
        {
            var row = new DataGridViewRow();
            row.CreateCells(view, Path, FileName, $"{(double)Length / 1024:F3}KB");
            row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml(InValidFile ? "#FB9966" : "#92AAF3");
            if (InValidCue)    row.DefaultCellStyle.ForeColor = ColorTranslator.FromHtml("#113285");
            if (InValidEncode) row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#4E4F97");
            return row;
        }
    }
}
