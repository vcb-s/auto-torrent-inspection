using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection
{
    public partial class FormFileDup: Form
    {
        private IEnumerable<KeyValuePair<long, List<FileDescription>>> _sizeData;

        public FormFileDup()
        {
            InitializeComponent();
        }

        public FormFileDup(IEnumerable<KeyValuePair<long, List<FileDescription>>> sizeData)
        {
            InitializeComponent();
            _sizeData = sizeData;
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        }
    }
}
