using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AutoTorrentInspection
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!IsSupportedRuntimeVersion())
            {
                var ret = Util.Notification.ShowInfo("需要 .Net4.7 或以上版本以保证所有功能正常运作，是否不再提示？");
                System.Diagnostics.Process.Start("http://dotnetsocial.cloudapp.net/GetDotnet?tfm=.NETFramework,Version=v4.7");
                if (ret == DialogResult.Yes) Util.RegistryStorage.Save("False", name: "DoVersionCheck");
            }

            if (args.Length == 0)
            {
                Application.Run(new Form1());
            }
            else
            {
                var argsFull = string.Join(" ", args);
                //argsFull = "\"" + argsFull + "\"";
                Application.Run(new Form1(argsFull));
            }
        }

        private static bool IsSupportedRuntimeVersion()
        {
            //https://msdn.microsoft.com/en-us/library/hh925568
            const int minSupportedRelease = 460798;
            if (Util.RegistryStorage.Load(name: "DoVersionCheck") == "False") return true;
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"))
            {
                if (key?.GetValue("Release") != null)
                {
                    var releaseKey = (int)key.GetValue("Release");
                    if (releaseKey >= minSupportedRelease) return true;
                }
            }
            return false;
        }
    }
}
