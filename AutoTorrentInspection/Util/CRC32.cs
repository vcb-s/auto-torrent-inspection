/* Crc32.NET is Copyright (C) 2006 by its contributors (see: COPYRIGHT).
 *
 * Crc32.NET is free software; you can redistribute it and/or modify it under
 * the terms of version 2 of the GNU General Public License as published by
 * the Free Software Foundation.
 *
 * Crc32.NET is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this source code; if not, write to
 *
 *   Free Software Foundation, Inc.
 *   51 Franklin Street, Fifth Floor,
 *   Boston, MA  02110-1301  USA
 */

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