using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        TorrentData _data;

        Node _node = new Node();

        void InsertToView(Node currentNode, TreeNodeCollection tn = null)
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
            var fileList = _data.GetRawFileList();
            foreach (var list in fileList)
            {
                _node.Insert(list);
            }
            InsertToView(_node);
        }
    }
}
