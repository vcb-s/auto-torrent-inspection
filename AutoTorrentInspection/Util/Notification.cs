using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTorrentInspection.Util
{
    public static class Notification
    {
        public static DialogResult ShowError(string argMessage, Exception exception)
        {
            var text = $"{_(argMessage)}:";
#if DEBUG
            var currentException = exception;
            while (currentException != null)
            {
                text += $"\n{_(exception.Message)}\n{currentException.StackTrace}\nwith inner exception: \n";
                currentException = currentException.InnerException;
            }

            text += "null";
#endif
            return MessageBox.Show(caption: @"ATI Error",
                text: text

                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Hand);
        }

        public static DialogResult ShowInfo(string argMessage)
        {
            return MessageBox.Show(caption: @"ATI Info",
                    text: _(argMessage),
                    buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Information);
        }

        public static void ShowWithTitle(this string argMessage, string title)
        {
            new Task(() => MessageBox.Show(caption: _(title), text: _(argMessage))).Start();
        }

        public static DialogResult ShowQuestion(string argQuestion, string argTitle, bool argShowCancel = true)
        {
            var msgBoxBtns = MessageBoxButtons.YesNoCancel;
            if (!argShowCancel)
            {
                msgBoxBtns = MessageBoxButtons.YesNo;
            }
            return MessageBox.Show(argQuestion, argTitle, msgBoxBtns, MessageBoxIcon.Question);
        }

        private static string _(string value) => value.Replace('\0', ' ');
    }
}