using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AutoTorrentInspection.Forms
{
    public partial class FormLog : Form
    {
        private readonly StringBuilder _builder;

        public FormLog(StringBuilder builder)
        {
            _builder = builder;
            InitializeComponent();
            InitForm();
        }

        private void InitForm()
        {
            Text = $"AutoTorrentInspection v{Assembly.GetExecutingAssembly().GetName().Version} -- Log";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void frmLog_Activated(object sender, EventArgs e)
        {
            txtLog.Text = _builder.ToString();
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
            catch (Exception exception)
            {
                Logger.Log(exception);
                MessageBox.Show(exception.Message);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.UnicodeText, txtLog.SelectedText);
            }
            catch (Exception exception)
            {
                Logger.Log(exception);
                MessageBox.Show(exception.Message);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                txtLog.Text = _builder.ToString();
            }
            catch (Exception exception)
            {
                Logger.Log(exception);
                MessageBox.Show(exception.Message);
            }
        }

        private void frmLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // To avoid getting disposed
            e.Cancel = true;
            Hide();
        }
    }
}