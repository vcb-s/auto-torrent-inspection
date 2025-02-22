﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Jil;

namespace AutoTorrentInspection
{
    public class GlobalConfiguration
    {
        private static Configuration _instance;

        private const string ConfigFile = "config.json";

        protected GlobalConfiguration() {}

        public static Configuration Instance(bool reload = false)
        {
            if (_instance == null || reload)
            {
                try
                {
                    using (var input = new StreamReader(ConfigFile))
                    {
                        _instance = JSON.Deserialize<Configuration>(input);
                        Logger.Log("Load configuration file success");
                    }
                }
                catch (Exception exception)
                {
                    Logger.Log(exception);
                    _instance = new Configuration();
                }
            }
            return _instance;
        }
    }

    public class Configuration
    {
        public int Version = 4;
        public Naming Naming = new Naming();
        public RowColor RowColor = new RowColor();
        public InspectionOptions InspectionOptions = new InspectionOptions();
        public string[] TrackerList = {
            "http://open.acgtracker.com:1096/announce",
            "http://nyaa.tracker.wf:7777/announce",
            "http://sukebei.tracker.wf:8888/announce",
            "udp://tracker.torrent.eu.org:451/announce",
            "udp://open.stealth.si:80/announce",
            "udp://tracker.opentrackr.org:1337/announce",
            "http://t.acg.rip:6699/announce",
            "http://share.hkg-fansub.info:80/announce.php",
            "http://tracker.sbsub.com:2710/announce"
        };

        public ASS ASS = new ASS();

        public override string ToString()
        {
            return JSON.Serialize(this, new Options(true));
        }
    }

    public class Naming
    {
        public Pattern Pattern = new Pattern();
        public Extension Extension = new Extension();
        public string[] UnexpectedCharacters =
        {
            "\u3099", "\u309a", "\u200B",
            "\u0300", "\u0310", "\u0320", "\u0330", "\u0340", "\u0350", "\u0360",
            "\u0301", "\u0311", "\u0321", "\u0331", "\u0341", "\u0351", "\u0361",
            "\u0302", "\u0312", "\u0322", "\u0332", "\u0342", "\u0352", "\u0362",
            "\u0303", "\u0313", "\u0323", "\u0333", "\u0343", "\u0353", "\u0363",
            "\u0304", "\u0314", "\u0324", "\u0334", "\u0344", "\u0354", "\u0364",
            "\u0305", "\u0315", "\u0325", "\u0335", "\u0345", "\u0355", "\u0365",
            "\u0306", "\u0316", "\u0326", "\u0336", "\u0346", "\u0356", "\u0366",
            "\u0307", "\u0317", "\u0327", "\u0337", "\u0347", "\u0357", "\u0367",
            "\u0308", "\u0318", "\u0328", "\u0338", "\u0348", "\u0358", "\u0368",
            "\u0309", "\u0319", "\u0329", "\u0339", "\u0349", "\u0359", "\u0369",
            "\u030A", "\u031A", "\u032A", "\u033A", "\u034A", "\u035A", "\u036A",
            "\u030B", "\u031B", "\u032B", "\u033B", "\u034B", "\u035B", "\u036B",
            "\u030C", "\u031C", "\u032C", "\u033C", "\u034C", "\u035C", "\u036C",
            "\u030D", "\u031D", "\u032D", "\u033D", "\u034D", "\u035D", "\u036D",
            "\u030E", "\u031E", "\u032E", "\u033E", "\u034E", "\u035E", "\u036E",
            "\u030F", "\u031F", "\u032F", "\u033F", "\u034F", "\u035F", "\u036F",
        };
    }

