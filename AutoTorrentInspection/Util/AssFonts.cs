using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public void FeedFont(string fontPath)
        {
            GetNameVia(fontPath, ref _existFonts);
        }

        public HashSet<string> UsedFonts => _usedFonts;
        public HashSet<string> ExistFonts => _existFonts;

        private static readonly Regex StyleRegex = new Regex(@"^Style:[^,]*,\s*([^,]*)\s*,\d+");
        private static readonly Regex InlineFontRegex = new Regex(@"{[^}]*\\fn\s*([^\\}]*)\s*[^}]*?}");

        public static ISet<string> GetFontsUsed(string subtitlePath)
        {
            var usedFonts = new HashSet<string>();
            GetFontsUsed(subtitlePath, ref usedFonts);
            return usedFonts;
        }

        private static void GetFontsUsed(string subtitlePath, ref HashSet<string> usedFonts)
        {
            if (usedFonts == null) usedFonts = new HashSet<string>();
            using (var stream = File.OpenText(subtitlePath))
            {
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    var ret = StyleRegex.Match(line);
                    if (ret.Success)
                    {
                        string font = ret.Groups[1].Value.TrimStart('@');
                        if (usedFonts.Contains(font))
                            continue;
                        usedFonts.Add(font);
                    }
                    else if (line.StartsWith("Dialogue"))
                    {
                        var rets = InlineFontRegex.Matches(line);
                        foreach (Match item in rets)
                        {
                            string font = item.Groups[1].Value.TrimStart('@');
                            if (usedFonts.Contains(font))
                                continue;
                            usedFonts.Add(font);
                        }
                    }
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
            PrivateFontCollection fontCol = new PrivateFontCollection();
            fontCol.AddFontFile(fontPath);
            foreach (var item in fontCol.Families)
                existFonts.Add(item.Name);
        }

        public static Dictionary<string, string> LoadInstalledFonts()
        {
            var ret = new Dictionary<string, string>();
            var languageCode = new[] { 1033, 1028, 2052, 1041 };
            foreach (FontFamily fontFamily in new InstalledFontCollection().Families)
            {
                string key = fontFamily.Name.ToLower();
                if (!ret.ContainsKey(key))
                {
                    ret[key] = fontFamily.Name;
                }
                foreach (int language in languageCode)
                {
                    string name = fontFamily.GetName(language);
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
