using System;
using System.Linq;
using System.Text;
using BencodeNET;
using BencodeNET.Objects;
using System.Collections.Generic;
using BencodeNET.Parsing;
using BencodeNET.Torrents;

namespace AutoTorrentInspection.Util
{
    public class TorrentData
    {
        private readonly Torrent _torrent;

        public TorrentData(string path)
        {
            var parser = new BencodeParser();
            _torrent = parser.Parse<Torrent>(path);
        }

        public IEnumerable<string> GetAnnounceList()
        {
            return _torrent.Trackers.Flatten();
        }

        public string CreatedBy => _torrent.CreatedBy;

        public DateTime CreationDate
        {
            get
            {
                var timeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                var utcTime        = _torrent.CreationDate ?? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return utcTime.Add(timeZoneOffset);
            }
        }

        public string Comment => _torrent.Comment;

        public string Source
        {
            get
            {
                var extra = _torrent.ExtraFields["info"] as BDictionary;
                if (extra?.ContainsKey("source") ?? false)
                {
                    return (extra["source"] as BString)?.ToString() ?? "";
                }
                if (extra?.ContainsKey("publisher") ?? false)
                {
                    return (extra["publisher"] as BString)?.ToString() ?? "";
                }
                return "";
            }
        }

        public string TorrentName
        {
            get { return new BString(_torrent.DisplayName, _torrent.Encoding).ToString(); }
        }

        public bool IsPrivate => _torrent.IsPrivate;

        public string Encoding => _torrent.Encoding.ToString();

        public IEnumerable<IEnumerable<string>> GetRawFileList()
        {
            return GetRawFileListWithAttribute().Select(item => item.Key);
        }

        public IEnumerable<KeyValuePair<IEnumerable<string>, FileSize>> GetRawFileListWithAttribute()
        {
            if (_torrent.FileMode == TorrentFileMode.Single)
            {
                FileSize fs = new FileSize(_torrent.File.FileSize);
                yield return new KeyValuePair<IEnumerable<string>, FileSize>(new[] {TorrentName}, fs);
                yield break;
            }
            foreach (var file in _torrent.Files)
            {
                if (file.Path.Last().Contains("_____padding_file_")) continue;
                FileSize fs = new FileSize(file.FileSize);
                yield return new KeyValuePair<IEnumerable<string>, FileSize>(file.Path, fs);
            }
        }


        public Dictionary<string, List<FileDescription>> GetFileList()
        {
            var fileDic = new Dictionary<string, List<FileDescription>>();
            var torrentName = TorrentName;
            if (_torrent.FileMode == TorrentFileMode.Single)
            {
                fileDic.Add("single",
                    new List<FileDescription>
                    {
                        new FileDescription(torrentName, "", torrentName, _torrent.File.FileSize)
                    });
                return fileDic;
            }
            foreach (var file in _torrent.Files)
            {
                var singleFile = file.Path.ToList();
                var category   = singleFile.Count != 1 ? singleFile.First() : "root";
                var path       = new StringBuilder();
                for (int i = 0; i < singleFile.Count - 1; i++)
                {
                    path.Append($"{singleFile[i]}\\");
                }
                //var path = singleFile.Aggregate(new StringBuilder(), (current, item) => current.Append($"{item}\\"));
                var name = singleFile.Last();
                if (name.Contains("_____padding_file_")) continue;
                //reason: https://zh.wikipedia.org/zh-hant/BitComet#.E6.96.87.E4.BB.B6.E5.88.86.E5.A1.8A.E5.B0.8D.E9.BD.8A

                fileDic.TryAdd(category, new List<FileDescription>());
                fileDic[category].Add(new FileDescription(name, path.ToString(), torrentName, file.FileSize));
            }
            return fileDic;
        }
    }
}
