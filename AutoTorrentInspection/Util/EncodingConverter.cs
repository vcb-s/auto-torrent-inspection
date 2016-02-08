using System.IO;
using System.Text;

namespace AutoTorrentInspection.Util
{
    internal static class EncodingConverter
    {
        /// <summary>
        /// 用指定的编码打开文本文件并读取全部内容
        /// </summary>
        public static string GetStringFrom(string filename, string encoding)
        {
            StreamReader sr = null;
            try
            {
                Stream s = File.OpenRead(filename);
                sr = new StreamReader(s, Encoding.GetEncoding(encoding));
                return sr.ReadToEnd();
            }
            finally
            {
                sr?.Close();
            }
        }

        /// <summary>
        /// 用指定的编码保存文本文件
        /// </summary>
        public static void SaveAsEncoding(string content, string filename, string encoding)
        {
            StreamWriter sw = null;
            try
            {
                Stream s = File.Create(filename);
                sw = new StreamWriter(s, Encoding.GetEncoding(encoding));
                sw.Write(content);
            }
            finally
            {
                sw?.Close();
            }
        }
    }
}
