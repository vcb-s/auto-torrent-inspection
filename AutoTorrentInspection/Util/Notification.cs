using System;
using System.Windows.Forms;

namespace AutoTorrentInspection.Util
{
    public static class Notification
    {
        public static DialogResult ShowError(string argMessage, Exception exception)
        {
            return MessageBox.Show(caption: @"ATI Error",
                text: $"{_(argMessage)}:{Environment.NewLine}{_(exception.Message)}"
#if DEBUG
                + $"{Environment.NewLine}{exception.StackTrace}"
#endif
                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Hand);
        }

        public static DialogResult ShowInfo(string argMessage)
        {
            return MessageBox.Show(caption: @"ATI Info",
                text: _(argMessage),
                buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Information);
        }

        public static DialogResult ShowWithTitle(this string argMessage, string title)
        {
            return MessageBox.Show(caption: _(title), text: _(argMessage));
        }

        private static string _(string value) => value.Replace('\0', ' ');
    }
}