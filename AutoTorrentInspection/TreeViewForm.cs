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


        public TreeViewForm(TorrentData data)
        {
            _data = data;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }

        private void TreeViewForm_Load(object sender, EventArgs e)
        {
            Text = _data.TorrentName;
            _node = new Node(_data.GetRawFileListWithAttribute());
            _node.InsertTo(treeView1.Nodes);
        }
    }
}
