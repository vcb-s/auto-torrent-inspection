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
            foreach (var item in Directory.GetDirectories(folderPath))
            {
                folderQueue.Enqueue(item);
            }
            while (folderQueue.Count > 0)
            {
                var currentFolder = folderQueue.Dequeue();
                fileList.AddRange(splitFunc(Directory.GetFiles(currentFolder)));
                foreach (var item in Directory.GetDirectories(currentFolder))
                {
                    folderQueue.Enqueue(item);
                }
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
                if (!fileDic.ContainsKey(category))
                {
                    fileDic.Add(category, new List<FileDescription>());
                }
                string fullPath = $"{rawList.Key}\\{file}";
                fileDic[category].Add(FileDescription.CreateWithCheck(Path.GetFileName(file),
                                                    category == "root" ? "" : file.Substring(0, slashPosition),
                                                    Path.GetExtension(file).ToLower(),
                                                    fullPath.Length > 256 ? 0L: new FileInfo(fullPath).Length));
            }
            return fileDic;
        }
    }
}