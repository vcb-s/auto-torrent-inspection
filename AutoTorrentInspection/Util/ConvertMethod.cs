using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoTorrentInspection.Util
{
    public static class ConvertMethod
    {
        private static IEnumerable<string> EnumerateFolder(string folderPath)
        {
            Func<string[], IEnumerable<string>> splitFunc =
                array => array.Select(item => item.Substring(folderPath.Length + 1));
            foreach (var file in splitFunc(Directory.GetFiles(folderPath)))
            {
                yield return file;
            }
            var folderQueue = new Queue<string>();
            folderQueue.EnqueueRange(Directory.GetDirectories(folderPath));
            while (folderQueue.Count > 0)
            {
                var currentFolder = folderQueue.Dequeue();
                foreach (var file in splitFunc(Directory.GetFiles(currentFolder)))
                {
                    yield return file;
                }
                folderQueue.EnqueueRange(Directory.GetDirectories(currentFolder));
            }
        }

        public static Dictionary<string, List<FileDescription>> GetFileList(string folderPath)
        {
            string basePath = Path.GetDirectoryName(folderPath);
            var fileDic = new Dictionary<string, List<FileDescription>>();
            foreach (var file in EnumerateFolder(folderPath))
            {
                var categorySlashPosition = file.IndexOf("\\", StringComparison.Ordinal);
                var category              = categorySlashPosition > -1 ? file.Substring(0, categorySlashPosition) : "root";
                var pathSlashPosition     = file.LastIndexOf("\\", StringComparison.Ordinal);
                var relativePath          = category == "root" ? "" : file.Substring(0, pathSlashPosition);
                fileDic.TryAdd(category, new List<FileDescription>());
                fileDic[category].Add(new FileDescription(Path.GetFileName(file), relativePath, basePath, $"{folderPath}\\{file}"));
            }
            return fileDic;
        }

        public static string GetUTF8String(byte[] buffer)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 3) return Encoding.UTF8.GetString(buffer);
            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
            {
                return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
            }
            return Encoding.UTF8.GetString(buffer);
        }

        public static KeyValuePair <HashSet<KeyValuePair<IEnumerable<string>, FileSize>>, HashSet<KeyValuePair<IEnumerable<string>, FileSize>>> GetTorrentSet(TorrentData torrent1, TorrentData torrent2)
        {
            var ret1 = new HashSet<KeyValuePair<IEnumerable<string>, FileSize>>(torrent1.GetRawFileListWithAttribute());
            var ret2 = new HashSet<KeyValuePair<IEnumerable<string>, FileSize>>(torrent2.GetRawFileListWithAttribute());
            return new KeyValuePair<HashSet<KeyValuePair<IEnumerable<string>, FileSize>>, HashSet<KeyValuePair<IEnumerable<string>, FileSize>>> (ret1, ret2);
        }

        public static Dictionary<bool, List<KeyValuePair<IEnumerable<string>, FileSize>>> GetDiff(TorrentData torrent1, TorrentData torrent2)
        {
            var set = GetTorrentSet(torrent1, torrent2);
            var ret = new Dictionary<bool, List<KeyValuePair<IEnumerable<string>, FileSize>>>
            {
                [false] = new List<KeyValuePair<IEnumerable<string>, FileSize>>(),
                [true] = new List<KeyValuePair<IEnumerable<string>, FileSize>>()
            };
            foreach (var item in set.Key.Where(item => !set.Value.Contains(item))) ret[false].Add(item);//in torrent1, not in torrent2
            foreach (var item in set.Value.Where(item => !set.Key.Contains(item))) ret[true].Add(item);
            return ret;
        }

        // Only call GetFileWithLongPath() if the path is too long
        // ... otherwise, new FileInfo() is sufficient
        //source from http://stackoverflow.com/questions/12204186/error-file-path-is-too-long
        public static FileInfo GetFile(string path)
        {
            if (path.Length >= MAX_FILE_PATH)
            {
                return GetFileWithLongPath(path);
            }
            return new FileInfo(path);
        }

        private static int MAX_FILE_PATH = 260;
        private static int MAX_DIR_PATH  = 248;

        private static FileInfo GetFileWithLongPath(string path)
        {
            string[] subpaths = path.Split('\\');
            StringBuilder sbNewPath = new StringBuilder(subpaths[0]);
            // Build longest sub-path that is less than MAX_PATH characters
            for (int index = 1; index < subpaths.Length; index++)
            {
                if (sbNewPath.Length + subpaths[index].Length >= MAX_DIR_PATH)
                {
                    subpaths = subpaths.Skip(index).ToArray();
                    break;
                }
                sbNewPath.Append("\\" + subpaths[index]);
            }
            DirectoryInfo dir = new DirectoryInfo(sbNewPath.ToString());
            bool foundMatch = dir.Exists;
            if (!foundMatch) return null;// If we didn't find a match, return null;

            // Make sure that all of the subdirectories in our path exist.
            // Skip the last entry in subpaths, since it is our filename.
            // If we try to specify the path in dir.GetDirectories(),
            // We get a max path length error.
            int i = 0;
            while (i < subpaths.Length - 1 && foundMatch)
            {
                foundMatch = false;
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    if (subDir.Name == subpaths[i])
                    {
                        // Move on to the next subDirectory
                        dir = subDir;
                        foundMatch = true;
                        break;
                    }
                }
                i++;
            }
            if (!foundMatch) return null;

            // Now that we've gone through all of the subpaths, see if our file exists.
            // Once again, If we try to specify the path in dir.GetFiles(),
            // we get a max path length error.
            return dir.GetFiles().First(item => item.Name == subpaths[subpaths.Length - 1]);
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
        /// <param name="dictionary">用作要添加的键/值对的对象。</param><param name="key">用作要添加的元素的键的对象。</param><param name="value">用作要添加的元素的值的对象。</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> 为 null。</exception><exception cref="T:System.NotSupportedException"><see cref="T:System.Collections.Generic.IDictionary`2"/> 为只读。</exception>
        public static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key)) return;
            dictionary.Add(key, value);
        }

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        private const int WM_SETREDRAW = 11;

        public static void SuspendDrawing(this Control control, Action action)
        {
            SendMessage(control.Handle, WM_SETREDRAW, false, 0);
            action();
            SendMessage(control.Handle, WM_SETREDRAW, true, 0);
            control.Refresh();
        }
    }
}
