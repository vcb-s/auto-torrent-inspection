using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AutoTorrentInspection.Forms
{
    public partial class FormLog : Form
    {
        public FormLog()
        {
            InitializeComponent();
            InitForm();
        }

        private void InitForm()
        {
            Text = $"AutoTorrentInspection v{Assembly.GetExecutingAssembly().GetName().Version} -- Log";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
        }

        private void frmLog_Activated(object sender, EventArgs e)
        {
            txtLog.Text = Logger.MessagesText;
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.Select(txtLog.TextLength + 1, 0);
            txtLog.ScrollToCaret();
            grpLog.Text = $"Log ({txtLog.Lines.LongLength})";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Util.Notification.ShowError("Close", ex);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.UnicodeText, txtLog.SelectedText);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Util.Notification.ShowError("Copy", ex);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                txtLog.Text = Logger.MessagesText;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Util.Notification.ShowError("Refresh", ex);
            }
        }

        private void frmLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // To avoid getting disposed
            e.Cancel = true;
            Hide();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                if (Util.Notification.ShowQuestion("Are you sure you want to clear the log?", "Are you sure?") == DialogResult.Yes)
                {
                    Logger.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Util.Notification.ShowError("Clear", ex);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Title = "Select filename for log...",
                    CheckFileExists = true,
                    DefaultExt = "txt",
                    Filter = "*.txt|*.txt",
                    FileName = $"[{DateTime.Now:yyyy-MM-dd}][{DateTime.Now:HH-mm-ss}][ATI v{ Assembly.GetExecutingAssembly().GetName().Version }].txt"
                };
                if(sfd.ShowDialog() == DialogResult.Yes)
                {
                    using(var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        sw.Write(Logger.MessagesText);
                    }
                    Util.Notification.ShowInfo($"The log was saved to {sfd.FileName}!");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Util.Notification.ShowError("Save", ex);
            }
        }
    }
}
