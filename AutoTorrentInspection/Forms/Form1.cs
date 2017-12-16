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
using System.Threading.Tasks;
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
                Logger.Log(exception);
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
            FormLog formLog = null;
            _systemMenu.AddCommand("显示日志(&L)", () =>
            {
                if (formLog == null) formLog = new FormLog();
                formLog.Show();
            }, true);
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
                Logger.Log(Logger.Level.Debug, $"url: {url}");
                if (string.IsNullOrEmpty(url) || !url.ToLower().EndsWith(".torrent"))
                {
                    return;
                }
                using (var wc = new System.Net.WebClient())
                {
                    try
                    {
                        var filePath = Path.GetTempFileName()+".torrent";
                        FilePath = filePath;
                        wc.DownloadFileCompleted += LoadFile;
                        wc.DownloadFileAsync(new Uri(url), filePath);
                        FilePath = filePath;
                        return;
                    }
                    catch(Exception exception)
                    {
                        Logger.Log(exception);
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
            Logger.Log($"Refreshed, switch to '{cbCategory.Text}'");
        }

        private readonly string _currentTrackerList = string.Join("\n\n", GlobalConfiguration.Instance().TrackerList);

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
                new Task(() =>
                {
                    var fonts = GetUsedFonts().ToList();
                    if (fonts.Count == 0) return;
                    Logger.Log($"Fonts used in subtitles: {string.Join(", ", fonts)}");
                    string.Join(Environment.NewLine, fonts).ShowWithTitle("Fonts used in subtitles");
                }).Start();
                return;
            }

            var trackerList = string.Join("\n", _torrent.RawAnnounceList.Select(list => list.Aggregate(string.Empty, (current, url) => $"{current}{url}\n"))).TrimEnd().EncodeControlCharacters();
            var currentRule = trackerList == _currentTrackerList;
            var opeMap = new[] {"- ", "  ", "+ "};
            var content = ConvertMethod.GetDiff(trackerList, _currentTrackerList)
                .Aggregate(string.Empty, (current, item) => current + $"{opeMap[item.ope + 1]}{item.text}{Environment.NewLine}");
            Logger.Log(content);
            content.ShowWithTitle($@"Tracker List == {currentRule}");
        }

        private void LoadFile(object sender, AsyncCompletedEventArgs e)
        {
            Task.Factory.StartNew(() => LoadFile(FilePath));
        }

        private readonly string[] _loadingText = {
            "正在...更加用力地深思熟虑",
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
            "正在理清头绪",
            "正在除旧布新",
            "正在准备发射",
        };

        private readonly Random _random = new Random();

        private void LoadFile(string filepath)
        {
            Logger.Log($"{new string('=', 10)}Begin{new string('=', 10)}");
            btnWebP.Visible = btnWebP.Enabled = false;
            btnCompare.Visible = btnCompare.Enabled = false;
            _sizeData = null;
            _torrent  = null;
            btnRefresh.Enabled = true;

            try
            {
                toolStripStatusLabel_Status.Text = _loadingText[_random.Next(0, _loadingText.Length - 1)];
                Application.DoEvents();
                if (Directory.Exists(filepath))
                {
                    Logger.Log("Fetch file info");
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
                Logger.Log("Fetch torrent info");
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
                    new Task(() => Notification.ShowInfo(@"This torrent has been set as a private torrent")).Start();
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
                Logger.Log(exception);
                Notification.ShowError("Exception catched in LoadFile", exception);
                toolStripStatusLabel_Status.Text = exception.Message;
            }
        }

        [Flags]
        enum WebpState
        {
            Default          = -1,
            Zero             = 0,
            One              = 1,
            TwoOrMore        = 2,
            NotInRoot        = 4,
            IncorrectContent = 8,
            ReadFileFailed   = 16,
            EmptyInRoot      = 32
        }

        private void InspecteOperation()
        {
            if (_data.Any(catalog => catalog.Value.Any(item => item.Extension == ".ass")))
            {
                if (_data.ContainsKey("root"))
                {
                    if (!_data["root"].Any(item => item.FileName.ToLower().Contains("font")))
                    {
                        new Task(() => Notification.ShowInfo("发现ass格式字幕\n但未在根目录发现字体包")).Start();
                    }
                }
            }

            if (!GlobalConfiguration.Instance().InspectionOptions.WebPPosition) goto SKIP_WEBP;

            var webpState = WebpState.Default;
            const string webpReadMe = "readme about WebP.txt";
            Exception resultException = null;
            if (_data.Any(catalog => catalog.Value.Any(item => item.Extension == ".webp")))
            {
                var readmeCount = _data.SelectMany(pair => pair.Value).Count(item => item.FileName == webpReadMe);
                if (readmeCount == 0)
                {
                    webpState = WebpState.Zero;
                    goto EXIT_WEBP;
                }
                if (readmeCount == 1)     webpState = WebpState.One;
                else if (readmeCount > 1) webpState = WebpState.TwoOrMore;
                if (_data.TryGetValue("root", out var rootFiles))
                {
                    var readmeInRoot = rootFiles.Where(item => item.FileName == webpReadMe).ToList();
                    if (readmeInRoot.Count == 0)
                    {
                        webpState |= WebpState.NotInRoot;
                        //possible state:
                        //1. exists   but not in root
                        //2. multiple but not in root
                    }
                    else
                    {
                        Debug.Assert(readmeInRoot.Count == 1);
                        var readmeFile = readmeInRoot.First();
                        try
                        {
                            if (readmeFile.Length != 1186 || _torrent == null &&
                                File.ReadAllText(readmeFile.FullPath) != Resources.ReadmeAboutWebP)
                            {
                                webpState |= WebpState.IncorrectContent;
                                //possible state:
                                //1. one | incorrect
                                //2. two | incorrect
                            }
                        }
                        catch (Exception exception)
                        {
                            Logger.Log(exception);
                            resultException = exception;
                            webpState |= WebpState.ReadFileFailed;
                        }
                    }
                }
                else
                {
                    webpState |= WebpState.EmptyInRoot;
                    //todo: only folders in root path, check each of these
                }
                EXIT_WEBP:
                btnWebP.Visible = btnWebP.Enabled = webpState == WebpState.Zero;
                new Task(() =>
                {
                    switch (webpState)
                    {
                        case WebpState.Zero:
                            Notification.ShowInfo($"发现WebP格式图片\n但未在根目录发现{webpReadMe}");
                            break;
                        case WebpState.One:
                            break;
                        case WebpState.TwoOrMore:
                            Notification.ShowInfo($"存在复数个{webpReadMe}，但根目录下的报道没有偏差");
                            break;
                        case WebpState.One | WebpState.IncorrectContent:
                            Notification.ShowInfo($"{webpReadMe}的内容在报道上出现了偏差");
                            break;
                        case WebpState.TwoOrMore | WebpState.IncorrectContent:
                            Notification.ShowInfo($"存在复数个{webpReadMe}，并且根目录下的报道还出现了偏差\n现时请手工检查");
                            break;
                        case WebpState.One | WebpState.ReadFileFailed:
                            Notification.ShowError($"读取{webpReadMe}失败", resultException);
                            break;
                        case WebpState.TwoOrMore | WebpState.ReadFileFailed:
                            Notification.ShowError($"存在复数个{webpReadMe}，并且根目录下的读取失败\n请根据给定的异常进行排查", resultException);
                            break;
                        case WebpState.NotInRoot | WebpState.One:
                            Notification.ShowInfo($"{webpReadMe}处于非根目录\n似乎不大对路，现时请手工递归检查");
                            break;
                        case WebpState.NotInRoot | WebpState.TwoOrMore:
                            Notification.ShowInfo($"存在非根目录复数个{webpReadMe}\n似乎不大对路，现时请手工递归检查");
                            break;
                        case WebpState.EmptyInRoot | WebpState.One:
                            Notification.ShowInfo($"根目录为空并且{webpReadMe}处于非根目录\n似乎不大对路，现时请手工递归检查");
                            break;
                        case WebpState.EmptyInRoot | WebpState.TwoOrMore:
                            Notification.ShowInfo($"根目录为空并且存在复数个{webpReadMe}处于非根目录\n现时请手工递归检查");
                            break;
                        default:
                            throw new Exception($"webp state: \"{webpState}\", unknow combination");
                    }
                }).Start();
            }

            SKIP_WEBP:
            ThroughInspection();
            if (!GlobalConfiguration.Instance().InspectionOptions.CDNaming) goto SKIP_CD;
            CDInspection();
            SKIP_CD:
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
                (string name, int ord) tuple =  match.Success ? (name, int.Parse(match.Groups["ord"].Value)) : ("", -1);
                return tuple;
            }).Where(file => file.ord != -1).GroupBy(file => file.name);
            foreach (var group in data)
            {
                var arr = group.Select(file => file.ord).OrderBy(i=>i).Distinct().ToList();
                int begin = arr.First(), length = arr.Count;
                if (begin > 1 || !arr.SequenceEqual(Enumerable.Range(begin, length)))
                {
                    yield return group.First().name;
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
                    Logger.Log($"Inspection for {item}");
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
            {
                Logger.Log($"index missing: {string.Join(", ", ret)}");
                string.Join(Environment.NewLine, ret).ShowWithTitle("以下可能存在序号乱写的嫌疑");
            }
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

        private void CDInspection()
        {
            if (!_data.ContainsKey("CDs"))
            {
                Logger.Log("No 'CDs' found under root folder");
                return;
            }
            var INVALID_CD_FOLDER = Color.FromArgb(int.Parse(GlobalConfiguration.Instance().RowColor.INVALID_CD_FOLDER, System.Globalization.NumberStyles.HexNumber));
            var pat = new Regex(GlobalConfiguration.Instance().Naming.Pattern.CD);

            dataGridView1.Rows.AddRange(_data["CDs"].Select(Split).Distinct().Where(NotMatchPattern).Select(ToRow).ToArray());

            string Split(FileDescription file)
            {
                var path = file.ReletivePath;
                var beginIndex = path.IndexOf('\\') + 1;
                var endIndex = path.IndexOf('\\', beginIndex);
                if (endIndex == -1)
                    return path.Substring(beginIndex);
                return path.Substring(beginIndex, endIndex - beginIndex);
            }

            bool NotMatchPattern(string folder)
            {
                Logger.Log($"Progress: '{folder}'");
                return !pat.IsMatch(folder);
            }

            DataGridViewRow ToRow(string folder)
            {
                var row = new DataGridViewRow();
                row.Cells.AddRange(new DataGridViewTextBoxCell {Value = folder});
                row.DefaultCellStyle.BackColor = INVALID_CD_FOLDER;
                return row;
            }
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
                Logger.Log(exception);
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
                        text: "该cue内文件名与实际文件不相符, 是否尝试修复?\n注: 非常规编码可能无法正确修复, 此时请检查备份。");
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

        private DataGridViewRow _rowUnderMouse;

        private FileDescription _fileInfo => _rowUnderMouse?.Tag as FileDescription;

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _rowUnderMouse = dataGridView1.Rows[e.RowIndex];
            if (!(_rowUnderMouse.Tag is FileDescription fileInfo)) return;
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
                    break;
            }
        }

        private void OpenFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileInfo == null) return;
            Process.Start("Explorer.exe", $"/select,\"{_fileInfo.FullPath}\"");
            _rowUnderMouse = null;
        }

        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileInfo == null) return;
            Process.Start($"\"{_fileInfo.FullPath}\"");
            _rowUnderMouse = null;
        }

        private void DeleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileInfo == null) return;
            var category = cbCategory.Text == "root" ? _data.First(list => list.Value.Contains(_fileInfo)).Key : null;
            File.Delete(_fileInfo.FullPath);
            if (category != null) _data[category].Remove(_fileInfo);
            dataGridView1.Rows.Remove(_rowUnderMouse);
            Application.DoEvents();
            _rowUnderMouse = null;
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (_fixing || dataGridView1.SelectedCells.Count != 1) return;
            Debug.WriteLine($"{e.KeyCode} - {dataGridView1.SelectedCells[0].RowIndex}");
            var rowIndex = dataGridView1.SelectedCells[0].RowIndex;
            if (!(dataGridView1.Rows[rowIndex].Tag is FileDescription fileInfo)) return;

            var confindence = fileInfo.Confidence;
            toolStripStatusLabel_Encode.Text = $@"{fileInfo.Encode}({confindence:F2})";
            Application.DoEvents();
            if (cbFixCue.Checked && e.KeyCode == Keys.Enter)
            {
                CueFix(fileInfo, rowIndex);
                _fixing = false;
            }
        }

        private void btnCompare_Click(object sender, EventArgs e) => new FormFileDup(_sizeData).Show();

        private void btnTreeView_Click(object sender, EventArgs e)
        {
            if (_torrent == null) return;
            var frm = new TreeViewForm(_torrent);
            frm.Show();
        }
    }
}
