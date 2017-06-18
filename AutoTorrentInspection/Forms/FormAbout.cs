using System;
using System.Threading;
using System.Windows.Forms;

namespace AutoTorrentInspection.Forms
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
            this.CenterToScreen();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            while (Opacity > 0.01)
            {
                Opacity -= 0.01;
                Thread.Sleep(10);
            }
            this.Close();
        }
    }
}
