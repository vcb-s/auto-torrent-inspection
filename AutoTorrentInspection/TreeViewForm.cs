using System;
using System.Windows.Forms;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection
{
    public partial class TreeViewForm : Form
    {
        public TreeViewForm()
        {
            InitializeComponent();
        }

        private readonly TorrentData _data;

        private readonly Node _node = new Node();

        private void InsertToView(Node currentNode, TreeNodeCollection tn = null)
        {
            if (tn == null)
            {
                tn = treeView1.Nodes;
            }
            var dir = currentNode.GetDirectories();
            var file = currentNode.GetFiles();
            foreach (var node in dir)
            {
                var treeNode = tn.Insert(tn.Count, node.NodeName);
                InsertToView(node, treeNode.Nodes);
            }
            foreach (var node in file)
            {
                tn.Insert(tn.Count, node.NodeName);
            }
        }


        public TreeViewForm(TorrentData data)
        {
            _data = data;
            InitializeComponent();
        }

        private void TreeViewForm_Load(object sender, EventArgs e)
        {
            Text = _data.TorrentName;
            var fileList = _data.GetRawFileList();
            foreach (var list in fileList)
            {
                _node.Insert(list);
            }
            InsertToView(_node);
        }
    }
}
