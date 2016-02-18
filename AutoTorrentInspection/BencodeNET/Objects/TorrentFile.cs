using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace BencodeNET.Objects
{
    public class TorrentFile
    {
        private readonly BDictionary _data = new BDictionary();

        public BDictionary Info => (BDictionary) _data["info"];

        /// <summary>
        /// The first announce URL contained within the .torrent file
        /// </summary>
        public string Announce => _data.ContainsKey("announce") ? _data["announce"].ToString() : null;

        /// <summary>
        /// The announce URLs contained within the .torrent file
        /// </summary>
        public BList AnnounceList => _data.ContainsKey("announce-list") ? (BList) _data["announce-list"] : null;

        /// <summary>
        /// The creation date of the .torrent file
        /// </summary>
        public DateTime CreationDate
        {
            get
            {
                var unixTime = (BNumber) _data["creation date"] ?? 0;
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(unixTime);
            }
        }

        /// <summary>
        /// The comment contained within the .torrent file
        /// </summary>
        public string Comment => _data.ContainsKey("comment") ? _data["comment"].ToString() : null;

        /// <summary>
        /// The optional string showing who/what created the .torrent
        /// </summary>
        public string CreatedBy => _data.ContainsKey("created by") ? _data["created by"].ToString() : "null";

        /// <summary>
        /// The encoding used by the client that created the .torrent file
        /// </summary>
        public string Encoding => _data.ContainsKey("encoding") ? _data["encoding"].ToString() : null;

        public string CalculateInfoHash()
        {
            return CalculateInfoHash((BDictionary)_data["info"]);
        }

        public byte[] CalculateInfoHashBytes()
        {
            return CalculateInfoHashBytes((BDictionary)_data["info"]);
        }

        public static string CalculateInfoHash(BDictionary info)
        {
            var hashBytes = CalculateInfoHashBytes(info);

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static byte[] CalculateInfoHashBytes(BDictionary info)
        {
            using (var sha1 = new SHA1Managed())
            using (var ms = new MemoryStream())
            {
                info.EncodeToStream(ms);
                ms.Position = 0;

                return sha1.ComputeHash(ms);
            }
        }

        public IBObject this[BString key] => _data[key];

        public static bool operator ==(TorrentFile first, TorrentFile second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(TorrentFile first, TorrentFile second)
        {
            return !(first == second);
        }

        public override bool Equals(object other)
        {
            var torrent = other as TorrentFile;
            if (torrent == null)
                return false;

            var comparisons = new List<bool>
            {
                Info == torrent.Info,
                Announce == torrent.Announce,
                AnnounceList == torrent.AnnounceList,
                CreationDate == torrent.CreationDate,
                CreatedBy == torrent.CreatedBy,
                Comment == torrent.Comment,
                Encoding == torrent.Encoding,
                CalculateInfoHash() == torrent.CalculateInfoHash()
            };

            return !comparisons.Contains(false);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public TorrentFile()
        { }

        public TorrentFile(BDictionary torrent)
        {
            _data = torrent;
        }
    }
}
