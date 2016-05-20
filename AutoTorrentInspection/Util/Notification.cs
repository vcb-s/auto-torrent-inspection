using System;
using System.Windows.Forms;

namespace AutoTorrentInspection.Util
{
    public static class Notification
    {
        public static DialogResult ShowError(string argMessage, Exception exception)
        {
            return MessageBox.Show(caption: @"ATI Error",
                text: $"{argMessage}:{Environment.NewLine}{exception.Message}"
#if DEBUG
                + $"{Environment.NewLine}{exception.StackTrace}"
#endif
                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Hand);
        }

        public static DialogResult ShowInfo(string argMessage)
        {
            return MessageBox.Show(caption: @"ATI Warning",
                text: argMessage,
                buttons: MessageBoxButtons.YesNo,icon: MessageBoxIcon.Information);
        }
    }
}