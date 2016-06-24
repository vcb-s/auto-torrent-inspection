using System;
using System.Drawing;
using System.Reflection;
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

        private readonly TorrentData _data;

        private Node _node = new Node();


        public TreeViewForm(TorrentData data)
        {
            _data = data;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }

        private delegate int InsertNodeHandle(TreeNode node);

        private void TreeViewForm_Load(object sender, EventArgs e)
        {
            Text = _data.TorrentName;
            TreeNode treenode = new TreeNode(_data.TorrentName);
            long length = 0;
            var task = new Task(() =>
            {
                _node = new Node(_data.GetRawFileListWithAttribute());
                length = _node.InsertTo(treenode.Nodes);
            });
            task.ContinueWith(t =>
            {
                Invoke(new InsertNodeHandle(treeView1.Nodes.Add), treenode);

                Invoke(new Action(treenode.Expand), null);
                if (length == 0) return;
                Invoke(new Action(() => Text += $" [{FileSize.FileSizeToString(length)}]"), null);
                Invoke(new Action(() => treenode.Text += $" [{FileSize.FileSizeToString(length)}]"), null);
            });
            task.Start();
        }
    }
}
