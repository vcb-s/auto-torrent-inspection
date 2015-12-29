using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BencodeNET;
using BencodeNET.Objects;

namespace AutoTorrentInspection.Util
{
    public class TorrentData
    {
        private readonly TorrentFile _torrent;

        public TorrentData(string path)
        {
            _torrent = Bencode.DecodeTorrentFile(path);
        }

        public IEnumerable<string> GetAnnounceList()
        {
            var announceList = _torrent.AnnounceList;
            return announceList?.ToList().Select(item => ((BList)item).First().ToString()).ToList() ?? new List<string>() { _torrent.Announce };
        }

        public string TorrentName => _torrent.Info["name"].ToString();

        public Dictionary<string, List<FileDescription>> GetFileList()
        {
            var fileDic = new Dictionary<string, List<FileDescription>>();
            var files = (BList)_torrent.Info["files"];
            if (files == null) return fileDic;
            foreach (var bObject in files)
            {
                var singleFile = (BList)((BDictionary)bObject)["path"];
                var length     = ((BNumber)((BDictionary)bObject)["length"]).Value;
                var category = singleFile.Count != 1 ? singleFile.First().ToString() : "root";
                var path = new StringBuilder();
                for (int i = 0; i < singleFile.Count - 1; i++)
                {
                    path.Append(singleFile[i] + "\\");
                }
                var fileName = singleFile.Last().ToString();
                var fileExt  = Path.GetExtension(fileName).ToLower();

                if (!fileDic.ContainsKey(category))
                {
                    fileDic.Add(category, new List<FileDescription>());
                }
                fileDic[category].Add(new FileDescription
                {
                    FileName = fileName,
                    Path = path.ToString(),
                    Ext = fileExt,
                    Category = category,
                    Length = length,
                });
            }
            return fileDic;
        }
    }
}