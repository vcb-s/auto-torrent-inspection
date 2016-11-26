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

        public FormFileDup(IEnumerable<KeyValuePair<long, IEnumerable<FileDescription>>> sizeData)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            GetCRC(sizeData);
        }

        private async void GetCRC(IEnumerable<KeyValuePair<long, IEnumerable<FileDescription>>> sizeData)
        {
            var ret = sizeData.Select(size => new
            {
                filesize = size.Key,
                files = size.Value.Select(item => new
                {
                    crc  = Crc32Algorithm.FileCRC(item.FullPath),
                    file = item
                })
            });
            TreeNodeCollection treenode = treeView1.Nodes;
            foreach (var size in ret)
            {
                Debug.WriteLine(size.filesize + ":");
                var node = new TreeNode(FileSize.FileSizeToString(size.filesize));
                var tmp = new List<uint>();
                foreach (var file in size.files)
                {
                    uint crc = await file.crc;
                    tmp.Add(crc);
                    if (crc == 0) node.Nodes.Add(file.file.ReletivePath + file.file.FileName);
                    else
                    {
                        node.Nodes.Add($"[{crc:X}] " + file.file.FullPath);
                        Debug.WriteLine(file.file.FileName + " ||| crc: " + crc.ToString("X"));
                    }
                }
                var valid = tmp.Distinct().Count() != tmp.Count;
                if (valid) treenode.Add(node);
            }
            treeView1.Sort();
            treeView1.ExpandAll();
        }
    }
}
