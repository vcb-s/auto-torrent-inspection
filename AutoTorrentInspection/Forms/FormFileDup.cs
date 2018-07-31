using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTorrentInspection.Objects;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection.Forms
{
    public partial class FormFileDup: Form
    {
        private readonly CancellationTokenSource _cts;

        public FormFileDup()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            RemoveDupe(@"C:\Users\TautCony\PycharmProjects\CLC\FULL_TEST_CASE\2017");
        }

        public FormFileDup(IEnumerable<(long, IEnumerable<FileDescription>)> sizeData)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            _cts = new CancellationTokenSource();
            GetCRCAsync(sizeData, _cts.Token);
        }

        private async void RemoveDupe(string path)
        {
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Select(file => new FileInfo(file))
                .GroupBy(fi => fi.Length);
            foreach (var lengthGroup in files)
            {
                var length = lengthGroup.Key;
                Console.WriteLine(length);
                Console.WriteLine("-------------------");
                var group = new Dictionary<uint, List<FileInfo>>();
                foreach (var file in lengthGroup)
                {
                    var crc32 = await FileCRC32C(file.FullName);
                    if (!group.ContainsKey(crc32))
                    {
                        group[crc32] = new List<FileInfo>();
                    }
                    group[crc32].Add(file);
                }
                foreach (var pair in group)
                {
                    Console.WriteLine($"-------------{pair.Key:x8}-----------");
                    if (pair.Value.Count > 1)
                    {
                        foreach (var info in pair.Value.Skip(1))
                        {
                            //Console.WriteLine(info);
                            info.Delete();
                        }
                    }
                }
                Console.WriteLine("-------------------");
            }
        }

        private async void GetCRCAsync(IEnumerable<(long length, IEnumerable<FileDescription> files)> sizeData, CancellationToken token)
        {
            var ret = sizeData.Select(size => new
            {
                filesize = size.length,
                files = size.files.Select(item => new
                {
                    crc  = FileCRC32C(item.FullPath),
                    info = item
                })
            });
            var treenode = treeView1.Nodes;
            try
            {
                foreach (var size in ret)
                {
                    Logger.Log($"{size.filesize}:");
                    var node = new TreeNode(FileSize.FileSizeToString(size.filesize));
                    var tmp = new List<uint>();
                    foreach (var file in size.files)
                    {
                        token.ThrowIfCancellationRequested();
                        var crc = await file.crc;
                        tmp.Add(crc);
                        if (crc == 0) node.Nodes.Add(file.info.ReletivePath + file.info.FileName);
                        else
                        {
                            node.Nodes.Add($"[{crc:X}] {file.info.FullPath}");
                            Logger.Log($"{file.info.FileName} ||| CRC32C: {crc:X}");
                        }
                    }
                    var valid = tmp.Distinct().Count() != tmp.Count;
                    if (valid) treenode.Add(node);
                }
                if (treenode.Count == 0)
                    Notification.ShowInfo(@"没有出现雷同的文件");
            }
            catch (OperationCanceledException exception)
            {
                Logger.Log(exception);
            }
            catch (Exception exception)
            {
                Logger.Log(exception);
                Notification.ShowError("Exception catched in GetCRCAsync", exception);
            }
            finally
            {
                treeView1.Sort();
                treeView1.ExpandAll();
            }
        }

        private void FormFileDup_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts.Cancel();
        }

        /// <summary>
        /// Calculate file's CRC32C Value
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<uint> FileCRC32C(string filePath)
        {
            if (!File.Exists(filePath)) return 0;
            var currentCRC32C = 0U;
            const int capacity = 1024 * 1024;
            var buffer = new byte[capacity];
            using (var file = File.OpenRead(filePath))
            {
                int cbSize;
                do
                {
                    cbSize = await file.ReadAsync(buffer, 0, capacity).ConfigureAwait(false);
                    if (cbSize > 0) currentCRC32C = Crc32C.Crc32CAlgorithm.Append(currentCRC32C, buffer, 0, cbSize);
                } while (cbSize > 0);
                return currentCRC32C;
            }
        }
    }
}
