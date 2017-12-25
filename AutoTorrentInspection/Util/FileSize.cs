using System;

namespace AutoTorrentInspection.Util
{
    public class FileSize
    {
        public long Length { get; private set; }

        private static readonly string[] SizeTail = { "B", "KB", "MB", "GB", "TB", "PB" };

        private static string _toString(long length)
        {
            var scale = length == 0 ? 0 : (int)Math.Floor(Math.Log(length, 1024));
            return $"{length / Math.Pow(1024, scale):F3}{SizeTail[scale]}";
        }

        public override string ToString() => _toString(Length);

        public static string FileSizeToString(long length) => _toString(length);

        public FileSize(long length)
        {
            Length = length;
        }
    }
}