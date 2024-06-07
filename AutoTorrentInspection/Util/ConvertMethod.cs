using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AutoTorrentInspection.Objects;

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
            IEnumerable<string> SplitFunc(IEnumerable<string> array) => array.Select(item => item.Substring(folderPath.Length + 1));
            foreach (var file in SplitFunc(Directory.GetFiles(folderPath)))
            {
                yield return file;
            }
            var folderQueue = new Queue<string>();
            folderQueue.EnqueueRange(Directory.GetDirectories(folderPath));
            while (folderQueue.Count > 0)
            {
                var currentFolder = folderQueue.Dequeue();
                foreach (var file in SplitFunc(Directory.GetFiles(currentFolder)))
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
            folderPath = folderPath.TrimEnd('\\');
            var fileDic = new Dictionary<string, List<FileDescription>>();
            foreach (var file in EnumerateFolder(folderPath))
            {
                var slashPosition = file.IndexOf('\\');
                var category      = slashPosition != -1 ? file.Substring(0, slashPosition) : "root";
                var relativePath  = Path.GetDirectoryName(file);
                fileDic.TryAdd(category, new List<FileDescription>());
                fileDic[category].Add(new FileDescription(Path.GetFileName(file), relativePath, folderPath));
                Application.DoEvents();
            }
            return fileDic;
        }

        public static SeasonDir GetSeasonDirFileList(string folderPath, string subDirPath, int seasonId)
        {
            string currPath = Path.Combine(folderPath, subDirPath);
            DirectoryInfo info = new DirectoryInfo(currPath);
            Logger.Log($"currPath: {currPath}, name: {info.Name}");
            var fileDic = new SeasonDir(info.Name, seasonId);

            fileDic.SubDirs.TryAdd("root", new DirDescription(".", Path.Combine(subDirPath, "root"), folderPath));
            foreach (var dir in Directory.GetDirectories(currPath))
            {
                fileDic.SubDirs.TryAdd(dir.Substring(currPath.Length + 1), new DirDescription(dir.Substring(currPath.Length + 1), dir.Substring(folderPath.Length + 1), folderPath));
            }

            foreach (var file in EnumerateFolder(currPath))
            {
                var slashPosition = file.IndexOf('\\');
                var category      = slashPosition != -1 ? file.Substring(0, slashPosition) : "root";
                var relativePath  = Path.Combine(subDirPath, Path.GetDirectoryName(file) ?? "");

                Logger.Log($"file: {Path.GetFileName(file)}, relativePath: {relativePath}, folderPath: {folderPath}");
                fileDic.SubDirs[category].Files.Add(new FileDescription(Path.GetFileName(file), relativePath, folderPath));
                Application.DoEvents();
            }
            return fileDic;
        }

        public static SeriesDir GetSeriesDirFileList(string folderPath)
        {
            folderPath = folderPath.TrimEnd('\\');
            // 检测是系列目录还是季度目录
            bool isSeries = Directory.GetFiles(folderPath).Length == 0;

            DirectoryInfo info = new DirectoryInfo(folderPath);
            var fileDic = new SeriesDir(info.Name, isSeries);
            int seasonId = 0;

            if (isSeries)
            {
                foreach (var dir in Directory.GetDirectories(folderPath))
                {
                    Logger.Log($"search: {dir.Substring(folderPath.Length + 1)}");
                    fileDic.SeasonDirs.Add(GetSeasonDirFileList(folderPath, dir.Substring(folderPath.Length + 1), seasonId++));
                }
            }
            else
            {
                Logger.Log($"search: {folderPath}");
                fileDic.SeasonDirs.Add(GetSeasonDirFileList(folderPath, "", seasonId++));
            }

            Logger.Log($"series name: {fileDic.DirName}, isSeries: {fileDic.IsSeries}, validType: {fileDic.State}");
            foreach (var season in fileDic.SeasonDirs)
            {
                Logger.Log($"  season name: {season.DirName}, validType: {season.State}");
                foreach (var subdir in season.SubDirs)
                {
                    Logger.Log($"        subdir name: {subdir.Value.DirName}, validType: {subdir.Value.State}, fileNum: {subdir.Value.Files.Count}");
                    Logger.Log($"               relativePath: {subdir.Value.RelativePath}, basePath: {subdir.Value.BasePath}");
                }
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

        public static IEnumerable<(IEnumerable<string>, FileSize)> GetRawFolderFileListWithAttribute(string folderPath)
        {
            folderPath = Path.GetFullPath(folderPath).Trim('\\');
            foreach (var category in GetFileList(folderPath))
            {
                foreach (var item in category.Value)
                {
                    yield return (item.FullPath.Split('\\'), new FileSize(item.Length));
                }
            }
        }

        public static string EncodeControlCharacters(this string value)
        {
            var sb = new StringBuilder();
            foreach (var c in value)
            {
                if ((c <= 0x1F || c >= 0x7F && c <= 0x9F) && c != 0x0A)
                    sb.Append($"\\u{(int)c:x4}");
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private static int _(this int[,] mat, int i, int j)
        {
            if (i < 0) return 0;
            if (j < 0) return 0;
            return mat[i, j];
        }

        //https://en.wikipedia.org/wiki/Longest_common_subsequence_problem
        public static IEnumerable<(int ope, string text)> GetDiff(string lhs, string rhs, char separator = '\n')
        {
            string[] lList, rList;
            if (separator != '\0')
            {
                lList = lhs.Split(separator);
                rList = rhs.Split(separator);
            }
            else
            {
                lList = lhs.Select(c => new string(c, 1)).ToArray();
                rList = rhs.Select(c => new string(c, 1)).ToArray();
            }

            var mat = LCS(lList, rList);
            return PrintDiff(mat, lList, rList, lList.Length - 1, rList.Length - 1);

            int[,] LCS(string[] x, string[] y)
            {
                var c = new int[x.Length, y.Length];
                for (var i = 0; i < x.Length; ++i)
                for (var j = 0; j < y.Length; ++j)
                {
                    if (x[i] == y[j])
                        c[i, j] = c._(i - 1, j - 1) + 1;
                    else
                        c[i, j] = Math.Max(c._(i, j - 1), c._(i - 1, j));
                }
                return c;
            }

            IEnumerable<(int ope, string text)> PrintDiff(int[,] c, string[] x, string[] y, int i, int j)
            {
                IEnumerable<(int ope, string text)> inner = null;
                (int ope, string text) ret;
                if (i >= 0 && j >= 0 && x[i] == y[j])
                {
                    inner = PrintDiff(c, x, y, i - 1, j - 1);
                    ret = (0, x[i]);
                }
                else if (j >= 0 && (i == -1 || c._(i, j - 1) >= c._(i - 1, j)))
                {
                    inner = PrintDiff(c, x, y, i, j - 1);
                    ret = (1, y[j]);
                }
                else if (i >= 0 && (j == -1 || c._(i, j - 1) < c._(i - 1, j)))
                {
                    inner = PrintDiff(c, x, y, i - 1, j);
                    ret = (-1, x[i]);
                }
                else
                {
                    ret = (0, "");
                }
                if (inner != null) foreach (var item in inner)
                {
                    yield return item;
                }
                yield return ret;
            }
        }

        private class SetComparer : EqualityComparer<(IEnumerable<string>, FileSize)>
        {
            public override bool Equals((IEnumerable<string>, FileSize) x, (IEnumerable<string>, FileSize) y)
            {
                return x.Item2.Length == y.Item2.Length && x.Item1.SequenceEqual(y.Item1);
            }

            public override int GetHashCode((IEnumerable<string>, FileSize) obj)
            {
                return obj.Item1.Aggregate(obj.Item2.Length.GetHashCode(), (current, i) => current ^ i.GetHashCode());
            }
        }

        public static (Node inANotInB, Node inBNotInA) GetDiffNode(TorrentData torrent1, TorrentData torrent2)
        {
            var setComparer = new SetComparer();
            var set1 = new HashSet<(IEnumerable<string>, FileSize)>(torrent1.GetRawFileListWithAttribute(), setComparer);
            var set2 = new HashSet<(IEnumerable<string>, FileSize)>(torrent2.GetRawFileListWithAttribute(), setComparer);
            var disq = new HashSet<(IEnumerable<string>, FileSize)>(torrent1.GetRawFileListWithAttribute(), setComparer);
            disq.SymmetricExceptWith(set2);

            return (
                new Node(disq.Where(item => set1.Contains(item) && !set2.Contains(item))),
                new Node(disq.Where(item => set2.Contains(item) && !set1.Contains(item)))
                );
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
