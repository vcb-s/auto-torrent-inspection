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
                var slashPosition = file.IndexOf("\\", StringComparison.Ordinal);
                var category = slashPosition > -1 ? file.Substring(0, slashPosition) : "root";
                fileDic.TryAdd(category, new List<FileDescription>());
                string fullPath = $"{rawList.Key}\\{file}";
                fileDic[category].Add(FileDescription.CreateWithCheck(Path.GetFileName(file),
                                                    category == "root" ? "" : file.Substring(0, slashPosition),
                                                    Path.GetExtension(file).ToLower(),
                                                    fullPath.Length > 256 ? 0L: new FileInfo(fullPath).Length));
            }
            return fileDic;
        }

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
