using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoTorrentInspection.Objects;

namespace AutoTorrentInspection.Util
{
    public static class FileHeader
    {
        //https://en.wikipedia.org/wiki/List_of_file_signatures
        //http://www.garykessler.net/library/file_sigs.html
        private static readonly Dictionary<string, (byte[] signature, int offset)[]> Header = new Dictionary<string, (byte[], int)[]>
        {
            [".mp3"]  = new[] { (new byte[] { 0x49, 0x44, 0x33 }, 0) },
            [".flac"] = new[] { (new byte[] { 0x66, 0x4C, 0x61, 0x43 }, 0) },
            [".m4a"]  = new[] { (new byte[] { 0x66, 0x74, 0x79, 0x70, 0x4D, 0x34, 0x41, 0x20 }, 4) },
            [".aac"]  = new[] { (new byte[] { 0xFF, 0xF1 }, 0) },
            [".tak"]  = new[] { (new byte[] { 0x74, 0x42, 0x61, 0x4B }, 0) },

            [".txt"]  = new[] { (new byte[] { 0xEF, 0xBB, 0xBF }, 0) },
            [".pdf"]  = new[] { (new byte[] { 0x25, 0x50, 0x44, 0x46 }, 0) },

            [".7z"]   = new[] { (new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, 0) },
            [".zip"]  = new[] { (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0) },
            [".rar"]  = new[] { (new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A }, 0) },

            [".mkv"]  = new[] { (new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, 0) },
            [".mka"]  = new[] { (new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, 0) },
            [".mp4"]  = new[] { (new byte[] { 0x66, 0x74, 0x79, 0x70 }, 4) },

            [".webp"] = new[] { (new byte[] { 0x52, 0x49, 0x46, 0x46 }, 0), (new byte[] { 0x57, 0x45, 0x42, 0x50 }, 8) },
            [".png"]  = new[] { (new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0) },
            [".jpg"]  = new[] { (new byte[] { 0xFF, 0xD8, 0xFF }, 0) },
            [".jpeg"] = new[] { (new byte[] { 0xFF, 0xD8, 0xFF }, 0) },
            //https://github.com/bitsgalore/jp2kMagic
            [".jp2"]  = new[] { (new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20 }, 0) },
            [".j2c"]  = new[] { (new byte[] { 0xFF, 0x4F, 0xFF, 0x51, 0x00, 0x2F, 0x00, 0x00 }, 0) },
        };

        private static readonly int MaxLength;

        static FileHeader()
        {
            foreach (var tuplese in Header)
            {
                foreach (var tuple in tuplese.Value)
                {
                    MaxLength = Math.Max(MaxLength, tuple.offset + tuple.signature.Length);
                }
            }
        }

        public static bool Check(string path)
        {
            var ext = Path.GetExtension(path)?.ToLower() ?? "";
            if (ext == "") return false;//no ext file should not exists here
            if (!Header.ContainsKey(ext)) return true;//for file not in list, supposed as valid file
            var headers = Header[ext];
            using (var stream = File.OpenRead(path))
            {
                if (stream.Length == 0) return true;//empty file is accepted
                var ret = true;
                foreach (var header in headers)
                {
                    if (stream.Length < header.signature.Length + header.offset)
                    {
                        Logger.Log(Logger.Level.Error, $"{path}: Too short to contain file signature");
                        return false;
                    }
                    stream.Seek(header.offset, SeekOrigin.Begin);
                    var bytes = stream.ReadBytes(header.signature.Length);
                    ret &= bytes.SequenceEqual(header.signature);
                    if (ret) continue;
                    Logger.Log(Logger.Level.Error, $"{path}: Expected signature->[{header.signature.ToHex()}]({header.offset}), actual signature->[{bytes.ToHex()}]({header.offset})");
                    Logger.Log(Logger.Level.Error, $"{path}: Actual extension should be: {stream.MatchSignature() ?? "unknow"}");
                    break;
                }
                return ret;
            }
        }

        public static string ToHex(this byte[] bytes)
        {
            return bytes.Aggregate("", (current, item) => current + $"{item:X2} ").TrimEnd();
        }

        public static string MatchSignature(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            if (stream.Length < MaxLength) return null;
            var bytes = stream.ReadBytes(MaxLength);
            foreach (var tuplese in Header)
            {
                var valid = true;
                foreach (var tuple in tuplese.Value)
                {
                    var sub = new byte[tuple.signature.Length];
                    Array.Copy(bytes, tuple.offset, sub, 0, tuple.signature.Length);
                    valid &= sub.SequenceEqual(tuple.signature);
                }
                if (valid) return tuplese.Key;
            }
            return null;
        }
    }
}