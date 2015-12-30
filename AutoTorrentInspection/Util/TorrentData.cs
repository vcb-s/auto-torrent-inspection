using System.IO;
using System.Linq;
using System.Text;
using BencodeNET;
using BencodeNET.Objects;
using System.Collections.Generic;

namespace AutoTorrentInspection.Util
{
    public class TorrentData
    {
        private readonly TorrentFile _torrent;

        public TorrentData(string path)
        {
            _torrent = Bencode.DecodeTorrentFile(path);
        }

        public bool IsPrivate { private set; get; }

        public IEnumerable<string> GetAnnounceList()
        {
            var announceList = _torrent.AnnounceList;
            return announceList?.ToList().Select(item => ((BList)item).First().ToString()).ToList() ?? new List<string> { _torrent.Announce };
        }

        public string TorrentName => _torrent.Info["name"].ToString();

        public string CreatedBy => _torrent.CreatedBy;


        public Dictionary<string, List<FileDescription>> GetFileList()
        {
            var fileDic = new Dictionary<string, List<FileDescription>>();
            var files = (BList)_torrent.Info["files"];
            IsPrivate = ((BNumber) _torrent.Info["private"]) != null;
            if (files == null)
            {
                var name    = _torrent.Info["name"].ToString();
                var fileExt = Path.GetExtension(name).ToLower();
                var length  = ((BNumber)_torrent.Info["length"]).Value;
                fileDic.Add("singe", new List<FileDescription>());
                fileDic["singe"].Add(FileDescription.CreateWithCheck(name, "", fileExt, length));
                return fileDic;
            }
            foreach (var bObject in files)
            {
                var singleFile = (BList)((BDictionary)bObject)["path"];
                var length     = ((BNumber)((BDictionary)bObject)["length"]).Value;
                var category   = singleFile.Count != 1 ? singleFile.First().ToString() : "root";
                var path       = new StringBuilder();
                for (int i = 0; i < singleFile.Count - 1; i++)
                {
                    path.Append($"{singleFile[i]}\\");
                }
                var name = singleFile.Last().ToString();
                var fileExt  = Path.GetExtension(name).ToLower();

                if (!fileDic.ContainsKey(category))
                {
                    fileDic.Add(category, new List<FileDescription>());
                }
                fileDic[category].Add(FileDescription.CreateWithCheck(name, "", fileExt, length));
            }
            return fileDic;
        }
    }
}