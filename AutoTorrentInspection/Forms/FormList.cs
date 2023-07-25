using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AutoTorrentInspection.Forms
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>")]
    public partial class FormList : Form
    {
        public FormList(IEnumerable<string> fonts, IEnumerable<string> styles, IEnumerable<string> tags)
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            listView_font.Items.AddRange(Process(fonts));
            listView_style.Items.AddRange(Process(styles));
            listView_tag.Items.AddRange(Process(tags));
        }

        private static ListViewItem[] Process(IEnumerable<string> input)
        {
            return input
                .OrderBy(item => item)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => new ListViewItem(item))
                .ToArray();
        }
    }
}