    public class Pattern
    {
        [JilDirective(Ignore = true)]
        public string FCH   = @"^(?:\[(?:[^\[\]])*philosophy\-raws(?:[^\[\]])*\])\[[^\[\]]+\]\[(?:(?:[^\[\]]+\]\[(?:BDRIP|DVDRIP|BDRemux))|(?:(?:BDRIP|DVDRIP|BDRemux)(?:\]\[[^\[\]]+)?))\]\[(?:(?:(?:HEVC )?Main10P)|(?:(?:AVC )?Hi10P)|Hi444PP|H264) \d*(?:FLAC|AC3)\]\[(?:(?:(?:1920|1440)[Xx]1080)|(?:1280[Xx]720)|(?:1024[Xx]576)|(?:720[Xx]480))\](?:(?:\.(?:sc|tc|chs|cht))?\.ass|(?:\.(?:mkv|mka|flac)))$";
        [JilDirective(Ignore = true)]
        public string MAWEN = @"^[^\[\]]+ \[(?:BD|BluRay|BD\-Remux|SPDVD|DVD) (?:1920x1080p?|1280x720p?|720x480p?|1080p|720p|480p)(?: (?:23\.976|24|25|29\.970|59\.940)fps)?(?: vfr)? (?:(?:(?:AVC|HEVC)\-(?:Lossless-)?(?:yuv420p10|yuv420p8|yuv444p10))|(?:x264(?:-Hi(?:10|444P)P)?|x265-Ma10P))(?: (?:FLAC|AAC|AC3)(?:x\d)?)+(?: (?:Chap|Ordered\-Chap))?\](?: v\d)? - (?:[^\.&]+ ?& ?)*mawen1250(?: ?& ?[^\.&]+)*(?:(?:\.(?:sc|tc|chs|cht))?\.ass|(?:\.(?:mkv|mka|flac)))$";

        private const string DATE = @"(?<year>[0-9]{2})(?<month>(0[1-9])|(1[0-2]))(?<day>(0[1-9])|([1-2][0-9])|3[0-1])";
        private const string SPCD = @"SPCD";
        private const string VOLUME = @"(0[1-9])|([1-9][0-9])";
        private const string CONTENT_NAME = @$"(?!{SPCD})([^｢｣／& ]+[^｢｣／&]*[^｢｣／& ]+|[^｢｣／& ]+)";
        private const string SPECIAL_NAME = @"(｢[^｢｣]+｣)";
        private const string DESCRIPTION = @$"({CONTENT_NAME}|{SPECIAL_NAME}|{CONTENT_NAME} {SPECIAL_NAME})";
        private const string DESCRIPTIONS = @$"({DESCRIPTION}( & {DESCRIPTION})*)";
        private const string ARTISTS = @"／([^｢｣／\[\] ]+[^｢｣／\[\]]*[^｢｣／\[\] ]+|[^｢｣／\[\] ]+)";
        private const string HIRES_FORMAT = @"(?<bit>16|24|32)bit_(?<freq>48|96|192|384)kHz";
        private const string FILE_FORMAT = @"(?<audio>((flac|wavpack)\+aac\+mp3)|((flac|wavpack)\+mp3\+aac)|((flac|wavpack)\+(aac|mp3))|flac|wavpack|aac|mp3)(\+(?<image>webp\+jpg|jpg\+webp|webp|jpg))?(\+(?<video>mkv\+mp4|mp4\+mkv|mkv|mp4))?";

        public string CD_DIR = @$"^\[{DATE}\] ({SPCD}( {VOLUME})?( {DESCRIPTIONS})?|{DESCRIPTIONS})({ARTISTS})?( \[{HIRES_FORMAT}\])? \({FILE_FORMAT}\)$";

        private const string GROUP = @"([^\[\]]*)VCB\-S(tudio)?([^\[\]]*)";
        private const string TITLE = @"([^\[\] ]+[^\[\]]*[^\[\] ]+|[^\[\] ]+)";
        private const string EP = @"([^\[\] ]+[^\[\]]*[^\[\] ]+|[^\[\] ]+)";
        private const string LEVEL = @"(Hi10p|Hi444pp|Ma1(0|2)p|Ma444-1(0|2)p)";
        private const string RESOLUTION = @"([1-9][0-9]{2,3}p|4K)";
        private const string UHD_SUFFIX = @"(HDR|HDR10|SDR|DoVi(_P[\d\.]+)?)";
        private const string VIDEO_FORMAT = @"(x264|x265|svtav1)";
        private const string AUDIO_FORMAT = @"(_\d?flac)?(_\d?aac)?(_\d?eac3)?(_\d?ac3)?(_\d?dts)?";
        private const string LANGUAGE_TAG = @"([\w-&]+)";

