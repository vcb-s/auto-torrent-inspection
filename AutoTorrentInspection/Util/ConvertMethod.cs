using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AutoTorrentInspection.Util
{
    public static class ConvertMethod
    {
        private static KeyValuePair<string,IEnumerable<string>>  EnumerateFolder(string path)
        {
            var fileList = new List<string>(Directory.GetFiles(path).ToList().Select(item => item.Substring(path.Length + 1, item.Length - path.Length - 1)));
            var folderQueue = new Queue<string>();
            foreach (var item in Directory.GetDirectories(path))
            {
                folderQueue.Enqueue(item);
            }
            while (folderQueue.Count > 0)
            {
                var currentFolder = folderQueue.Dequeue();
                fileList.AddRange(Directory.GetFiles(currentFolder).ToList().Select(item=>item.Substring(path.Length + 1, item.Length - path.Length - 1)));
                foreach (var item in Directory.GetDirectories(currentFolder))
                {
                    folderQueue.Enqueue(item);
                }
            }
            return new KeyValuePair<string, IEnumerable<string>>(path, fileList);
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
                if (!fileDic.ContainsKey(category))
                {
                    fileDic.Add(category, new List<FileDescription>());
                }
                var fd = new FileDescription
                {
                    FileName = Path.GetFileName(file),
                    Path = category == "root" ? "" : file.Substring(0, slashPosition),
                    Ext = Path.GetExtension(file).ToLower(),
                    Length = new FileInfo(rawList.Key + "\\" + file).Length
                };
                fd.CheckValid();
                fileDic[category].Add(fd);
            }
            return fileDic;
        }
    }
}