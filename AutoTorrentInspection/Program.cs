using System;
using System.IO;
using System.Windows.Forms;
using AutoTorrentInspection.Forms;
using AutoTorrentInspection.Logging.Handlers;
using Microsoft.Win32;
using AutoTorrentInspection.Util;

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
                var ret = Notification.ShowInfo("需要 .Net4.7 或以上版本以保证所有功能正常运作，是否不再提示？");
                System.Diagnostics.Process.Start("http://dotnetsocial.cloudapp.net/GetDotnet?tfm=.NETFramework,Version=v4.7");
                if (ret == DialogResult.Yes) RegistryStorage.Save("False", name: "DoVersionCheck");
            }

            Logger.StoreLogMessages = true;
            Logger.LoggerHandlerManager.AddHandler(new DebugConsoleLoggerHandler());

            if (!CheckDependencies())
            {
                //todo: download the dependencies automatically
                return;
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


        private static bool CheckDependencies()
        {
            var ret = true;
            var basePath = Path.GetDirectoryName(Application.ExecutablePath) ?? "";
            var requires = new[]
            {
                ("Jil", "https://www.nuget.org/api/v2/package/Jil/", "2.15.4"),
                ("Sigil", "https://www.nuget.org/api/v2/package/Sigil/", "4.7.0")
            };
            foreach (var require in requires)
            {
                var dllPath = Path.Combine(basePath, require.Item1 + ".dll");
                if (File.Exists(dllPath)) continue;
                Notification.ShowInfo($"缺少{require.Item1} {require.Item3}");
                ret = false;
            }
            return ret;
        }
    }
}
