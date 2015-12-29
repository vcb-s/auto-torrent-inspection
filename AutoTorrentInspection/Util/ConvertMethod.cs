using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection.Util
{
    public class ConvertMethod
    {

        private static IEnumerable<string> EnumerateFolder(string path)
        {
            var fileList = new List<string>(Directory.GetFiles(path));
            var folderQueue = new Queue<string>();
            foreach (var item in Directory.GetDirectories(path))
            {
                folderQueue.Enqueue(item);
            }
            while (folderQueue.Count > 0)
            {
                var currentFolder = folderQueue.Dequeue();
                fileList.AddRange(Directory.GetFiles(currentFolder));
                foreach (var item in Directory.GetDirectories(currentFolder))
                {
                    folderQueue.Enqueue(item);
                }
            }
            return fileList;
        }

        public static Dictionary<string, List<FileDescription>> GetFileList(string folderPath)
        {
            var fileDic = new Dictionary<string, List<FileDescription>>();
            var fileList = EnumerateFolder(folderPath).ToList();
            if (fileList.Count == 0) return fileDic;
            fileDic.Add("folder", new List<FileDescription>());
            foreach (var file in fileList)
            {
                var length = new FileInfo(file).Length;
                var fileName = Path.GetFileName(file);
                var fileExt = Path.GetExtension(file)?.ToLower();
                fileDic["folder"].Add(new FileDescription
                {
                    FileName = fileName,
                    Path = file,
                    Ext = fileExt,
                    Category = "folder",
                    Length = length,
                });
            }
            return fileDic;
        }
    }
}