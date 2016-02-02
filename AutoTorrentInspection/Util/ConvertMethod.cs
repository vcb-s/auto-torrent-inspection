using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    public static class ConvertMethod
    {
        private static KeyValuePair<string, IEnumerable<string>>  EnumerateFolder(string folderPath)
        {
            Func<string[], IEnumerable<string>> splitFunc =
                array => array.Select(item => item.Substring(folderPath.Length + 1, item.Length - folderPath.Length - 1));
            var fileList = new List<string>(splitFunc(Directory.GetFiles(folderPath)));
            var folderQueue = new Queue<string>();
            folderQueue.EnqueueRange(Directory.GetDirectories(folderPath));
            while (folderQueue.Count > 0)
            {
                var currentFolder = folderQueue.Dequeue();
                fileList.AddRange(splitFunc(Directory.GetFiles(currentFolder)));
                folderQueue.EnqueueRange(Directory.GetDirectories(currentFolder));
            }
            return new KeyValuePair<string, IEnumerable<string>>(folderPath, fileList);
        }

        public static Dictionary<string, List<FileDescription>> GetFileList(string folderPath)
        {
            var fileDic = new Dictionary<string, List<FileDescription>>();
            var rawList = EnumerateFolder(folderPath);
            var fileList = rawList.Value.ToList();
            if (!fileList.Any()) return fileDic;
            foreach (var file in fileList)
            {
                var categorySlashPosition = file.IndexOf("\\", StringComparison.Ordinal);
                var category = categorySlashPosition > -1 ? file.Substring(0, categorySlashPosition) : "root";
                var pathSlashPosition = file.LastIndexOf("\\", StringComparison.Ordinal);
                var relativePath = category == "root" ? "" : file.Substring(0, pathSlashPosition);
                fileDic.TryAdd(category, new List<FileDescription>());
                fileDic[category].Add(FileDescription.CreateWithCheckFile(Path.GetFileName(file), relativePath, $"{rawList.Key}\\{file}"));
            }
            return fileDic;
        }

        private static string GetUTF8String(byte[] buffer)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 3) return Encoding.UTF8.GetString(buffer);
            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
            {
                return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
            }
            return Encoding.UTF8.GetString(buffer);
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
            bool asciiOnly = true;
            int continuationBytes = 0;
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

        public static bool CueMatchCheck(FileDescription cueFile, bool utf8)
        {
            var cueContext = utf8 ? GetUTF8String(File.ReadAllBytes(cueFile.FullPath)) : File.ReadAllText(cueFile.FullPath, Encoding.Default);
            var rootPath   = Path.GetDirectoryName(cueFile.FullPath);
            var result     = true;
            foreach (Match audioName in Regex.Matches(cueContext, "FILE \"(?<fileName>.+)\" WAVE"))
            {
                var audioFile = $"{rootPath}\\{audioName.Groups["fileName"].Value}";
                result &= File.Exists(audioFile);
            }
            return result;
        }


        /// <summary>
        /// 将指定集合的元素添加到 <see cref= "T:System.Collections.Generic.Queue`1"/> 的结尾处。
        /// </summary>
        /// <param name="queue">用作要添加的目标 <see cref= "T:System.Collections.Generic.Queue`1"/>。</param>  <param name="source">应将其元素添加到 <see cref= "T:System.Collections.Generic.Queue`1"/> 的结尾的集合。</param>
        private static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> source)
        {
            foreach (T item in source)
            {
                queue.Enqueue(item);
            }
        }

        /// <summary>
        /// 在 <see cref="T:System.Collections.Generic.IDictionary`2"/> 中尝试添加一个带有所提供的键和值的元素。
        /// </summary>
        /// <param name="dictionary">用作要添加的键/值对的对象。</param><param name="key">用作要添加的元素的键的对象。</param><param name="value">用作要添加的元素的值的对象。</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> 为 null。</exception><exception cref="T:System.NotSupportedException"><see cref="T:System.Collections.Generic.IDictionary`2"/> 为只读。</exception>
        public static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key)) return;
            dictionary.Add(key, value);
        }
    }
}
