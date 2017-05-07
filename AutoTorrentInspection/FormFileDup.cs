using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTorrentInspection.Util;
using AutoTorrentInspection.Util.Crc32.NET;

namespace AutoTorrentInspection
{
    public partial class FormFileDup: Form
    {
        public FormFileDup()
        {
            InitializeComponent();
        }

        public FormFileDup(IEnumerable<(long, IEnumerable<FileDescription>)> sizeData)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            GetCRCAsync(sizeData);
        }

        private async void GetCRCAsync(IEnumerable<(long length, IEnumerable<FileDescription> files)> sizeData)
        {
            var ret = sizeData.Select(size => new
            {
                filesize = size.length,
                files = size.files.Select(item => new
                {
                    crc  = Crc32Algorithm.FileCRC(item.FullPath),
                    info = item
                })
            });
            TreeNodeCollection treenode = treeView1.Nodes;
            try {
                foreach (var size in ret)
                {
                    Debug.WriteLine(size.filesize + ":");
                    var node = new TreeNode(FileSize.FileSizeToString(size.filesize));
                    var tmp = new List<uint>();
                    foreach (var file in size.files)
                    {
                        uint crc = await file.crc;
                        tmp.Add(crc);
                        if (crc == 0) node.Nodes.Add(file.info.ReletivePath + file.info.FileName);
                        else
                        {
                            node.Nodes.Add($"[{crc:X}] " + file.info.FullPath);
                            Debug.WriteLine($"{file.info.FileName} ||| crc: {crc:X}");
                        }
                    }
                    var valid = tmp.Distinct().Count() != tmp.Count;
                    if (valid) treenode.Add(node);
                }
                treeView1.Sort();
                treeView1.ExpandAll();
            }
            catch (Exception exception)
            {
                Notification.ShowError("Exception catched in GetCRCAsync", exception);
                Close();
            }
        }
    }
}
