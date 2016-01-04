using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
            if (fileList.Count == 0) return fileDic;
            foreach (var file in fileList)
            {
                var slashPosition = file.LastIndexOf("\\", StringComparison.Ordinal);
                var category = slashPosition > -1 ? file.Substring(0, slashPosition) : "root";
                fileDic.TryAdd(category, new List<FileDescription>());
                string fullPath = $"{rawList.Key}\\{file}";
                fileDic[category].Add(FileDescription.CreateWithCheckFile(Path.GetFileName(file),
                                                    category == "root" ? "" : file.Substring(0, slashPosition),
                                                    Path.GetExtension(file).ToLower(),
                                                    fullPath));
            }
            return fileDic;
        }

        /// <summary>
        /// Determines wether a text file is encoded in UTF by analyzing its context.
        /// </summary>
        /// <param name="filePath">The text file to analyze.</param>
        public static bool IsUTF8(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            if (bytes.Length <= 0) return false;
            int asciiOnly = 1, continuationBytes = 0;
            foreach (var item in bytes)
            {
                if ((sbyte)item < 0) asciiOnly = 0;
                if (continuationBytes != 0)
                {
                    if ((item & 0xC0) != 0x80u)
                        return false;
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
            return continuationBytes == 0 && asciiOnly == 0;
        }



        /// <summary>
        /// 将指定集合的元素添加到 <see cref= "T:System.Collections.Generic.Queue`1"/> 的结尾处。
        /// </summary>
        /// <param name="queue">用作要添加的目标 <see cref= "T:System.Collections.Generic.Queue`1"/>。</param>  <param name="source">应将其元素添加到 <see cref= "T:System.Collections.Generic.Queue`1"/> 的结尾的集合。</param>
        private static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> source)
        {
            foreach (var item in source)
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
