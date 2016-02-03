using System;
using System.IO;
using NChardet;

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
                int lang = 2;//
                //用指定的语参数实例化Detector
                Detector det = new Detector(lang);
                //初始化
                MyCharsetDetectionObserver cdo = new MyCharsetDetectionObserver();
                det.Init(cdo);

                stream = File.OpenRead(filename);// response.GetResponseStream();

                byte[] buf = new byte[1024];
                int len;
                bool done = false;
                bool isAscii = true;

                while ((len = stream.Read(buf, 0, buf.Length)) != 0)
                {
                    // 探测是否为Ascii编码
                    if (isAscii)
                        isAscii = det.isAscii(buf, len);

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
                    Console.WriteLine("CHARSET = ASCII");
                    return "ASCII";
                    //found = true;
                }
                else if (cdo.Charset != null)
                {
                    Console.WriteLine("CHARSET = {0}", cdo.Charset);
                    return cdo.Charset;
                }

                string[] prob = det.getProbableCharsets();
                foreach (string item in prob)
                {
                    Console.WriteLine("Probable Charset = " + item);
                    switch (item)
                    {
                        case "Shift_JIS":
                            return item;
                        case "EUC-JP":
                            return item;
                    }
                }
                return "";
            }
            finally
            {
                stream?.Close();
            }
        }
    }
    internal class MyCharsetDetectionObserver : NChardet.ICharsetDetectionObserver
    {
        public string Charset = null;

        public void Notify(string charset)
        {
            Charset = charset;
        }
    }
}
