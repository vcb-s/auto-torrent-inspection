using System.Linq;
using BencodeNET.Torrents;

namespace AutoTorrentInspection.Objects
{
    public partial class FileDescription
    {
        private FileDescription(string torrentName) : this()
        {
            SourceType = SourceTypeEnum.TorrentFile;
            BasePath = torrentName;
        }

        public FileDescription(MultiFileInfo file, string torrentName) : this(torrentName)
        {
            ReletivePath = string.Join("\\", file.Path.Take(file.Path.Count - 1));
            FileName = file.FileName;
            Length = file.FileSize;
            BaseValidation();
        }

        public FileDescription(SingleFileInfo file, string torrentName) : this(torrentName)
        {
            FileName = file.FileName;
            Length = file.FileSize;
            BaseValidation();
        }
    }
}
