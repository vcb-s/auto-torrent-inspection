using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ChapterTool.Forms;

namespace AutoTorrentInspection.Util
{
    public static class Updater
    {
        private static void OnResponse(IAsyncResult ar)
        {
            Regex versionRegex = new Regex(@"AutoTorrentInspection (\d+)\.(\d+)\.(\d+)\.(\d+)");
            WebRequest webRequest = (WebRequest)ar.AsyncState;
            Stream responseStream = webRequest.EndGetResponse(ar).GetResponseStream();
            if (responseStream == null) return;

            StreamReader streamReader = new StreamReader(responseStream);
            string context = streamReader.ReadToEnd();
            var result = versionRegex.Match(context);
            if (!result.Success) return;

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var major = int.Parse(result.Groups[1].Value);
            var minor = int.Parse(result.Groups[2].Value);
            var bulid = int.Parse(result.Groups[3].Value);
            var reversion = int.Parse(result.Groups[4].Value);
            Version remoteVersion = new Version(major, minor, bulid, reversion);
            if (currentVersion >= remoteVersion)
            {
                MessageBox.Show($"v{currentVersion} 已是最新版", @"As Expected");
                return;
            }
            var dialogResult = MessageBox.Show(caption: @"Wow! Such Impressive", text: $"新车已发车 v{remoteVersion}，上车!",
                                               buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Asterisk);
            if (dialogResult != DialogResult.Yes) return;
            FormUpdater formUpdater = new FormUpdater(Application.ExecutablePath, remoteVersion);
            formUpdater.ShowDialog();
        }

        public static void CheckUpdate()
        {
            bool connected = IsConnectInternet();
            if (!connected) return;
            WebRequest webRequest = WebRequest.Create("http://tcupdate.applinzi.com/index.php");
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            webRequest.BeginGetResponse(OnResponse, webRequest);
        }

        private static bool IsConnectInternet()
        {
            return InternetGetConnectedState(0, 0);
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(int description, int reservedValue);
    }
}