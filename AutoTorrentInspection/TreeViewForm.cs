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

        public TreeViewForm(TorrentData data)
        {
            _data = data;
            InitializeComponent();
            AddCommand();
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }

        private readonly TorrentData _data;

        private Node _node = new Node();

        private SystemMenu _systemMenu;

        private void AddCommand()
        {
            _systemMenu = new SystemMenu(this);
            _systemMenu.AddCommand("生成Json(&J)", () =>
                 {
                     Clipboard.SetText(_node.Json);
                     Notification.ShowInfo("已复制至剪贴板");
                 }, true);
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
                Invoke(new Func<TreeNode, int>(treeView1.Nodes.Add), treenode);
                Invoke(new Action(treenode.Expand), null);
                if (length == 0) return;
                Invoke(new Action(() =>
                {
                    Text += $" [{FileSize.FileSizeToString(length)}]";
                    treenode.Text += $" [{FileSize.FileSizeToString(length)}]";
                }), null);
            });
            task.Start();
        }
    }
}
