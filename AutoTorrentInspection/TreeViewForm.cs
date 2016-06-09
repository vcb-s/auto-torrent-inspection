using System;
using System.Drawing;
using System.Reflection;
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

        private Node _node = new Node();

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
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }

        private void TreeViewForm_Load(object sender, EventArgs e)
        {
            Text = _data.TorrentName;
            var fileList = _data.GetRawFileList();
            _node = new Node(fileList);
            InsertToView(_node);
        }
    }
}
