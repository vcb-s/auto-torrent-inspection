namespace AutoTorrentInspection.Util
{
    public class FileDescription
    {
        public string FileName { set; get; }
        public string Path { set; get; }
        public string Ext  {set; get;}
        public string Category { set; get; }
        public long Length { set; get; }
        public bool Valid { set; get; }
    }
}