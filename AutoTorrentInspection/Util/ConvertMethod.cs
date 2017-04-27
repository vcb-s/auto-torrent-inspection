using System;
using System.Collections;
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
        /// <summary>
        /// 枚举指定文件夹下的所有文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns>均为相对路径</returns>
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

        /// <summary>
        /// 获取并检查指定文件夹下所有文件
        /// </summary>
        /// <param name="folderPath">载入的文件夹的绝对路径</param>
        /// <returns></returns>
        public static Dictionary<string, List<FileDescription>> GetFileList(string folderPath)
        {
            folderPath = folderPath.Trim('\\');
            var fileDic = new Dictionary<string, List<FileDescription>>();
            foreach (var file in EnumerateFolder(folderPath))
            {
                var slashPosition = file.IndexOf('\\');
                var category      = slashPosition != -1 ? file.Substring(0, slashPosition) : "root";
                var relativePath  = Path.GetDirectoryName(file);
                fileDic.TryAdd(category, new List<FileDescription>());
                fileDic[category].Add(new FileDescription(Path.GetFileName(file), relativePath, folderPath));
            }
            return fileDic;
        }

        public static string GetUTFString(this byte[] buffer)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 3) return Encoding.UTF8.GetString(buffer);
            if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
            if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                return Encoding.Unicode.GetString(buffer);
            if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(buffer);
            return Encoding.UTF8.GetString(buffer);
        }

        public static IEnumerable<KeyValuePair<IEnumerable<string>, FileSize>> GetRawFolderFileListWithAttribute(string folderPath)
        {
            folderPath = Path.GetFullPath(folderPath).Trim('\\');
            foreach (var category in GetFileList(folderPath))
            {
                foreach (var item in category.Value)
                {
                    yield return new KeyValuePair<IEnumerable<string>, FileSize>(item.FullPath.Split('\\'), new FileSize(item.Length));
                }
            }
        }

        public static Dictionary<IEnumerable<string>, FileSize>[] GetTorrentSet(TorrentData torrent1, TorrentData torrent2)
        {
            var ret = new Dictionary<IEnumerable<string>, FileSize>[2];
            ret[0] = torrent1.GetRawFileListWithAttribute().ToDictionary(item => item.Key, item => item.Value);
            ret[1] = torrent2.GetRawFileListWithAttribute().ToDictionary(item => item.Key, item => item.Value);
            return ret;
        }

        public static Dictionary<IEnumerable<string>, FileSize>[] GetDiff(TorrentData torrent1, TorrentData torrent2)
        {
            var set = GetTorrentSet(torrent1, torrent2);
            var ret = new Dictionary<IEnumerable<string>, FileSize>[2];
            ret[0] = new Dictionary<IEnumerable<string>, FileSize>();
            ret[1] = new Dictionary<IEnumerable<string>, FileSize>();

            foreach (var item in set[0].Where(item =>
            {
                var suspicious = set[1].Where(tmp => tmp.Key.SequenceEqual(item.Key)).ToArray();
                if (!suspicious.Any()) return true;
                var suspiciousItem = suspicious.First();
                if (item.Value.Length != suspiciousItem.Value.Length) return true;
                return false;
            }))
                ret[0].Add(item.Key, item.Value);//in torrent1, not in torrent2
            foreach (var item in set[1].Where(item => !set[0].Any(tmp => tmp.Key.SequenceEqual(item.Key))))
                ret[1].Add(item.Key, item.Value);
            return ret;
        }

        public static KeyValuePair<Node, Node> GetDiffNode(TorrentData torrent1, TorrentData torrent2)
        {
            var ret = GetDiff(torrent1, torrent2);
            return new KeyValuePair<Node, Node>(new Node(ret[0]), new Node(ret[1]));
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
            var subpaths  = path.Split('\\');
            var newPathBuilder = new StringBuilder(subpaths.FirstOrDefault());
            // Build longest sub-path that is less than MAX_PATH characters
            int index;
            for (index = 1; index < subpaths.Length; ++index)
            {
                if (newPathBuilder.Length + subpaths[index].Length >= MAX_DIR_PATH)
                    break;
                newPathBuilder.Append("\\" + subpaths[index]);
            }
            var dir = new DirectoryInfo(newPathBuilder.ToString());
            var foundMatch = dir.Exists;

            // Make sure that all of the subdirectories in our path exist.
            // Skip the last entry in subpaths, since it is our filename.
            // If we try to specify the path in dir.GetDirectories(),
            // We get a max path length error.
            for (; index < subpaths.Length - 1 && foundMatch; ++index)
            {
                foundMatch = false;
                foreach (var subDir in dir.GetDirectories())
                {
                    if (subDir.Name != subpaths[index]) continue;
                    // Move on to the next subDirectory
                    dir = subDir;
                    foundMatch = true;
                    break;
                }
            }

            // Now that we've gone through all of the subpaths, see if our file exists.
            // Once again, If we try to specify the path in dir.GetFiles(),
            // we get a max path length error.
            return foundMatch ? dir.GetFiles().First(item => item.Name == subpaths[subpaths.Length - 1]) : null;
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