        private const string PROFILE = @$"({LEVEL}_)?{RESOLUTION}(_{UHD_SUFFIX})?";
        private const string AV_FORMAT = @$"{VIDEO_FORMAT}{AUDIO_FORMAT}";

        public string SERIES_TITLE = @$"^\[{GROUP}\] {TITLE}$";
        public string SEASON_TITLE = @$"^\[{GROUP}\] {TITLE} \[{PROFILE}\]$";
        public string[] SUBTITLES = {"CDs", "Scans", "SPs"};

        public string VCBS_NORMAL = @$"^\[{GROUP}\] {TITLE} (\[{EP}\])?\[{PROFILE}\]\[{AV_FORMAT}\]((\.{LANGUAGE_TAG})?\.ass|(\[{LANGUAGE_TAG}\])?\.(mkv|mka|mp4))$";
        public string VCBS_SPECIAL = @$"^\[{GROUP}\] {TITLE} \[{EP}\]\.(png|7z|zip|flac|m4a|eac3|ac3|dts)$";
    }

    public class Extension
    {
        public string[] VideoExtensions = {"mkv", "mp4"};
        [JilDirective(Ignore = true)]
        public Regex VideoExtension => new Regex($@"\.{string.Join("|", VideoExtensions)}$", RegexOptions.IgnoreCase);

        public string[] AudioExtensions = {"flac", "wv", "m4a", "mp3", "cue", "log"};
        [JilDirective(Ignore = true)]
        public Regex AudioExtension => new Regex($@"\.{string.Join("|", AudioExtensions)}$", RegexOptions.IgnoreCase);

        public string[] ImageExtensions = {"jpg", "webp"};
        [JilDirective(Ignore = true)]
        public Regex ImageExtension => new Regex($@"\.{string.Join("|", ImageExtensions)}$", RegexOptions.IgnoreCase);

        public string[] ExceptExtensions = {"7z", "zip"};
        [JilDirective(Ignore = true)]
        public Regex ExceptExtension => new Regex($@"\.{string.Join("|", ExceptExtensions)}$", RegexOptions.IgnoreCase);
    }

    public class RowColor
    {
        public string INVALID_FILE           = "fffb9966";
        public string VALID_FILE             = "ff92aaf3";
        public string INVALID_DIR            = "ffba55d3";
        public string INVALID_CUE            = "ffff6538";
        public string INVALID_ENCODE         = "ff51559b";
        public string INVALID_PATH_LENGTH    = "ffff0a32";
        public string INVALID_FLAC_LEVEL     = "ffcfd8dc";
        public string NON_UTF_8_W_BOM        = "fffbbc05";
        public string INVALID_FILE_SIGNATURE = "ff009933";
        public string INVALID_CD_FOLDER      = "ff0559ae";
        public string TAMPERED_LOG           = "ff8b4513";
        public string INVALID_FILE_NAME_CHAR = "ff2e373b";
        public string EMPTY_FILE             = "ffcad7ce";
    }

    public class InspectionOptions
    {
        public bool WebPPosition = false;
        public bool CDNaming = true;
        public bool FileHeader = true;
        public bool FLACCompressRate = true;
        public bool CUEEncoding = true;
        public bool LogValidation = true;
    }

    public class ASS
    {
        public string[] UnexpectedTags = { "1img", "2img", "3img", "4img", "1vc", "2vc", "3vc", "4vc", "1va", "2va", "3va", "4va", "distort", "frs", "fsvp", "jitter", "mover", "moves3", "moves4", "movevc", "rndx", "rndy", "rndz", "rnds", "rnd", "z" };
    }
}
