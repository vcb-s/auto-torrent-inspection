using System;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    public static class CRC32
    {
        private static readonly Regex CRC32Regex = new Regex(@"\[(?<CRC>[a-fA-F0-9]{8})\]\.");

        /// <summary>
        /// Get crc32 value in filename
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="crc32"></param>
        /// <returns></returns>
        public static bool FindCRC(string filePath, out uint crc32)
        {
            var result = CRC32Regex.Match(filePath);
            if (result.Success)
            {
                var crc = result.Groups["CRC"].Value;
                crc32 = (uint)Convert.ToInt64(crc, 16);
                return true;
            }
            crc32 = 0x00000000;
            return false;
        }

        public static bool IsCRCExsits(string filePath)
        {
            return CRC32Regex.Match(filePath).Success;
        }
    }
}