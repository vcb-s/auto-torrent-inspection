using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
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
            if (!directory.EndsWith("\\")) directory += "\\";
            try
            {
                //找到目录里的所有主文件名相同的文件
                var files = Directory.GetFiles(directory, filename.Substring(0, filename.LastIndexOf('.')) + ".*", SearchOption.TopDirectoryOnly);
                var matchedFile = files.Select(file => new FileInfo(file)).First(fi => RAudioExt.IsMatch(fi.Extension));
                original        = original.Replace(filename, matchedFile.Name);
            }
            catch(Exception exception)
            {
                Debug.WriteLine(exception.Message + '\n' + directory);
            }
            return original;
        }

        private static readonly Regex RAudioExt = new Regex(@"\.(flac|m4a|tak|ape|tta|wav|mp3|bin|img)");

        public static void MakeBackup(string filename)
        {
            if (File.Exists(filename + ".bak")) return;
            try
            {
                FileInfo fi = new FileInfo(filename);
                var bakPath = $"{fi.DirectoryName}\\{fi.Name}.bak";
                if (!File.Exists(bakPath))
                {
                    fi.CopyTo(bakPath);
                }
            }
            catch (IOException exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }
    }
}