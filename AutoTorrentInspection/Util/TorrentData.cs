using System;
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

        public IEnumerable<string> GetAnnounceList()
        {
            var list = _torrent.AnnounceList;
            if(list == null || list.Count < 1) return new []{ _torrent.Announce };
            return list.Select(item => ((BList) item).First().ToString());
        }

        public string CreatedBy => _torrent.CreatedBy;

        public DateTime CreationDate
        {
            get
            {
                TimeSpan timeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                DateTime utcTime        = _torrent.CreationDate;
                return utcTime.Add(timeZoneOffset);
            }
        }

        public string Comment => _torrent.Comment;

        public string Source
        {
            get
            {
                string source = string.Empty;
                if (_torrent.Info.ContainsKey("source"))
                {
                    source = _torrent.Info["source"].ToString();
                }
                else
                {
                    if (_torrent.Info.ContainsKey("publisher"))
                    {
                        source = _torrent.Info.ContainsKey("publisher").ToString();
                    }
                    if (_torrent.Info.ContainsKey("publisher.utf-8"))
                    {
                        source = _torrent.Info["publisher.utf-8"].ToString();
                    }
                }
                return source;
            }
        }

        public string TorrentName
        {
            get
            {
                if (_torrent.Info.ContainsKey("name.utf-8"))
                {
                    return _torrent.Info["name.utf-8"].ToString();
                }
                return _torrent.Info["name"].ToString();
            }
        }

        public bool IsPrivate
        {
            get
            {
                var pri = _torrent.Info["private"];
                return pri != null && (BNumber) pri != 0 && (BNumber) pri == 1;
            }
        }

        public string Encoding => _torrent.Encoding;

        public IEnumerable<IEnumerable<string>> GetRawFileList()
        {
            if (!_torrent.Info.ContainsKey("files"))
            {
                yield return new []{ TorrentName };
            }
            else
            {
                var files = (BList)_torrent.Info["files"];
                foreach (var file in files)
                {
                    BList singleFile = (BList)((BDictionary)file)["path"];
                    if (((BDictionary)file).ContainsKey("path.utf-8"))
                    {
                        singleFile = (BList)((BDictionary)file)["path.utf-8"];
                    }
                    if (singleFile.Last().ToString().Contains("_____padding_file_")) continue;
                    var length = ((BNumber)((BDictionary)file)["length"]).Value;
                    yield return singleFile.Select(item=>item.ToString());
                }
            }
        }

        public Dictionary<string, List<FileDescription>> GetFileList()
        {
            var fileDic = new Dictionary<string, List<FileDescription>>();
            if (!_torrent.Info.ContainsKey("files"))
            {
                var name = TorrentName;
                var length = ((BNumber)_torrent.Info["length"]).Value;
                fileDic.Add("single", new List<FileDescription> { new FileDescription(name, "", length) });
                return fileDic;
            }
            var files = (BList)_torrent.Info["files"];
            foreach (var file in files)
            {
                BList singleFile = (BList) ((BDictionary) file)["path"];
                if (((BDictionary) file).ContainsKey("path.utf-8"))
                {
                    singleFile = (BList) ((BDictionary) file)["path.utf-8"];
                }
                var length     = ((BNumber) ((BDictionary) file)["length"]).Value;
                var category   = singleFile.Count != 1 ? singleFile.First().ToString() : "root";
                var path       = new StringBuilder();
                for (int i = 0; i < singleFile.Count - 1; i++)
                {
                    path.Append($"{singleFile[i]}\\");
                }
                //var path = singleFile.Aggregate(new StringBuilder(), (current, item) => current.Append($"{item}\\"));
                var name = singleFile.Last().ToString();
                if (name.Contains("_____padding_file_")) continue;
                //reason: https://zh.wikipedia.org/zh-hant/BitComet#.E6.96.87.E4.BB.B6.E5.88.86.E5.A1.8A.E5.B0.8D.E9.BD.8A

                fileDic.TryAdd(category, new List<FileDescription>());
                fileDic[category].Add(new FileDescription(name, path.ToString(), length));
            }
            return fileDic;
        }
    }
}
