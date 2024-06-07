using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoTorrentInspection.Util;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using BencodeNET.Torrents;

namespace AutoTorrentInspection.Objects
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
            return _torrent.Trackers.SelectMany(x => x);
        }

        public IList<IList<string>> RawAnnounceList => _torrent.Trackers;

        public string CreatedBy => _torrent.CreatedBy;

        public DateTime CreationDate
        {
            get
            {
                var utcTime = _torrent.CreationDate ?? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);
            }
        }

        public string Comment => _torrent.Comment;

        public string Source
        {
            get
            {
                if (!(_torrent.ExtraFields["info"] is BDictionary extra)) return "";
                if (extra.ContainsKey("source"))
                {
                    return (extra["source"] as BString)?.ToString() ?? "";
                }
                if (extra.ContainsKey("publisher"))
                {
                    return (extra["publisher"] as BString)?.ToString() ?? "";
                }
                return "";
            }
        }

        public string TorrentName => !string.IsNullOrEmpty(_torrent.DisplayNameUtf8) ? _torrent.DisplayNameUtf8 : _torrent.DisplayName;

        public bool IsPrivate => _torrent.IsPrivate;

        public bool IsSingle => _torrent.FileMode == TorrentFileMode.Single;

        public string Encoding => _torrent.Encoding?.WebName;

        public string MagnetLink => _torrent.GetMagnetLink();

        public long PieceSize => _torrent.PieceSize;

        public IEnumerable<IEnumerable<string>> GetRawFileList()
        {
            return GetRawFileListWithAttribute().Select(item => item.path);
        }

        public IEnumerable<(IEnumerable<string> path, FileSize size)> GetRawFileListWithAttribute()
        {
            if (_torrent.FileMode == TorrentFileMode.Single)
            {
                var fs = new FileSize(_torrent.File.FileSize);
                yield return (new[] {TorrentName}, fs);
                yield break;
            }
            foreach (var file in _torrent.Files)
            {
                if (file.Path.Last().StartsWith("_____padding_file")) continue;
                var fs = new FileSize(file.FileSize);
                yield return (file.Path, fs);
            }
        }


        public Dictionary<string, List<FileDescription>> GetFileList()
        {
            var fileDic = new Dictionary<string, List<FileDescription>>();
            var torrentName = TorrentName;
            if (_torrent.FileMode == TorrentFileMode.Single)
            {
                return new Dictionary<string, List<FileDescription>>
                {
                    ["single"] = new List<FileDescription>
                    {
                        new FileDescription(_torrent.File, torrentName)
                    }
                };
            }
            foreach (var file in _torrent.Files)
            {
                var category   = file.Path.Count != 1 ? file.Path.First() : "root";
                if (file.FileName.StartsWith("_____padding_file")) continue;
                //reason: https://zh.wikipedia.org/zh-hant/BitComet#.E6.96.87.E4.BB.B6.E5.88.86.E5.A1.8A.E5.B0.8D.E9.BD.8A

                fileDic.TryAdd(category, new List<FileDescription>());
                fileDic[category].Add(new FileDescription(file, torrentName));
            }
            return fileDic;
        }

        public SeriesDir GetSeriesDirFileList()
        {
            SeriesDir fileDic;
            var torrentName = TorrentName;
            Logger.Log($"torrentName: {torrentName}");

            // 检测是系列目录还是季度目录
            bool isSeries = !_torrent.Files.Where(_ => _.Path.Count == 1 && !_.FileName.StartsWith("_____padding_file")).Any();
            fileDic = new SeriesDir(torrentName, isSeries);
            int seasonId = 0;

            if (isSeries)
            {
                foreach (var sdir in _torrent.Files.Where(_ => _.Path.Count != 1 && !_.FileName.StartsWith("_____padding_file")).Select(_ => _.Path.First()).Distinct())
                {
                    Logger.Log($"search: {sdir}");
                    var seasonDir = new SeasonDir(sdir, seasonId++);

                    seasonDir.SubDirs.TryAdd("root", new DirDescription(".", Path.Combine(sdir, "root"), torrentName));
                    foreach (var dir in _torrent.Files.Where(_ => _.Path.Count >= 3 && _.Path.First() == sdir && !_.FileName.StartsWith("_____padding_file"))
                            .Select(_ => _.Path[1]).Distinct())
                    {
                        seasonDir.SubDirs.TryAdd(dir, new DirDescription(dir, Path.Combine(sdir, dir), torrentName));
                    }

                    foreach (var file in _torrent.Files.Where(_ => _.Path.Count >= 2 && _.Path.First() == sdir))
                    {
                        var category = file.Path.Count != 2 ? file.Path[1] : "root";
                        if (file.FileName.StartsWith("_____padding_file")) continue;

                        seasonDir.SubDirs[category].Files.Add(new FileDescription(file, torrentName));
                    }

                    fileDic.SeasonDirs.Add(seasonDir);
                }
            }
            else
            {
                Logger.Log($"search: {torrentName}");
                var seasonDir = new SeasonDir(torrentName, seasonId++);

                seasonDir.SubDirs.TryAdd("root", new DirDescription(".", "root", torrentName));
                foreach (var dir in _torrent.Files.Where(_ => _.Path.Count != 1 && !_.FileName.StartsWith("_____padding_file")).Select(_ => _.Path.First()).Distinct())
                {
                    seasonDir.SubDirs.TryAdd(dir, new DirDescription(dir, dir, torrentName));
                }

                foreach (var file in _torrent.Files)
                {
                    var category = file.Path.Count != 1 ? file.Path.First() : "root";
                    if (file.FileName.StartsWith("_____padding_file")) continue;

                    seasonDir.SubDirs[category].Files.Add(new FileDescription(file, torrentName));
                }
                fileDic.SeasonDirs.Add(seasonDir);
            }

            Logger.Log($"series name: {fileDic.DirName}, isSeries: {fileDic.IsSeries}, validType: {fileDic.State}");
            foreach (var season in fileDic.SeasonDirs)
            {
                Logger.Log($"  season name: {season.DirName}, validType: {season.State}");
                foreach (var subdir in season.SubDirs)
                {
                    Logger.Log($"        subdir name: {subdir.Value.DirName}, validType: {subdir.Value.State}, fileNum: {subdir.Value.Files.Count}");
                    Logger.Log($"               relativePath: {subdir.Value.RelativePath}, basePath: {subdir.Value.BasePath}");
                }
            }

            return fileDic;
        }
    }
}
