using NChardet;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace AutoTorrentInspection.Util
{
    public static class EncodingDetector
    {
        /// <summary>
        /// 获取文本文件使用的编码
        /// </summary>
        public static string GetEncoding(string filename)
        {
            Stream stream = null;
            try
            {
                const int lang = 1;
                //1.Japanese
                //2.Chinese
                //3.Simplified Chinese
                //4.Traditional Chinese
                //5.Korean
                //6.Dont know(默认)
                //用指定的语参数实例化Detector
                Detector det = new Detector(lang);
                //初始化
                MyCharsetDetectionObserver cdo = new MyCharsetDetectionObserver();
                det.Init(cdo);
                if (!File.Exists(filename))
                {
                    return "";
                }
                Debug.WriteLine($"--{Path.GetFileName(filename)}--");
                stream = File.OpenRead(filename);// response.GetResponseStream();

                int len;
                var buf      = new byte[1024];
                bool done    = false;
                bool isAscii = true;

                while ((len = stream.Read(buf, 0, buf.Length)) != 0)
                {
                    // 探测是否为Ascii编码
                    if (isAscii)
                        isAscii = Detector.isAscii(buf, len);

                    // 如果不是Ascii编码，并且编码未确定，则继续探测
                    if (!isAscii && !done)
                        done = det.DoIt(buf, len, false);

                }
                stream.Close();
                stream.Dispose();
                //调用DatEnd方法，
                //如果引擎认为已经探测出了正确的编码，
                //则会在此时调用ICharsetDetectionObserver的Notify方法
                det.DataEnd();

                if (isAscii)
                {
                    Debug.WriteLine("CHARSET = ASCII");
                    return "ASCII";
                }
                else if (cdo.Charset != null)
                {
                    Debug.WriteLine($"CHARSET = {cdo.Charset}");
                    return cdo.Charset;
                }

                var prob = det.getProbableCharsets();

                var probEncode = prob.Aggregate("", (current, item) => current + item + " ");
                Debug.WriteLine($"Probable Charset = {probEncode}");
                foreach (string item in prob)
                {
                    switch (item)
                    {
                        case "Shift_JIS":
                            return item;
                        case "EUC-JP":
                            return item;
                    }
                }
                return "GB18030";//to avoid exception while can not determine encode.
            }
            finally
            {
                stream?.Close();
            }
        }
    }
    internal class MyCharsetDetectionObserver : ICharsetDetectionObserver
    {
        public string Charset;

        public void Notify(string charset)
        {
            Charset = charset;
        }
    }
}
