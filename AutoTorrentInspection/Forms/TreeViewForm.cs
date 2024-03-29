﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTorrentInspection.Objects;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection.Forms
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>")]
    public partial class TreeViewForm : Form
    {
        public TreeViewForm()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        public TreeViewForm(TorrentData data)
        {
            _data = data;
            InitializeComponent();
            AddCommand();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private readonly TorrentData _data;

        private Node _node = new Node();

        private readonly Queue<TorrentData> _torrentQueue = new Queue<TorrentData>();

        private SystemMenu _systemMenu;

        private void AddCommand()
        {
            _systemMenu = new SystemMenu(this);
            _systemMenu.AddCommand("生成Json(&J)", () =>
            {
                Clipboard.SetText(_node.Json);
                Notification.ShowInfo("已复制至剪贴板");
            }, true);
            if (_data != null)
            {
                _systemMenu.AddCommand("生成磁力链接(&M)", () =>
                {
                    Clipboard.SetText(_data.MagnetLink);
                    Notification.ShowInfo("已复制至剪贴板");
                }, false);
            }
        }

        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);

            // Let it know all messages so it can handle WM_SYSCOMMAND
            // (This method is inlined)
            _systemMenu?.HandleMessage(ref msg);
        }

        private void TreeViewForm_Load(object sender, EventArgs e)
        {
            if (_data == null)
            {
                Text = $@"颜色含义：{KnownColor.PowderBlue}为甲有乙无/大小不一致，{KnownColor.PaleVioletRed}为甲无乙有";
                return;
            }
            Text = _data.TorrentName;
            var treenode = new TreeNode(_data.TorrentName);
            long length = 0;
            var task = new Task(() =>
            {
                _node = new Node(_data.GetRawFileListWithAttribute());
                length = _node.InsertTo(treenode.Nodes);
            });
            task.ContinueWith(t =>
            {
                Invoke(new Func<TreeNode, int>(treeView1.Nodes.Add), treenode);
                Invoke(new Action(treenode.Expand), null);
                if (length == 0) return;
                Invoke(new Action(() =>
                {
                    Text += $@" [{FileSize.FileSizeToString(length)}]";
                    treenode.Text += $@" [{FileSize.FileSizeToString(length)}]";
                }), null);
            });
            task.Start();
        }

        private void TreeViewForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void TreeViewForm_DragDrop(object sender, DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null || paths.Length == 0) return;
            if (string.IsNullOrEmpty(paths[0])) return;
            if (Path.GetExtension(paths[0]).ToLower() != ".torrent") return;
            if (_torrentQueue.Count >= 2) _torrentQueue.Dequeue();
            _torrentQueue.Enqueue(new TorrentData(paths[0]));
            if (_torrentQueue.Count == 2)
            {
                treeView1.Nodes.Clear();
                var tmp = _torrentQueue.ToArray();
                //var ret = ConvertMethod.GetDiffNode(tmp[0], tmp[1]);
                var ret = ConvertMethod.GetDiffNode(tmp[0], tmp[1]);
                ret.inANotInB.InsertTo(treeView1.Nodes, KnownColor.PowderBlue);
                ret.inBNotInA.InsertTo(treeView1.Nodes, KnownColor.PaleVioletRed);
            }
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e) => TreeViewForm_DragEnter(sender, e);

        private void treeView1_DragDrop(object sender, DragEventArgs e) => TreeViewForm_DragDrop(sender, e);

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var text = e.Node.Text;
            try { text = text.Substring(0, text.IndexOf('\ufeff')); }
            catch { /* ignored */ }
            Clipboard.SetText(text);
        }
    }
}
