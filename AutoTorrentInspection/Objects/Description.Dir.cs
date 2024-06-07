using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;

namespace AutoTorrentInspection.Objects
{
    public class DirDescription
    {
        public string DirName;
        public string BasePath;
        public string RelativePath;

        public List<FileDescription> Files;

        public FileState State = FileState.InValidDir;

        public static readonly Regex SeriesTitlePattern = new Regex(GlobalConfiguration.Instance().Naming.Pattern.SERIES_TITLE);
        public static readonly Regex SeasonTitlePattern = new Regex(GlobalConfiguration.Instance().Naming.Pattern.SEASON_TITLE);
        public static readonly Regex SubTitlePattern = new Regex(@$"^({string.Join("|", GlobalConfiguration.Instance().Naming.Pattern.SUBTITLES)})$");

        public DirDescription(string dirName, string relativePath, string basePath)
        {
            DirName = dirName;
            BasePath = basePath;
            RelativePath = relativePath;
            Files = new List<FileDescription>();
            DirValidation();
        }

        public static bool RegexesMatch(string value, params Regex[] regexes)
        {
            return regexes.Any(regex => regex.IsMatch(value));
        }

        public bool DirValidation()
        {
            if (RelativePath.Contains("root"))
            {
                if (DirName == ".")
                {
                    State = FileState.ValidFile;
                    return true;
                }
                return false;
            }
            else
            {
                if (RegexesMatch(DirName, SubTitlePattern))
                {
                    State = FileState.ValidFile;
                    return true;
                }
                return false;
            }
        }

        public DataGridViewRow ToRow()
        {
            var row = new DataGridViewRow();
            row.Cells.AddRange(new DataGridViewTextBoxCell {Value = DirName});
            row.DefaultCellStyle.BackColor = FileDescription.StateColor[State];
            return row;
        }
    }

    public class SeasonDir
    {
        public string DirName;
        public int SeasonId;
        public Dictionary<string, DirDescription> SubDirs;

        public FileState State = FileState.InValidDir;

        public SeasonDir(string dirName, int seasonId)
        {
            DirName = dirName;
            SeasonId = seasonId;
            SubDirs = new Dictionary<string, DirDescription>();
            DirValidation();
        }

        public bool DirValidation()
        {
            if (DirDescription.RegexesMatch(DirName, DirDescription.SeasonTitlePattern))
            {
                State = FileState.ValidFile;
                return true;
            }
            return false;
        }

        public DataGridViewRow ToRow()
        {
            var row = new DataGridViewRow();
            row.Cells.AddRange(new DataGridViewTextBoxCell {Value = DirName});
            row.DefaultCellStyle.BackColor = FileDescription.StateColor[State];
            return row;
        }
    }

    public class SeriesDir
    {
        public string DirName;
        public List<SeasonDir> SeasonDirs;

        public FileState State = FileState.InValidDir;
        public bool IsSeries;

        public SeriesDir(string dirName, bool isSeries)
        {
            DirName = dirName;
            IsSeries = isSeries;
            SeasonDirs = new List<SeasonDir>();
            DirValidation();
        }

        public bool DirValidation()
        {
            if (!IsSeries || DirDescription.RegexesMatch(DirName, DirDescription.SeriesTitlePattern))
            {
                State = FileState.ValidFile;
                return true;
            }
            return false;
        }

        public DataGridViewRow ToRow()
        {
            var row = new DataGridViewRow();
            row.Cells.AddRange(new DataGridViewTextBoxCell {Value = DirName});
            row.DefaultCellStyle.BackColor = FileDescription.StateColor[State];
            return row;
        }
    }
}
