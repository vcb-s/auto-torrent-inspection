using Ude;
using System.IO;

namespace AutoTorrentInspection.Util
{
    public static class EncodingDetector
    {
        public static string GetEncoding(string filename, out float confindece)
        {
            confindece = 0.0f;
            using (var fs = File.OpenRead(filename))
            {
                ICharsetDetector cdet = new CharsetDetector();
                cdet.Feed(fs);
                cdet.DataEnd();
                if (cdet.Charset != null)
                {
                    Logger.Log($"Charset: {cdet.Charset}, confidence: {cdet.Confidence}");
                    confindece = cdet.Confidence;
                    return cdet.Charset;
                }
                Logger.Log($"{filename}: Detection failed.");
                return "UTF-8";
            }
        }

        // 0000 0000-0000 007F - 0xxxxxxx                   (ascii converts to 1 octet!)
        // 0000 0080-0000 07FF - 110xxxxx 10xxxxxx          ( 2 octet format)
        // 0000 0800-0000 FFFF - 1110xxxx 10xxxxxx 10xxxxxx ( 3 octet format)
        /// <summary>
        /// Determines wether a text file is encoded in UTF by analyzing its context.
        /// </summary>
        /// <param name="filePath">The text file to analyze.</param>
        public static bool IsUTF8(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            if (bytes.Length <= 0) return false;
            var asciiOnly = true;
            var continuationBytes = 0;
            foreach (var item in bytes)
            {
                if ((sbyte)item < 0) asciiOnly = false;
                if (continuationBytes != 0)
                {
                    if ((item & 0xC0) != 0x80u) return false;
                    --continuationBytes;
                }
                else
                {
                    if (item < 0x80u) continue;
                    var temp = item;
                    do
                    {
                        temp <<= 1;
                        ++continuationBytes;
                    } while ((sbyte)temp < 0);
                    --continuationBytes;
                    if (continuationBytes == 0) return false;
                }
            }
            return continuationBytes == 0 && !asciiOnly;
        }
    }
}
