using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string[] _paths = new string[20];

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            _paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (string.IsNullOrEmpty(_paths?[0])) return;
            if (Directory.Exists(_paths?[0]))
            {
                _data = GetFileList(_paths[0]);
                ThroughInspection();
            }
            if (Path.GetExtension(_paths[0]).ToLower() != ".torrent") return;
            LoadFile(_paths[0]);
        }

        private static Dictionary<string, List<FileDescription>> GetFileList(string folderPath)
        {
            var fileDic = new Dictionary<string, List<FileDescription>>();
            var fileList = EnumerateFolder(folderPath).ToList();
            if (fileList.Count == 0) return fileDic;
            fileDic.Add("folder", new List<FileDescription>());
            foreach (var file in fileList)
            {
                var length = new FileInfo(file).Length;
                var fileName = Path.GetFileName(file);
                var fileExt = Path.GetExtension(file)?.ToLower();
                fileDic["folder"].Add(new FileDescription
                {
                    FileName = fileName,
                    Path = file,
                    Ext = fileExt,
                    Category = "folder",
                    Length = length,
                });
            }
            return fileDic;
        }

        private void LoadFile(string filepath)
        {
            _torrent = new TorrentData(filepath);
            _data = _torrent.GetFileList();
            ThroughInspection();
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
            Text = $"Auto Torrent Inspection - {(_torrent != null?_torrent.TorrentName : _paths[0])}";
        }

        private TorrentData _torrent;
        private Dictionary<string, List<FileDescription>> _data;

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            LoadFile(openFileDialog1.FileName);
        }
        private readonly Regex _partten = new Regex(@"^\[[^\[\]]*VCB-S(?:tudio)*[^\[\]]*\] [^\[\]]+ (\[.*\d*\])*\[((?<Ma>Ma10p_1080p)|(?<Hi>(Hi10p|Hi444pp)_(1080|720|480)p)|(?<EIGHT>(1080|720)p))\]\[((?<HEVC-Ma>x265)|(?<AVC-Hi>x264)|(?(EIGHT)x264))_\d*(flac|aac|ac3)\](?<SUB>(\.(sc|tc)|\.(chs|cht))*)\.((?(AVC)(mkv|mka|flac))|(?(HEVC)(mkv|mka|flac)|(?(EIGHT)mp4))|(?(SUB)ass))$");
        private readonly Regex _musicPartten = new Regex(@"\.(flac|tak|m4a|cue|log|jpg|jpeg|jp2)");

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (_data == null) return;
            dataGridView1.Rows.Clear();
            Inspection(cbCategory.Text);
        }

        private readonly List<string> _exceptExt = new List<string> {".rar", ".7z", ".zip"};

        private void Inspection(string category)
        {
            foreach (var item in _data[category])
            {
                item.Valid = (_exceptExt.IndexOf(item.Ext) <= 0 && !_partten.IsMatch(item.FileName) && !_musicPartten.IsMatch(item.FileName.ToLower()));
                if (item.Valid|| cbShowAll.Checked)
                {
                    Invoke(new AddRowDelegate(AddRow), item);
                }
            }
            cbState.CheckState = dataGridView1.Rows.Count == 1 ? CheckState.Checked : CheckState.Unchecked;
        }

        private delegate void AddRowDelegate(FileDescription item);

        private void AddRow(FileDescription item)
        {
            int index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells[0].Value = item.Path;
            dataGridView1.Rows[index].Cells[1].Value = item.FileName;
            dataGridView1.Rows[index].Cells[2].Value = $"{(double)item.Length / 1024:F3}KB";
            dataGridView1.Rows[index].DefaultCellStyle.BackColor = ColorTranslator.FromHtml(item.Valid?"#51A8DD":"#92AAF3");
        }

        private static IEnumerable<string> EnumerateFolder(string path)
        {
            var fileList = new List<string>(Directory.GetFiles(path));
            var folderQueue = new Queue<string>();
            foreach (var item in Directory.GetDirectories(path))
            {
                folderQueue.Enqueue(item);
            }
            while (folderQueue.Count > 0)
            {
                var currentFolder = folderQueue.Dequeue();
                fileList.AddRange(Directory.GetFiles(currentFolder));
                foreach (var item in Directory.GetDirectories(currentFolder))
                {
                    folderQueue.Enqueue(item);
                }
            }
            return fileList;
        }

        private void btnAnnounceList_Click(object sender, EventArgs e)
        {
            if (_torrent == null) return;
            MessageBox.Show(string.Join("\n", _torrent.GetAnnounceList()),@"Tracker List");
        }
    }
}
