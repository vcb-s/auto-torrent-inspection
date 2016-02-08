using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    public static class CueCurer
    {
        private static readonly Regex CueFileNameRegex = new Regex(@"FILE\s\""(?<fileName>.*?)\""\sWAVE", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        /// <summary>
        /// 检测cue文件内的文件名是否与文件对应
        /// </summary>
        /// <param name="cueFile"></param>
        /// <returns></returns>
        public static bool CueMatchCheck(FileDescription cueFile)
        {
            var cueContext = EncodingConverter.GetStringFrom(cueFile.FullPath, cueFile.Encode);
            var rootPath = Path.GetDirectoryName(cueFile.FullPath);
            var result = true;
            foreach (Match audioName in CueFileNameRegex.Matches(cueContext))
            {
                var audioFile = $"{rootPath}\\{audioName.Groups["fileName"].Value}";
                result &= File.Exists(audioFile);
            }
            return result;
        }

        /// <summary>
        /// 修复cue文件中对应音频文件错误的扩展名
        /// </summary>
        /// <param name="original">cue文件的内容</param>
        /// <param name="directory">cue文件所在目录</param>
        public static string FixFilename(string original, string directory)
        {
            string filename = CueFileNameRegex.Match(original).Groups["fileName"].ToString();
            if (!directory.EndsWith("\\"))
            {
                directory += "\\";
            }
            string[] files = null;
            if (!File.Exists(directory + filename))
            {
                //找到目录里的所有主文件名相同的文件
                files = Directory.GetFiles(directory, filename.Split('.')[0] + ".*", SearchOption.TopDirectoryOnly);

            }
            if (files != null && files.Length > 0)
            {
                foreach (var file in files)
                {
                    var fi = new FileInfo(file);
                    if (ExtIsAudioFile(fi.Extension))
                    {
                        fi = new FileInfo(file);
                        original = original.Replace(filename, fi.Name);
                    }
                }
            }

            return original;
        }

        private static bool ExtIsAudioFile(string ext)
        {
            if (ext.StartsWith("."))
            {
                ext = ext.TrimStart('.');
            }
            return ext.Equals("flac", StringComparison.CurrentCultureIgnoreCase) ||
                   ext.Equals("m4a",  StringComparison.CurrentCultureIgnoreCase) ||
                   ext.Equals("tak",  StringComparison.CurrentCultureIgnoreCase) ||
                   ext.Equals("wav",  StringComparison.CurrentCultureIgnoreCase) ||
                   ext.Equals("mp3",  StringComparison.CurrentCultureIgnoreCase) ||
                   ext.Equals("bin",  StringComparison.CurrentCultureIgnoreCase) ||
                   ext.Equals("img",  StringComparison.CurrentCultureIgnoreCase);
        }

        public static void MakeBackup(string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (!File.Exists(fi.DirectoryName + "\\" + fi.Name + ".bak"))
                {
                    fi.CopyTo(fi.DirectoryName + "\\" + fi.Name + ".bak");
                }
            }

            catch (IOException)
            { }
        }
    }
}