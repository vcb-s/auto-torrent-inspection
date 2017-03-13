using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using AutoTorrentInspection.Util;
using System.Collections.Generic;
using AutoTorrentInspection.Properties;


namespace AutoTorrentInspection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AddCommand();
        }

        public Form1(string args)
        {
            InitializeComponent();
            AddCommand();
            FilePath = args;
            try
            {
                Debug.Assert(FilePath != null);
                if (Path.GetExtension(FilePath).ToLower() != ".torrent" && !Directory.Exists(FilePath))
                {
                    Notification.ShowInfo(@"无效的路径");
                    Environment.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Notification.ShowError(@"Exception catched in Form constructor", exception);
                Environment.Exit(0);
            }
            LoadFile(FilePath);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = $@"Auto Torrent Inspection v{Assembly.GetExecutingAssembly().GetName().Version}";
            RegistryStorage.Save(Application.ExecutablePath);
            RegistryStorage.RegistryAddCount(@"Software\AutoTorrentInspection\Statistics", @"count");
            Updater.CheckUpdateWeekly("AutoTorrentInspection");
        }

        private SystemMenu _systemMenu;

        private void AddCommand()
        {
            _systemMenu = new SystemMenu(this);
            _systemMenu.AddCommand("检查更新(&U)", Updater.CheckUpdate, true);
            _systemMenu.AddCommand("关于(&A)", () => { new FormAbout().Show(); }, false);
        }

        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);

            // Let it know all messages so it can handle WM_SYSCOMMAND
            // (This method is inlined)
            _systemMenu.HandleMessage(ref msg);
        }

        private string FilePath
        {
            get { return _paths[0]; }
            set { _paths[0] = value; }
        }
        private string[] _paths = new string[20];
        private TorrentData _torrent;
        private Dictionary<string, List<FileDescription>> _data;
        private IEnumerable<KeyValuePair<long, IEnumerable<FileDescription>>> _sizeData;
        private HashSet<string> _fonts;
        private AssFonts _assFonts;

        private bool _isUrl;

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            _isUrl = false;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                _isUrl = true;
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (_isUrl)
            {
                string url = e.Data.GetData("Text") as string;
                Debug.WriteLine(url ?? "null");
                if (string.IsNullOrEmpty(url) || !url.ToLower().EndsWith(".torrent"))
                {
                    return;
                }
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        string filePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()+".torrent");
                        wc.DownloadFileCompleted += LoadFile;
                        wc.DownloadFileAsync(new Uri(url), filePath);
                        FilePath = filePath;
                        return;
                    }
                    catch(Exception exception)
                    {
                        Notification.ShowError(@"种子文件下载失败", exception);
                        FilePath = string.Empty;
                        return;
                    }
                }
            }
            _paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (string.IsNullOrEmpty(FilePath)) return;
            if (Path.GetExtension(FilePath).ToLower() != ".torrent" && !Directory.Exists(FilePath)) return;
            LoadFile(FilePath);
        }

        private void btnLoadFile_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                LoadFile(openFileDialog1.FileName);
            }
            else if (e.Button == MouseButtons.Right)
            {
                new TreeViewForm().Show();
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (_data == null) return;
            dataGridView1.Rows.Clear();
            Application.DoEvents();
            dataGridView1.SuspendDrawing(() => Inspection(cbCategory.Text));
        }

        private const string CurrentTrackList = "http://208.67.16.113:8000/annonuce\n" +
                                                "udp://208.67.16.113:8000/annonuce\n" +
                                                "udp://tracker.openbittorrent.com:80/announce\n"+
                                                "http://t.acg.rip:6699/announce";

        private void btnAnnounceList_Click(object sender, EventArgs e)
        {
            if (_torrent == null)
            {
                new Thread(() =>
                {
                    _assFonts = new AssFonts();
                    var ass = _data.SelectMany(item => item.Value).Where(file => file.Extension == ".ass");
                    foreach (var file in ass) _assFonts.FeedSubtitle(file.FullPath);
                    _fonts = _assFonts.UsedFonts;
                    string context = string.Empty;
                    foreach (var item in _fonts.ToList().OrderBy(i => i)) context += item + "\n";
                    if (string.IsNullOrEmpty(context)) return;
                    context.ShowWithTitle("Fonts used in subtitles");
                }).Start();
                return;
            }
            var combineList = string.Join("\n", _torrent.GetAnnounceList());
            var currentRuler = combineList == CurrentTrackList;
            combineList.ShowWithTitle($@"Tracker List == {currentRuler}");
        }

        private void LoadFile(object sender, AsyncCompletedEventArgs e)
        {
            LoadFile(FilePath);
        }

        private readonly string[] _loadingText = {
            "正在重新校正什么来着",
            "正在打蜡、除蜡",
            "正在树立威望",
            "正在纸上谈兵",
            "正在蓄势待发",
            "正在勇敢梦想",
            "正在试着装忙",
            "正在抽丝剥茧",
            "正在更换灯泡",
            "正在加油打气",
            "正在挑拨离间",
            "正在推向极限",
            "耐心就是美德",
        };

        private void LoadFile(string filepath)
        {
            btnWebP.Visible = btnWebP.Enabled = false;
            btnCompare.Visible = btnCompare.Enabled = false;
            _sizeData = null;
            _torrent  = null;
            btnRefresh.Enabled = true;
            
            try
            {
                toolStripStatusLabel_Status.Text = _loadingText[new Random().Next() % _loadingText.Length];
                Application.DoEvents();
                if (Directory.Exists(filepath))
                {
                    _data = ConvertMethod.GetFileList(filepath);
                    btnAnnounceList.Enabled = true;
                    btnAnnounceList.Text = "Fonts";
                    btnTreeView.Visible = btnTreeView.Enabled = false;
                    cbFixCue.Enabled = true;
                    _sizeData = FileSizeDuplicateInspection();
                    if (_sizeData?.Any() ?? false) btnCompare.Visible = btnCompare.Enabled = true;
                    InspecteOperation();
                    return;
                }
                _torrent = new TorrentData(filepath);
                _data    = _torrent.GetFileList();
                btnAnnounceList.Enabled = true;
                btnAnnounceList.Text = "Tracker";
                btnTreeView.Visible = btnTreeView.Enabled = true;
                cbFixCue.Enabled = false;

                if (_sizeData == null) _sizeData = FileSizeDuplicateInspection();
                if (_sizeData?.Any() ?? false) btnCompare.Visible = btnCompare.Enabled = true;

                if (_torrent.IsPrivate)
                {
                    Notification.ShowInfo(@"This torrent has been set as a private torrent");
                }
                if (!string.IsNullOrEmpty(_torrent.Comment) || !string.IsNullOrEmpty(_torrent.Source))
                {
                    $@"Comment: {_torrent.Comment ?? "无可奉告"}{Environment.NewLine}Source: {_torrent.Source}"
                        .ShowWithTitle("Comment/Source");
                }
                InspecteOperation();
            }
            catch (Exception exception)
            {
                Notification.ShowError("Exception catched in LoadFile", exception);
            }
        }

        private void InspecteOperation()
        {
            if (_data.Any(catalog => catalog.Value.Any(item => item.Extension == ".ass")))
            {
                if (_data.ContainsKey("root"))
                {
                    if (!_data["root"].Any(item => item.FileName.ToLower().Contains("font")))
                    {
                        Notification.ShowInfo($"发现ass格式字幕\n但未在根目录发现字体包");
                    }
                }
            }
            if (_data.Any(catalog => catalog.Value.Any(item => item.Extension == ".webp")))
            {
                const string webpReadMe = "readme about WebP.txt";
                if (_data.ContainsKey("root"))
                {
                    var readme = _data["root"].Where(item => item.FileName == webpReadMe).ToList();
                    var show = false;
                    if (!readme.Any())//no readme found
                    {
                        Notification.ShowInfo($"发现WebP格式图片\n但未在根目录发现{webpReadMe}");
                        show = _torrent == null;//create the txt
                    }
                    else if(_torrent == null)// found and in folder mode
                    {
                        try
                        {
                            var path = readme.First().FullPath;
                            if (readme.First().Length != 1186 || File.ReadAllText(path) != Resources.ReadmeAboutWebP)
                            {
                                Notification.ShowInfo($"{webpReadMe}的内容在报道上出现了偏差");
                                show = true;//rewrite the txt
                            }
                        }
                        catch (Exception exception)
                        {
                            Notification.ShowError($"读取{webpReadMe}失败", exception);
                        }
                    }
                    else
                    {
                        if (readme.First().Length != 1186)
                        {
                            Notification.ShowInfo($"{webpReadMe}的内容在报道上出现了偏差");
                        }
                    }
                    btnWebP.Visible = btnWebP.Enabled = show;
                }
            }
            ThroughInspection();
            cbCategory.Enabled = cbCategory.Items.Count > 1;
        }

        private IEnumerable<KeyValuePair<long, IEnumerable<FileDescription>>> FileSizeDuplicateInspection()
        {
            foreach (var sizePair in _data.Values.SelectMany(i => i).GroupBy(i => i.Length))
            {
                foreach (var files in sizePair.GroupBy(i => i.Extension).SkipWhile(i => i.Count() <= 1))
                {
                    yield return new KeyValuePair<long, IEnumerable<FileDescription>>(sizePair.Key, files);
                }
            }
        }

        private void ThroughInspection()
        {
            dataGridView1.Rows.Clear();
            cbCategory.Items.Clear();
            Application.DoEvents();
            dataGridView1.SuspendDrawing(() =>
            {
                foreach (var item in _data.Keys)
                {
                    cbCategory.Items.Add(item);
                    Inspection(item);
                }
            });
            if (cbCategory.Items.Count > 0)
            {
                cbCategory.SelectedIndex = cbCategory.SelectedIndex == -1 ? 0 : cbCategory.SelectedIndex;
            }
            DateTime time = DateTime.Now;
            try {
                time = _torrent?.CreationDate ?? new DirectoryInfo(FilePath).LastWriteTime;
            } catch { /* ignored */ }
            Text = $@"Auto Torrent Inspection v{Assembly.GetExecutingAssembly().GetName().Version} - " +
                   $@"{_torrent?.TorrentName ?? FilePath} - By [{_torrent?.CreatedBy ?? "Folder"}] - " +
                   $@"{_torrent?.Encoding ?? "UND"} - {time}";
        }

        private void Inspection(string category)
        {
            Func<FileDescription, bool> check = item => item.State != FileState.ValidFile || cbShowAll.Checked;
            //dataGridView1.Rows.AddRange(_data[category].Where(item => check(item)).Select(r => r.ToRow()).ToArray());
            //Application.DoEvents();
            foreach (var item in _data[category].Where(item => check(item)))
            {
                dataGridView1.Rows.Add(item.ToRow());
                Application.DoEvents();
            }
            toolStripStatusLabel_Status.Text = dataGridView1.Rows.Count == 0 ? "状态正常, All Green"
                : $"发现 {dataGridView1.Rows.Count} 个世界的扭曲点{(cbShowAll.Checked ? "(并不是)" : "")}";
        }

        private void btnWebP_Click(object sender, EventArgs e)
        {
            string txtpath = Path.Combine(FilePath, "readme about WebP.txt");
            if (MessageBox.Show(@"是否在根目录生成 readme about WebP.txt", @"ATI Tips", MessageBoxButtons.YesNo)
                != DialogResult.Yes) return;
            try
            {
                File.WriteAllText(txtpath, Resources.ReadmeAboutWebP);
                btnWebP.Visible = btnWebP.Enabled = false;
            }
            catch (Exception exception)
            {
                Notification.ShowError(@"生成失败", exception);
            }
        }

        private void cbCategory_MouseEnter(object sender, EventArgs e) => toolTip1.Show(cbCategory.Text, cbCategory);

        private void cbCategory_MouseLeave(object sender, EventArgs e) => toolTip1.Hide(cbCategory);

        private void cbFixCue_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("如果编码检测置信度比较低的时候, \n请使用其他工具修正, 爆炸几率有点大。\n另, 注意删除bak文件。", cbFixCue);
        }

        private void cbFixCue_MouseLeave(object sender, EventArgs e) => toolTip1.Hide(cbFixCue);

        private bool _fixing;

        private void CueFix(FileDescription fileInfo, int rowIndex)
        {
            _fixing = true;
            Debug.WriteLine($"CueFix: GridView[R = {rowIndex}]");
            if (rowIndex < 0) return;

            Debug.Assert(fileInfo != null);
            if (fileInfo.Extension.ToLower() != ".cue") return;
            var confindence = fileInfo.Confidence;

            switch (fileInfo.State)
            {
                case FileState.InValidEncode:
                {
                    var dResult = MessageBox.Show(caption: @"来自TC的提示", buttons: MessageBoxButtons.YesNo,
                        text: $"该cue编码不是UTF-8, 是否尝试修复?\n注: 有{(confindence > 0.6 ? confindence > 0.9 ? "极小" : "小" : "大")}概率失败, 此时请检查备份。");
                    if (dResult == DialogResult.Yes)
                    {
                        CueCurer.MakeBackup(fileInfo.FullPath);
                        var originContext = EncodingConverter.GetStringFrom(fileInfo.FullPath, fileInfo.Encode);
                        EncodingConverter.SaveAsEncoding(originContext, fileInfo.FullPath, Encoding.UTF8);
                        fileInfo.RecheckCueFile(dataGridView1.Rows[rowIndex]);
                    }
                }
                break;
                case FileState.InValidCue:
                {
                    var dResult = MessageBox.Show(caption: @"来自TC的提示", buttons: MessageBoxButtons.YesNo,
                        text: $"该cue内文件名与实际文件不相符, 是否尝试修复?\n注: 非常规编码可能无法正确修复, 此时请检查备份。");
                    if (dResult == DialogResult.Yes)
                    {
                        CueCurer.MakeBackup(fileInfo.FullPath);
                        var originContext = EncodingConverter.GetStringFrom(fileInfo.FullPath, fileInfo.Encode);
                        var directory = Path.GetDirectoryName(fileInfo.FullPath);
                        var editedContext = CueCurer.FixFilename(originContext, directory);
                        EncodingConverter.SaveAsEncoding(editedContext, fileInfo.FullPath, Encoding.UTF8);
                        fileInfo.RecheckCueFile(dataGridView1.Rows[rowIndex]);
                    }
                }
                break;
            }
        }

        private string _filePosition = string.Empty;

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            FileDescription fileInfo = dataGridView1.Rows[e.RowIndex].Tag as FileDescription;
            if (fileInfo == null)  return;
            var confindence = fileInfo.Confidence;
            toolStripStatusLabel_Encode.Text = $@"{fileInfo.Encode}({confindence:F2})";
            Application.DoEvents();
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (cbFixCue.Checked)
                    {
                        CueFix(fileInfo, e.RowIndex);
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
            Process.Start("Explorer.exe", $"/select,\"{_filePosition}\"");
            _filePosition = string.Empty;
        }

        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_filePosition)) return;
            Process.Start($"\"{_filePosition}\"");
            _filePosition = string.Empty;
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (_fixing || dataGridView1.SelectedCells.Count != 1) return;
            Debug.WriteLine($"{e.KeyCode} - {dataGridView1.SelectedCells[0].RowIndex}");
            var rowIndex = dataGridView1.SelectedCells[0].RowIndex;
            FileDescription fileInfo = dataGridView1.Rows[rowIndex].Tag as FileDescription;
            if (fileInfo == null) return;

            var confindence = fileInfo.Confidence;
            toolStripStatusLabel_Encode.Text = $@"{fileInfo.Encode}({confindence:F2})";
            Application.DoEvents();
            if (cbFixCue.Checked && e.KeyCode == Keys.Enter)
            {
                CueFix(fileInfo, rowIndex);
                _fixing = false;
            }
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            new FormFileDup(_sizeData).Show();
            /*
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            var torrent = new TorrentData(openFileDialog1.FileName);

            //var fileList = torrent.GetRawFileList();
            var node = new Node(torrent.GetRawFileListWithAttribute());
            var cmpResult = CheckConsistency(node, FilePath);

            if (cmpResult.Result.ResultType == CheckResult.ResultTypeEnum.Normal)
            {
                int tsum = torrent.GetFileList().Values.Sum(item => item.Count);
                int fsum = _data.Values.Sum(item => item.Count);
                Notification.ShowInfo(tsum == fsum ? @"种子与文件夹内容完全一致" : $"文件夹中比种子内多 {fsum - tsum} 个文件");
            }
            else
            {
                Notification.ShowInfo($"First unmatched File: {cmpResult.Result.FileName}\nError Type: {cmpResult.Result.ResultType}");
            }
            */
        }

        private class CheckResult
        {
            public enum ResultTypeEnum
            {
                Normal,
                Exists,
                Size
            }

            public string FileName { get; set; }
            public ResultTypeEnum ResultType { get; set; }

        }

        private static async Task<CheckResult> CheckConsistency(Node node, string baseDirectory)
        {
            foreach (var directory in node.GetDirectories())
            {
                var result = await CheckConsistency(directory, baseDirectory);
                if (result.ResultType != CheckResult.ResultTypeEnum.Normal)
                {
                    return result;
                }
            }
            var masterRet = new CheckResult { FileName = node.NodeName, ResultType = CheckResult.ResultTypeEnum.Normal };
            foreach (var f in node.GetFiles().Select(file => new KeyValuePair<string, FileSize>(baseDirectory + file.FullPath, file.Attribute)))
            {
                var ret = new CheckResult { FileName = Path.GetFileName(f.Key), ResultType = CheckResult.ResultTypeEnum.Normal};
                if (!File.Exists(f.Key))
                {
                    ret.ResultType = CheckResult.ResultTypeEnum.Exists;
                }
                else
                {
                    var length = new FileInfo(f.Key).Length;
                    if (length != f.Value.Length)
                    {
                        ret.ResultType = CheckResult.ResultTypeEnum.Size;
                    }
                }
                if (ret.ResultType != CheckResult.ResultTypeEnum.Normal)
                {
                    return ret;
                }
            }
            return masterRet;
        }

        private void btnTreeView_Click(object sender, EventArgs e)
        {
            if (_torrent == null) return;
            var frm = new TreeViewForm(_torrent);
            frm.Show();
        }
    }
}
