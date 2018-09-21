using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutoTorrentInspection.Util
{
    class AssCheck
    {
        private HashSet<string> _usedFonts;
        private HashSet<string> _existFonts;
        private HashSet<string> _unuesedOrMissingStyles;
        private HashSet<string> _unexpectedTags;

        public AssCheck()
        {
            _usedFonts = new HashSet<string>();
            _existFonts = new HashSet<string>();
            _unuesedOrMissingStyles = new HashSet<string>();
            _unexpectedTags = new HashSet<string>();
        }

        public void FeedSubtitle(string subtitlePath)
        {
            Logger.Log($"Advanced SSA Subtitle: {subtitlePath}");
            GetFontsUsed(subtitlePath, ref _usedFonts, ref _unuesedOrMissingStyles);
            GetUnexpectedTags(subtitlePath, ref _unexpectedTags);
        }

        public void FeedSubtitle(IEnumerable<string> subtitlePaths)
        {
            foreach (var subtitlePath in subtitlePaths)
                FeedSubtitle(subtitlePath);
        }

        public void FeedFont(string fontPath)
        {
            GetFontNameVia(fontPath, ref _existFonts);
        }

        public HashSet<string> UsedFonts => _usedFonts;
        public HashSet<string> ExistFonts => _existFonts;
        public HashSet<string> UnusedOrMissingStyles => _unuesedOrMissingStyles;
        public HashSet<string> UnexpectedTags => _unexpectedTags;

        private static readonly Regex StyleRegex = new Regex(@"^Style:\s*(?<style>[^,]+?)\s*,\s*@?(?<font>[^,]+?)\s*,\s*\d+");
        private static readonly Regex DialogueRegex = new Regex(@"^Dialogue:\s*\d+\s*,\s*[^,]*\s*,\s*[^,]*\s*,\s*\*?(?<style>[^,]+?)\s*,");
        private static readonly Regex InlineFontRegex = new Regex(@"{[^}]*\\fn\s*@?(?<font>[^\\}]*)\s*[^}]*?}");

        private static void GetFontsUsed(string subtitlePath, ref HashSet<string> usedFonts, ref HashSet<string> unusedOrMissingStyles)
        {
            const string undefinedStyle = "未定义的Style: {0}";
            const string unusedStyle = "未使用的Style: {0}";
            const string warningLine = ", 行号: {0}";
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
                            unusedOrMissingStyles.Add(warning);
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
                    unusedOrMissingStyles.Add(warning);
                }
            }
        }

        private static void GetUnexpectedTags(string subtitlePath, ref HashSet<string> unexpectedTags)
        {
            using (var stream = File.OpenText(subtitlePath))
            {
                string line;

                while ((line = stream.ReadLine()) != null)
                {
                    for (int i = 0, j; (j = line.IndexOf('\\', i)) >= 0; i = j)
                    {
                        var cmd = "";
                        for (int t = ++j; t < line.Length && line[t] != '(' && line[t] != '\\'; ++t)
                        {
                            cmd += line[t];
                        }

                        if (string.IsNullOrWhiteSpace(cmd))
                        {
                            continue;
                        }

                        foreach (var tag in GlobalConfiguration.Instance().ASS.UnexceptedTags)
                        {
                            if (cmd.StartsWith(tag))
                            {
                                unexpectedTags.Add(tag);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<string> GetFontNameVia(string fontPath)
        {
            var set = new HashSet<string>();
            GetFontNameVia(fontPath, ref set);
            return set.ToList();
        }

        private static void GetFontNameVia(string fontPath, ref HashSet<string> existFonts)
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
