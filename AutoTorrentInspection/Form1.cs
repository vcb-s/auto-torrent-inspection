using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
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
        }

        private string FilePath
        {
            get { return _paths[0]; }
            set { _paths[0] = value; }
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
            if (string.IsNullOrEmpty(FilePath)) return;
            if (Path.GetExtension(FilePath).ToLower() != ".torrent" && !Directory.Exists(FilePath)) return;
            LoadFile(FilePath);
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

        private const string CurrentTrackList = "http://208.67.16.113:8000/annonuce\n" +
                                                "udp://208.67.16.113:8000/annonuce\n" +
                                                "udp://tracker.openbittorrent.com:80/announce\n"+
                                                "http://t.acg.rip:6699/announce";

        private void btnAnnounceList_Click(object sender, EventArgs e)
        {
            if (_torrent == null) return;
            var combineList = string.Join("\n", _torrent.GetAnnounceList());
            var currentRuler = combineList == CurrentTrackList;
            MessageBox.Show(text: combineList, caption: $"Tracker List == {currentRuler}");
        }

        private void LoadFile(string filepath)
        {
            _torrent = null;
            try
            {
                toolStripStatusLabel_Status.Text = @"读取并检查文件中…";
                Application.DoEvents();
                if (Directory.Exists(filepath))
                {
                    _data = ConvertMethod.GetFileList(filepath);
                    btnAnnounceList.Enabled = false;
                    goto Inspection;
                }
                _torrent = new TorrentData(filepath);
                _data    = _torrent.GetFileList();
                btnAnnounceList.Enabled = true;
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
            if (cbCategory.Items.Count > 0)
            {
                cbCategory.SelectedIndex = cbCategory.SelectedIndex == -1 ? 0 : cbCategory.SelectedIndex;
            }
            Text = $"Auto Torrent Inspection - {(_torrent != null?_torrent.TorrentName : FilePath)} - By [{_torrent?.CreatedBy ?? "folder"}] - {_torrent?.CreationDate}";
        }

        private void Inspection(string category)
        {
            foreach (var item in _data[category].Where(item => item.InValidFile || item.InValidCue || item.InValidEncode || cbShowAll.Checked))
            {
                dataGridView1.Rows.Add(item.ToRow());
                Application.DoEvents();
            }
            toolStripStatusLabel_Status.Text = dataGridView1.Rows.Count == 0
                ? "状态正常, All Green"
                : $"发现世界的扭曲点 {dataGridView1.Rows.Count} 个" + (cbShowAll.Checked ? "(并不是)" : "");
        }

        private void cbCategory_MouseEnter(object sender, EventArgs e) => toolTip1.Show(cbCategory.Text, cbCategory);

        private void cbCategory_MouseLeave(object sender, EventArgs e) => toolTip1.Hide(cbCategory);

        private void cbFixCue_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("如果编码检测置信度比较低的时候, \n请使用其他工具修正, 爆炸几率有点大。\n另, 注意删除bak文件。", cbFixCue);
        }

        private void cbFixCue_MouseLeave(object sender, EventArgs e) => toolTip1.Hide(cbFixCue);

        private void CueFix(FileDescription fileInfo, int rowIndex)
        {
            Debug.WriteLine($"GridView[R = {rowIndex}]");
            if (rowIndex < 0) return;

            Debug.Assert(fileInfo != null);
            if (fileInfo.Extension.ToLower() != ".cue") return;
            var confindence = fileInfo.Confidence;
            if (fileInfo.InValidEncode)
            {
                var dResult = MessageBox.Show(caption: @"来自TC的提示", buttons: MessageBoxButtons.OKCancel,
                    text:
                        $"该cue编码不是UTF-8, 是否尝试修复?\n注: 有" + (confindence > 0.6 ? "小" : "大") +
                        @"概率失败, 此时请检查备份。");
                if (dResult == DialogResult.OK)
                {
                    CueCurer.MakeBackup(fileInfo.FullPath);
                    var originContext = EncodingConverter.GetStringFrom(fileInfo.FullPath, fileInfo.Encode);
                    EncodingConverter.SaveAsEncoding(originContext, fileInfo.FullPath, "UTF-8");
                    fileInfo.RecheckCueFile(dataGridView1.Rows[rowIndex]);
                }
            }
            else if (fileInfo.InValidCue)
            {
                var dResult = MessageBox.Show(caption: @"来自TC的提示", buttons: MessageBoxButtons.OKCancel,
                    text: $"该cue内文件名与实际文件不相符, 是否尝试修复?\n注: 非常规编码可能无法正确修复, 此时请检查备份。");
                if (dResult == DialogResult.OK)
                {
                    CueCurer.MakeBackup(fileInfo.FullPath);
                    var originContext = EncodingConverter.GetStringFrom(fileInfo.FullPath, fileInfo.Encode);
                    var directory = Path.GetDirectoryName(fileInfo.FullPath);
                    var editedContext = CueCurer.FixFilename(originContext, directory);
                    EncodingConverter.SaveAsEncoding(editedContext, fileInfo.FullPath, "UTF-8");
                    fileInfo.RecheckCueFile(dataGridView1.Rows[rowIndex]);
                }
            }
        }

        private string _filePosition = string.Empty;

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            FileDescription fileInfo = dataGridView1.Rows[e.RowIndex].Tag as FileDescription;
            if (fileInfo == null)  return;
            var confindence = fileInfo.Confidence;
            toolStripStatusLabel_Encode.Text = fileInfo.Encode + (confindence > 0.99F ? "" : $"({confindence:F2})");
            Application.DoEvents();
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (cbFixCue.Checked)
                    {
                        CueFix(fileInfo,e.RowIndex);
                    }
                    break;
                case MouseButtons.Right:
                    if (fileInfo.SourceType != SourceTypeEnum.RealFile) return;
                    contextMenuOpenFolder.Show(MousePosition);
                    _filePosition = fileInfo.FullPath;
                    break;
            }
        }

        private void OpenFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_filePosition)) return;
            Process.Start("Explorer.exe", "/select," + $"\"{_filePosition}\"");
            _filePosition = string.Empty;
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            Debug.WriteLine($"{e.KeyCode} - {dataGridView1.SelectedCells[0].RowIndex}");

            if (dataGridView1.SelectedCells.Count != 1) return;
            var rowIndex = dataGridView1.SelectedCells[0].RowIndex;
            FileDescription fileInfo = dataGridView1.Rows[rowIndex].Tag as FileDescription;
            if (fileInfo == null) return;

            var confindence = fileInfo.Confidence;
            toolStripStatusLabel_Encode.Text = fileInfo.Encode + (confindence > 0.99F ? "" : $"({confindence:F2})");
            if (cbFixCue.Checked && e.KeyCode == Keys.Enter)
            {
                CueFix(fileInfo, rowIndex);
            }
        }


    }
}
