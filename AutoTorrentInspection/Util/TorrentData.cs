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

        public IEnumerable<string> GetAnnounceList() => _torrent.AnnounceList?.ToList().Select(item => ((BList)item).First().ToString()).ToList() ?? new List<string> { _torrent.Announce };

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
                //reason: https://www.ptt.cc/bbs/P2PSoftWare/M.1191552305.A.5CE.html

                fileDic.TryAdd(category, new List<FileDescription>());
                fileDic[category].Add(new FileDescription(name, path.ToString(), length));
            }
            return fileDic;
        }
    }
}