using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using AutoTorrentInspection.Properties;
using AutoTorrentInspection.Util;

namespace AutoTorrentInspection.Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AddCommand();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        public Form1(string args)
        {
            InitializeComponent();
            AddCommand();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
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
            _systemMenu.AddCommand("导出概要(&E)", ExportSummary, false);
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
            get => _paths[0];
            set => _paths[0] = value;
        }
        private string[] _paths = new string[20];
        private TorrentData _torrent;
        private Dictionary<string, List<FileDescription>> _data;
        private IEnumerable<(long, IEnumerable<FileDescription>)> _sizeData;

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
                var url = e.Data.GetData("Text") as string;
                Debug.WriteLine(url ?? "null");
                if (string.IsNullOrEmpty(url) || !url.ToLower().EndsWith(".torrent"))
                {
                    return;
                }
                using (var wc = new System.Net.WebClient())
                {
                    try
                    {
                        var filePath = Path.GetTempFileName()+".torrent";
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

        private const string CurrentTrackerList = "http://nyaa.tracker.wf:7777/announce\n\n" +
                                                  "http://208.67.16.113:8000/annonuce\n\n" +
                                                  "udp://208.67.16.113:8000/annonuce\n\n" +
                                                  "udp://tracker.openbittorrent.com:80/announce\n\n"+
                                                  "http://t.acg.rip:6699/announce";

        private IEnumerable<string> GetUsedFonts()
        {
            var assFonts = new AssFonts();
            assFonts.FeedSubtitle(_data.Values.SelectMany(_ => _).Where(file => file.Extension == ".ass").Select(file => file.FullPath));
            return assFonts.UsedFonts.OrderBy(i => i);
        }

        private void btnAnnounceList_Click(object sender, EventArgs e)
        {
            if (_torrent == null)
            {
                new Thread(() =>
                {
                    var context = string.Join("\n", GetUsedFonts());
                    if (string.IsNullOrEmpty(context)) return;
                    context.ShowWithTitle("Fonts used in subtitles");
                }).Start();
                return;
            }

            var trackerList = string.Join("\n", _torrent.RawAnnounceList.Select(list => list.Aggregate(string.Empty, (current, url) => $"{current}{url}\n"))).TrimEnd();
            var currentRule = trackerList == CurrentTrackerList;
            trackerList.ShowWithTitle($@"Tracker List == {currentRule}");
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
                toolStripStatusLabel_Status.Text = exception.Message;
            }
        }

        enum WebpState
        {
            Fine             = 0,
            NotFound         = 1,
            IncorrectContent = 2,
            ReadFileFailed   = 4,
            MultipleFiles    = 8,
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

            var webpState = WebpState.Fine;
            const string webpReadMe = "readme about WebP.txt";
            Exception resultException = null;
            if (_data.Any(catalog => catalog.Value.Any(item => item.Extension == ".webp")))
            {
                if (_data.TryGetValue("root", out var rootFiles))
                {
                    var readme = rootFiles.Where(item => item.FileName == webpReadMe).ToList();
                    if      (readme.Count == 0) webpState = WebpState.NotFound;
                    else if (readme.Count >  1) webpState = WebpState.MultipleFiles;
                    else
                    {
                        var readmeFile = readme.First();
                        if (readmeFile.Length != 1186) webpState = WebpState.IncorrectContent;
                        try
                        {
                            if (_torrent == null && webpState != WebpState.IncorrectContent &&
                                File.ReadAllText(readmeFile.FullPath) != Resources.ReadmeAboutWebP)
                            {
                                webpState = WebpState.IncorrectContent;
                            }
                        }
                        catch(Exception exception)
                        {
                            resultException = exception;
                            webpState = WebpState.ReadFileFailed;
                        }
                    }
                }
                else webpState = WebpState.NotFound;
                switch (webpState)
                {
                    case WebpState.Fine:
                        break;
                    case WebpState.NotFound:
                        Notification.ShowInfo($"发现WebP格式图片\n但未在根目录发现{webpReadMe}");
                        break;
                    case WebpState.IncorrectContent:
                        Notification.ShowInfo($"{webpReadMe}的内容在报道上出现了偏差");
                        break;
                    case WebpState.ReadFileFailed:
                        Notification.ShowError($"读取{webpReadMe}失败", resultException);
                        break;
                    case WebpState.MultipleFiles:
                        Notification.ShowInfo($"发现复数个{webpReadMe}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            ThroughInspection();
            cbCategory.Enabled = cbCategory.Items.Count > 1;
        }

        private IEnumerable<(long length, IEnumerable<FileDescription> files)> FileSizeDuplicateInspection()
        {
            //拍扁并按体积分组
            foreach (var sizePair in _data.Values.SelectMany(i => i).GroupBy(i => i.Length))
            {
                //再按后缀分组并跳过单个文件的
                foreach (var files in sizePair.GroupBy(i => i.Extension).SkipWhile(i => i.Count() <= 1))
                {
                    yield return (sizePair.Key, files);
                }
            }
        }

        private static readonly Regex FileOrderPattern = new Regex(@"^\[[^\[\]]*VCB\-S(?:tudio)*[^\[\]]*\] (?<name>[^\[\]]+)\[(?<type>[^\d]*)(?<ord>\d+)(?:v\d)?\]");

        private IEnumerable<string> FileOrderMissingInspection()
        {
            var data = _data.Values.SelectMany(i => i).Select(file =>
            {
                var match = FileOrderPattern.Match(file.FileName);
                var name = $"{file.ReletivePath}/{match.Groups["name"]}[{match.Groups["type"].Value}]{file.Extension}";
                return match.Success ? (name, int.Parse(match.Groups["ord"].Value)) : ("", -1);
            }).Where(file => file.Item2 != -1).GroupBy(file => file.Item1);
            foreach (var group in data)
            {
                var arr = group.Select(file => file.Item2).OrderBy(i=>i).Distinct().ToList();
                int begin = arr.First(), length = arr.Count;
                if (begin > 1 || !arr.SequenceEqual(Enumerable.Range(begin, length)))
                {
                    yield return group.First().Item1;
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

            var time = DateTime.Now;
            try
            {
                time = _torrent?.CreationDate ?? new DirectoryInfo(FilePath).LastWriteTime;
            }
            catch { /* ignored */ }
            var title = new List<string>
            {
                $"Auto Torrent Inspection v{Assembly.GetExecutingAssembly().GetName().Version}",
                $"{_torrent?.TorrentName ?? FilePath}",
                _torrent?.CreatedBy,
                _torrent?.Encoding,
                time.ToString(System.Globalization.CultureInfo.CurrentCulture),
                (_torrent?.PieceSize ?? 0) != 0 ? $"PieceSize: {_torrent?.PieceSize / 1024}KiB" : null
            }.Where(item => item != null);
            Text = string.Join(" - ", title);

            var ret = FileOrderMissingInspection().ToList();
            if (ret.Count != 0)
                string.Join("\n", ret).ShowWithTitle("以下可能存在序号乱写的嫌疑");
        }

        private void Inspection(string category)
        {
            Func<FileDescription, bool> filter = item => item.State != FileState.ValidFile || cbShowAll.Checked;
            //dataGridView1.Rows.AddRange(_data[category].Where(item => check(item)).Select(r => r.ToRow()).ToArray());
            //Application.DoEvents();
            foreach (var item in _data[category].Where(filter))
            {
                dataGridView1.Rows.Add(item.ToRow());
                Application.DoEvents();
            }
            toolStripStatusLabel_Status.Text = dataGridView1.Rows.Count == 0 ? "状态正常, All Green"
                : $"发现 {dataGridView1.Rows.Count} 个世界的扭曲点{(cbShowAll.Checked ? "(并不是)" : "")}";
        }

        private void btnWebP_Click(object sender, EventArgs e)
        {
            var txtpath = Path.Combine(FilePath, "readme about WebP.txt");
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
                        fileInfo.CueFileRevalidation(dataGridView1.Rows[rowIndex]);
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
                        fileInfo.CueFileRevalidation(dataGridView1.Rows[rowIndex]);
                    }
                }
                break;
            }
        }

        private string _filePosition = string.Empty;

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var fileInfo = dataGridView1.Rows[e.RowIndex].Tag as FileDescription;
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
            var fileInfo = dataGridView1.Rows[rowIndex].Tag as FileDescription;
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
        }

        private void btnTreeView_Click(object sender, EventArgs e)
        {
            if (_torrent == null) return;
            var frm = new TreeViewForm(_torrent);
            frm.Show();
        }

        private void ExportSummary()
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            using (var writer = new StreamWriter(File.OpenWrite(FilePath + ".md"), Encoding.UTF8))
            {
                writer.WriteLine("# Summary");
                writer.WriteLine($"## Source type: {(_torrent == null ? "Folder" : "Torrent")}");
                writer.WriteLine();

                if (_torrent != null)
                {
                    writer.WriteLine($"- TorrentName:\t{_torrent.TorrentName}\n" +
                                     $"- CreatedBy:\t{_torrent.CreatedBy}\n" +
                                     $"- IsPrivate:\t{_torrent.IsPrivate}");
                    writer.WriteLine();
                    var trackerList = string.Join("\n", _torrent.RawAnnounceList.Select(list => list.Aggregate(string.Empty, (current, url) => $"{current}{url}\n"))).TrimEnd();
                    writer.WriteLine($"- TrackerList:\t{trackerList == CurrentTrackerList}");
                    writer.WriteLine();
                    writer.WriteLine($"{new string('=', 20)}\n\n" +
                                     $"{trackerList}\n\n" +
                                     $"{new string('=', 20)}");
                    writer.WriteLine();
                }
                else
                {
                    writer.WriteLine($"- PathName:\t{FilePath}");
                    writer.WriteLine();
                    var fonts = string.Join("\n\t- ", GetUsedFonts());
                    if (!string.IsNullOrEmpty(fonts))
                    {
                        writer.WriteLine("- Fonts:");
                        writer.WriteLine("\t- " + fonts);
                        writer.WriteLine();
                    }
                }
                writer.WriteLine("## Doubtful files");
                writer.WriteLine();

                var rows = new Dictionary<FileState, List<FileDescription>>();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    var fileInfo = row.Tag as FileDescription;
                    if (fileInfo == null) continue;
                    if (!rows.ContainsKey(fileInfo.State))
                    {
                        rows[fileInfo.State] = new List<FileDescription>();
                    }
                    rows[fileInfo.State].Add(fileInfo);
                }
                foreach (var state in rows)
                {
                    writer.WriteLine($"### {state.Key}");
                    foreach (var info in state.Value)
                    {
                        writer.WriteLine($"- {info.FullPath}");
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}
