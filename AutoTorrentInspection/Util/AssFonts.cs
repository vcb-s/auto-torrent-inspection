using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    class AssFonts
    {
        private HashSet<string> _usedFonts;
        private HashSet<string> _existFonts;

        public AssFonts()
        {
            _usedFonts = new HashSet<string>();
            _existFonts = new HashSet<string>();
        }

        public void FeedSubtitle(string subtitlePath)
        {
            GetFontsUsed(subtitlePath, ref _usedFonts);
        }

        public void FeedSubtitle(IEnumerable<string> subtitlePaths)
        {
            foreach(var subtitlePath in subtitlePaths)
                GetFontsUsed(subtitlePath, ref _usedFonts);
        }

        public void FeedFont(string fontPath)
        {
            GetNameVia(fontPath, ref _existFonts);
        }

        public HashSet<string> UsedFonts => _usedFonts;
        public HashSet<string> ExistFonts => _existFonts;

        private static readonly Regex StyleRegex = new Regex(@"^Style:\s*(?<style>[^,]+?)\s*,\s*@?(?<font>[^,]+?)\s*,\s*\d+");
        private static readonly Regex DialogueRegex = new Regex(@"^Dialogue:\s*\d+\s*,\s*[^,]*\s*,\s*[^,]*\s*,\s*\*?(?<style>[^,]+?)\s*,");
        private static readonly Regex InlineFontRegex = new Regex(@"{[^}]*\\fn\s*@?(?<font>[^\\}]*)\s*[^}]*?}");

        public static ISet<string> GetFontsUsed(string subtitlePath)
        {
            var usedFonts = new HashSet<string>();
            GetFontsUsed(subtitlePath, ref usedFonts);
            return usedFonts;
        }

        private static void GetFontsUsed(string subtitlePath, ref HashSet<string> usedFonts)
        {
            const string undefinedStyle = "未定义的Style: {0}";
            const string unusedStyle = "未使用的Style: {0}";
            const string warningLine = ", 行号: {0}";
            Logger.Log($"Advanced SSA Subtitle: {subtitlePath}");
            if (usedFonts == null) usedFonts = new HashSet<string>();
            var lineIndex = 0;
            using (var stream = File.OpenText(subtitlePath))
            {
                string line;
                var styles = new Dictionary<string, string>();
                var usedStyle = new HashSet<string>();

                while ((line = stream.ReadLine()) != null)
                {
                    ++lineIndex;
                    var styleLine = StyleRegex.Match(line);
                    if (styleLine.Success)
                    {
                        var style = styleLine.Groups["style"].Value;
                        var font = styleLine.Groups["font"].Value;
                        if (!styles.ContainsKey(style)) styles[style] = font;
                    }
                    else
                    {
                        var dialogueLine = DialogueRegex.Match(line);
                        if (!dialogueLine.Success) continue;
                        var style = dialogueLine.Groups["style"].Value;
                        if (styles.ContainsKey(style))
                        {
                            usedStyle.Add(style);
                            usedFonts.Add(styles[style]);
                        }
                        else
                        {
                            var warning = string.Format(undefinedStyle, style);
                            Logger.Log(Logger.Level.Warning, warning + string.Format(warningLine, lineIndex));
                            usedFonts.Add(warning);
                        }
                        var rets = InlineFontRegex.Matches(line);
                        foreach (Match item in rets)
                        {
                            usedFonts.Add(item.Groups["font"].Value);
                        }
                    }
                }
                foreach (var style in styles.Keys.Except(usedStyle))
                {
                    var warning = string.Format(unusedStyle, style);
                    Logger.Log(Logger.Level.Warning, warning);
                    usedFonts.Add(warning);
                }
            }
        }

        public IEnumerable<string> GetNameVia(string fontPath)
        {
            var set = new HashSet<string>();
            GetNameVia(fontPath, ref set);
            return set.ToList();
        }

        private static void GetNameVia(string fontPath, ref HashSet<string> existFonts)
        {
            var fontCol = new PrivateFontCollection();
            fontCol.AddFontFile(fontPath);
            foreach (var item in fontCol.Families)
                existFonts.Add(item.Name);
        }

        public static Dictionary<string, string> LoadInstalledFonts()
        {
            var ret = new Dictionary<string, string>();
            var languageCode = new[] { 1033, 1028, 2052, 1041 };
            foreach (var fontFamily in new InstalledFontCollection().Families)
            {
                var key = fontFamily.Name.ToLower();
                if (!ret.ContainsKey(key))
                {
                    ret[key] = fontFamily.Name;
                }
                foreach (var language in languageCode)
                {
                    var name = fontFamily.GetName(language);
                    key = name.ToLower();
                    if (!ret.ContainsKey(key))
                    {
                        ret[key] = fontFamily.Name;
                    }
                }
            }
            return ret;
        }
    }
}
