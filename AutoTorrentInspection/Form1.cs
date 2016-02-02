using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //this.Icon = Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
        }

        private string[] _paths = new string[20];
        private TorrentData _torrent;
        private Dictionary<string, List<FileDescription>> _data;

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            _paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (string.IsNullOrEmpty(_paths?[0])) return;
            if (Path.GetExtension(_paths[0]).ToLower() != ".torrent" && !Directory.Exists(_paths[0])) return;
            LoadFile(_paths[0]);
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            LoadFile(openFileDialog1.FileName);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (_data == null) return;
            dataGridView1.Rows.Clear();
            Inspection(cbCategory.Text);
        }

        private void btnAnnounceList_Click(object sender, EventArgs e)
        {
            if (_torrent == null) return;
            MessageBox.Show(text: string.Join("\n", _torrent.GetAnnounceList()),caption: @"Tracker List");
        }

        private void LoadFile(string filepath)
        {
            _torrent = null;
            try
            {
                if (Directory.Exists(filepath))
                {
                    _data = ConvertMethod.GetFileList(filepath);
                    goto Inspection;
                }
                _torrent = new TorrentData(filepath);
                _data    = _torrent.GetFileList();
                if (_torrent.IsPrivate)
                {
                    MessageBox.Show(caption: @"ATI Warning",       text: @"This torrent has been set as a private torrent",
                                    buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Warning);
                }
                if (!string.IsNullOrEmpty(_torrent.Comment) || !string.IsNullOrEmpty(_torrent.Source))
                {
                    MessageBox.Show(caption: @"Comment/Source",
                                    text:    $"Comment: {_torrent.Comment ?? "无可奉告"}{Environment.NewLine}Source: {_torrent.Source}");
                }
                Inspection:
                ThroughInspection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(caption: @"ATI Warning",       text: $"Exception Message: \n\n    {ex.Message}",
                                buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Hand);
            }
        }

        private void ThroughInspection()
        {
            dataGridView1.Rows.Clear();
            cbCategory.Items.Clear();
            foreach (var item in _data.Keys)
            {
                cbCategory.Items.Add(item);
                Inspection(item);
            }
            cbCategory.SelectedIndex = cbCategory.SelectedIndex == -1 ? 0 : cbCategory.SelectedIndex;
            Text = $"Auto Torrent Inspection - {(_torrent != null?_torrent.TorrentName : _paths[0])} - By [{_torrent?.CreatedBy ?? "folder"}] - {_torrent?.CreationDate}";
        }

        private void Inspection(string category)
        {
            foreach (var item in _data[category].Where(item => item.InValidFile || item.InValidCue || item.InValidEncode || cbShowAll.Checked))
            {
                dataGridView1.Rows.Add(item.ToRow(dataGridView1));
            }
            cbState.CheckState = dataGridView1.Rows.Count == 0 ? CheckState.Checked : CheckState.Unchecked;
        }

        private void cbCategory_MouseEnter(object sender, EventArgs e) => toolTip1.Show(cbCategory.Text, cbCategory);

        private void cbCategory_MouseLeave(object sender, EventArgs e) => toolTip1.Hide(cbCategory);
    }
}
