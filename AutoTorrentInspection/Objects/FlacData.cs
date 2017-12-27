using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoTorrentInspection.Objects
{
    public class FlacInfo
    {
        public long RawLength                           { get; set; }
        public long TrueLength                          { get; set; }
        public double CompressRate => TrueLength / (double)RawLength;
        public bool HasCover                            { get; set; }
        public long SampleRate                          { get; set; }
        public long BitPerSample                        { get; set; }
        public bool IsHiRes => SampleRate > 44100 && BitPerSample > 16;
        public string Encoder                           { get; set; }
        public Dictionary<string, string> VorbisComment { get; }

        public FlacInfo()
        {
            VorbisComment = new Dictionary<string, string>();
        }
    }

    //https://xiph.org/flac/format.html
    public static class FlacData
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private enum BlockType
        {
            STREAMINFO = 0x00,
            PADDING,
            APPLICATION,
            SEEKTABLE,
            VORBIS_COMMENT,
            CUESHEET,
            PICTURE
        };

        public static FlacInfo GetMetadataFromFlac(string flacPath)
        {
            Logger.Log(flacPath);
            using (var fs = File.OpenRead(flacPath))
            {
                var info      = new FlacInfo();
                var header    = Encoding.ASCII.GetString(fs.ReadBytes(4), 0, 4);
                if (header != "fLaC")
                    throw new InvalidDataException($"Except an flac but get an {header}" +
                        $"{Environment.NewLine}File name: {Path.GetFileName(flacPath)}");
                //METADATA_BLOCK_HEADER
                //1-bit Last-metadata-block flag
                //7-bit BLOCK_TYPE
                //24-bit Length
                long metaLength = 4/*header*/;
                while (fs.Position < fs.Length)
                {
                    var blockHeader       = fs.BEInt32();
                    var lastMetadataBlock = blockHeader >> 31 == 0x1;
                    var blockType         = (BlockType)((blockHeader >> 24) & 0x7f);
                    var length            = blockHeader & 0xffffff;
                    var prePos            = fs.Position;
                    metaLength           += length + 4/*length of METADATA_BLOCK_HEADER*/;
                    Logger.Log($"|+{blockType} with Length: {length}");
                    switch (blockType)
                    {
                    case BlockType.STREAMINFO:
                        Debug.Assert(length == 34);
                        ParseStreamInfo(fs, ref info);
                        break;
                    case BlockType.VORBIS_COMMENT:
                        ParseVorbisComment(fs, ref info);
                        break;
                    case BlockType.PICTURE:
                        ParsePicture(fs, ref info);
                        break;
                    case BlockType.PADDING:
                    case BlockType.APPLICATION:
                    case BlockType.SEEKTABLE:
                    case BlockType.CUESHEET:
                        fs.Seek(length, SeekOrigin.Current);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Invalid BLOCK_TYPE: 0x{blockType:X}");
                    }
                    Debug.Assert(fs.Position - prePos == length);
                    if (lastMetadataBlock) break;
                }
                Debug.Assert(fs.Position == metaLength);
                info.TrueLength = fs.Length - fs.Position;
                return info;
            }
        }

        private static void ParseStreamInfo(Stream fs, ref FlacInfo info)
        {
            var minBlockSize  = fs.BEInt16();
            var maxBlockSize  = fs.BEInt16();
            var minFrameSize  = fs.BEInt24();
            var maxFrameSize  = fs.BEInt24();
            var buffer        = fs.ReadBytes(8);
            var br            = new BitReader(buffer);
            var sampleRate    = br.GetBits(20);
            var channelCount  = br.GetBits(3)+1;
            var bitPerSample  = br.GetBits(5)+1;
            var totalSample   = br.GetBits(36);
            var md5           = fs.ReadBytes(16);
            info.RawLength    = channelCount * bitPerSample / 8 * totalSample;

            info.SampleRate = sampleRate;
            info.BitPerSample = bitPerSample;
            Logger.Log($" | minimum block size: {minBlockSize}, maximum block size: {maxBlockSize}");
            Logger.Log($" | minimum frame size: {minFrameSize}, maximum frame size: {maxFrameSize}");
            Logger.Log($" | Sample rate: {sampleRate}Hz, bits per sample: {bitPerSample}-bit");
            Logger.Log($" | Channel count: {channelCount}");
            var md5String     = md5.Aggregate("", (current, item) => current + $"{item:X2}");
            Logger.Log($" | MD5: {md5String}");
        }

        private static void ParseVorbisComment(Stream fs, ref FlacInfo info)
        {
            //only here in flac use little-endian
            var vendorLength        = (int) fs.LEInt32();
            var vendorRawStringData = fs.ReadBytes(vendorLength);
            var vendor              = Encoding.UTF8.GetString(vendorRawStringData, 0, vendorLength);
            info.Encoder            = vendor;
            Logger.Log($" | Vendor: {vendor}");
            var userCommentListLength = fs.LEInt32();
            for (var i = 0; i < userCommentListLength; ++i)
            {
                var commentLength        = (int) fs.LEInt32();
                var commentRawStringData = fs.ReadBytes(commentLength);
                var comment              = Encoding.UTF8.GetString(commentRawStringData, 0, commentLength);
                var spilterIndex         = comment.IndexOf('=');
                var key                  = comment.Substring(0, spilterIndex);
                var value                = comment.Substring(spilterIndex + 1, comment.Length - 1 - spilterIndex);
                info.VorbisComment[key]  = value;
                var summary              = value.Length > 25 ? value.Substring(0, 25) + "..." : value;
                Logger.Log($" | [{key}] = '{summary.Replace('\n', ' ')}'");
            }
        }

        private static readonly string[] PictureTypeName =
        {
            "Other", "32x32 pixels 'file icon'", "Other file icon",
            "Cover (front)", "Cover (back)", "Leaflet page",
            "Media", "Lead artist/lead performer/soloist", "Artist/performer",
            "Conductor", "Band/Orchestra", "Composer",
            "Lyricist/text writer", "Recording Location", "During recording",
            "During performance", "Movie/video screen capture", "A bright coloured fish",
            "Illustration", "Band/artist logotype", "Publisher/Studio logotype",
            "Reserved"
        };

        private static void ParsePicture(Stream fs, ref FlacInfo info)
        {
            var pictureType       = fs.BEInt32();
            var mimeStringLength  = (int) fs.BEInt32();
            var mimeType          = Encoding.ASCII.GetString(fs.ReadBytes(mimeStringLength), 0, mimeStringLength);
            var descriptionLength = (int) fs.BEInt32();
            var description       = Encoding.UTF8.GetString(fs.ReadBytes(descriptionLength), 0, descriptionLength);
            var pictureWidth      = fs.BEInt32();
            var pictureHeight     = fs.BEInt32();
            var colorDepth        = fs.BEInt32();
            var indexedColorCount = fs.BEInt32();
            var pictureDataLength = fs.BEInt32();
            fs.Seek(pictureDataLength, SeekOrigin.Current);
            info.HasCover         = true;
            if (pictureType > 20) pictureType = 21;
            Logger.Log($" | picture type: {PictureTypeName[pictureType]}");
            Logger.Log($" | picture format type: {mimeType}");
            if (descriptionLength > 0)
                Logger.Log($" | description: {description}");
            Logger.Log($" | attribute: {pictureWidth}px*{pictureHeight}px@{colorDepth}-bit");
            if (indexedColorCount != 0)
                Logger.Log($" | indexed-color color: {indexedColorCount}");
        }
    }

    internal static class Utils
    {
        public static byte[] ReadBytes(this Stream fs, int length)
        {
            var ret = new byte[length];
            fs.Read(ret, 0, length);
            return ret;
        }

        #region int reader
        public static uint BEInt32(this Stream fs)
        {
            var b = fs.ReadBytes(4);
            return b[3] + ((uint)b[2] << 8) + ((uint)b[1] << 16) + ((uint)b[0] << 24);
        }

        public static uint LEInt32(this Stream fs)
        {
            var b = fs.ReadBytes(4);
            return b[0] + ((uint)b[1] << 8) + ((uint)b[2] << 16) + ((uint)b[3] << 24);
        }

        public static int BEInt24(this Stream fs)
        {
            var b = fs.ReadBytes(3);
            return b[2] + (b[1] << 8) + (b[0] << 16);
        }

        public static int LEInt24(this Stream fs)
        {
            var b = fs.ReadBytes(3);
            return b[0] + (b[1] << 8) + (b[2] << 16);
        }

        public static int BEInt16(this Stream fs)
        {
            var b = fs.ReadBytes(2);
            return b[1] + (b[0] << 8);
        }

        public static int LEInt16(this Stream fs)
        {
            var b = fs.ReadBytes(2);
            return b[0] + (b[1] << 8);
        }
        #endregion
    }

    internal class BitReader
    {
        private readonly byte[] _buffer;
        private int _bytePosition;
        private int _bitPositionInByte;

        public int Position => _bytePosition * 8 + _bitPositionInByte;

        public BitReader(byte[] source)
        {
            _buffer = new byte[source.Length];
            Array.Copy(source, _buffer, source.Length);
        }

        public void Reset()
        {
            _bytePosition = 0;
            _bitPositionInByte = 0;
        }

        public bool GetBit()
        {
            if (_bytePosition >= _buffer.Length)
                throw new IndexOutOfRangeException(nameof(_bytePosition));
            var ret = ((_buffer[_bytePosition] >> (7 - _bitPositionInByte)) & 1) == 1;
            Next();
            return ret;
        }

        private void Next()
        {
            ++_bitPositionInByte;
            if (_bitPositionInByte != 8) return;
            _bitPositionInByte = 0;
            ++_bytePosition;
        }

        public void Skip(int length)
        {
            for (var i = 0; i < length; ++i)
            {
                Next();
            }
        }

        public long GetBits(int length)
        {
            long ret = 0;
            for (var i = 0; i < length; ++i)
            {
                ret |= ((long) (_buffer[_bytePosition] >> (7 - _bitPositionInByte)) & 1) << (length - 1 - i);
                Next();
            }
            return ret;
        }
    }
}