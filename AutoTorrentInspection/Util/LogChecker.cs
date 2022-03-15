using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    // Source: https://github.com/puddly/eac_logsigner
    public class LogChecker
    {
        public static class Core
        {
            private static byte[] FromHexString(string hex)
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }

            private static string ToHexString(byte[] ba)
            {
                StringBuilder hex = new StringBuilder(ba.Length * 2);
                foreach (byte b in ba)
                    hex.AppendFormat("{0:x2}", b);
                return hex.ToString();
            }


            private static string compute_checksum(string input_string)
            {
                input_string = input_string.Replace("\n", "").Replace("\r", "");
                var utf16_array = Encoding.Unicode.GetBytes(input_string);
                var key = FromHexString("9378716cf13e4265ae55338e940b376184da389e50647726b35f6f341ee3efd9");
                var cipher = new Rijndael(key, 256 / 8);
                var signature = FromHexString("0000000000000000000000000000000000000000000000000000000000000000");
                for (var i = 0; i < utf16_array.Length; i += 32)
                {
                    var plaintext_block = new byte[32];
                    var length = Math.Min(utf16_array.Length - i, 32);
                    Array.Copy(utf16_array, i, plaintext_block, 0, length);
                    for (var j = 0; j < 32; ++j)
                    {
                        plaintext_block[j] ^= signature[j];
                    }
                    signature = cipher.encrypt(plaintext_block);
                }
                return ToHexString(signature);
            }

            private static IEnumerable<(string, string, string)> extract_infos(string text)
            {
                return Regex.Split(text, new string('-', 60)).Select(extract_info);
            }

            private static (string unsigned_text, string version, string old_signature) extract_info(string text)
            {
                var version = "";
                var ret = Regex.Match(text, "Exact Audio Copy [^\r\n]+");
                if (ret.Success)
                {
                    version = ret.Value;
                }

                var signatures = Regex.Matches(text, "====.* ([0-9A-F]{64}) ====");
                if (signatures.Count == 0)
                    return (text, version, "");
                // get last signature
                var signature = signatures[signatures.Count - 1].Groups[1].Value;
                var fullLine = signatures[signatures.Count - 1].Value;

                var unsignedText = text.Replace(fullLine, "");
                return (unsignedText, version, signature);

            }

            public static List<(string version, string old_signature, string actual_signature)> eac_verify(string text)
            {
                var ret = new List<(string, string, string)>();

                foreach (var (unsignedText, version, oldSignature) in extract_infos(text))
                {
                    ret.Add((version, oldSignature.ToUpper(), compute_checksum(unsignedText).ToUpper()));
                }

                return ret;
            }
        }
    }
}
