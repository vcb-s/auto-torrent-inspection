using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AutoTorrentInspection.Util
{
    public class FlacInfo
    {
        public long RawLength                           { get; set; }
        public long TrueLength                          { get; set; }
        public double CompressRate => TrueLength / (double)RawLength;
        public bool HasCover                            { get; set; }
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
        private const long SizeThreshold = 1 << 20;

        public static event Action<string> OnLog;

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
            using (var fs = File.Open(flacPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fs.Length < SizeThreshold) return new FlacInfo();
                FlacInfo info = new FlacInfo {TrueLength = fs.Length};
                var header = Encoding.ASCII.GetString(fs.ReadBytes(4), 0, 4);
                if (header != "fLaC")
                    throw new InvalidDataException($"Except an flac but get an {header}");
                //METADATA_BLOCK_HEADER
                //1-bit Last-metadata-block flag
                //7-bit BLOCK_TYPE
                //24-bit Length
                while (fs.Position < fs.Length)
                {
                    uint blockHeader = fs.BEInt32();
                    bool lastMetadataBlock = blockHeader >> 31 == 0x1;
                    BlockType blockType = (BlockType)((blockHeader >> 24) & 0x7f);
                    int length = (int) (blockHeader & 0xffffff);
                    info.TrueLength -= length;
                    OnLog?.Invoke($"|+{blockType} with Length: {length}");
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
                        throw new ArgumentOutOfRangeException($"Invalid BLOCK_TYPE: 0x{blockType:X2}");
                    }
                    if (lastMetadataBlock) break;
                }
                return info;
            }
        }

        private static void ParseStreamInfo(Stream fs, ref FlacInfo info)
        {
            long minBlockSize = fs.BEInt16();
            long maxBlockSize = fs.BEInt16();
            long minFrameSize = fs.BEInt24();
            long maxFrameSize = fs.BEInt24();
            var buffer = fs.ReadBytes(8);
            BitReader br = new BitReader(buffer);
            int sampleRate = (int) br.GetBits(20);
            int channelCount = (int) br.GetBits(3)+1;
            int bitPerSample = (int) br.GetBits(5)+1;
            int totalSample = (int) br.GetBits(36);
            var md5 = fs.ReadBytes(16);
            info.RawLength = channelCount * bitPerSample / 8 * totalSample;
            OnLog?.Invoke($" | minimum block size: {minBlockSize}, maximum block size: {maxBlockSize}");
            OnLog?.Invoke($" | minimum frame size: {minFrameSize}, maximum frame size: {maxFrameSize}");
            OnLog?.Invoke($" | Sample rate: {sampleRate}Hz, bits per sample: {bitPerSample}-bit");
            OnLog?.Invoke($" | Channel count: {channelCount}");
            string md5String = md5.Aggregate("", (current, item) => current + $"{item:X2}");
            OnLog?.Invoke($" | MD5: {md5String}");
        }

        private static void ParseVorbisComment(Stream fs, ref FlacInfo info)
        {
            //only here in flac use little-endian
            int vendorLength = (int) fs.LEInt32();
            var vendorRawStringData = fs.ReadBytes(vendorLength);
            var vendor = Encoding.UTF8.GetString(vendorRawStringData, 0, vendorLength);
            info.Encoder = vendor;
            OnLog?.Invoke($" | Vendor: {vendor}");
            int userCommentListLength = (int) fs.LEInt32();
            for (int i = 0; i < userCommentListLength; ++i)
            {
                int commentLength = (int) fs.LEInt32();
                var commentRawStringData = fs.ReadBytes(commentLength);
                var comment = Encoding.UTF8.GetString(commentRawStringData, 0, commentLength);
                var spilterIndex = comment.IndexOf('=');
                var key = comment.Substring(0, spilterIndex);
                var value = comment.Substring(spilterIndex + 1, comment.Length - 1 - spilterIndex);
                info.VorbisComment[key] = value;
                var summary = value.Length > 25 ? value.Substring(0, 25) + "..." : value;
                OnLog?.Invoke($" | [{key}] = '{summary.Replace('\n', ' ')}'");
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
            int pictureType = (int) fs.BEInt32();
            int mimeStringLength = (int) fs.BEInt32();
            string mimeType = Encoding.ASCII.GetString(fs.ReadBytes(mimeStringLength), 0, mimeStringLength);
            int descriptionLength = (int) fs.BEInt32();
            string description = Encoding.UTF8.GetString(fs.ReadBytes(descriptionLength), 0, descriptionLength);
            int pictureWidth = (int) fs.BEInt32();
            int pictureHeight = (int) fs.BEInt32();
            int colorDepth = (int) fs.BEInt32();
            int indexedColorCount = (int) fs.BEInt32();
            int pictureDataLength = (int) fs.BEInt32();
            fs.Seek(pictureDataLength, SeekOrigin.Current);
            info.TrueLength -= pictureDataLength;
            info.HasCover = true;
            if (pictureType > 20) pictureType = 21;
            OnLog?.Invoke($" | picture type: {PictureTypeName[pictureType]}");
            OnLog?.Invoke($" | picture format type: {mimeType}");
            if (descriptionLength > 0)
                OnLog?.Invoke($" | description: {description}");
            OnLog?.Invoke($" | attribute: {pictureWidth}px*{pictureHeight}px@{colorDepth}-bit");
            if (indexedColorCount != 0)
                OnLog?.Invoke($" | indexed-color color: {indexedColorCount}");
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
            bool ret = ((_buffer[_bytePosition] >> (7 - _bitPositionInByte)) & 1) == 1;
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
            for (int i = 0; i < length; ++i)
            {
                Next();
            }
        }

        public long GetBits(int length)
        {
            long ret = 0;
            for (int i = 0; i < length; ++i)
            {
                ret |= ((long) (_buffer[_bytePosition] >> (7 - _bitPositionInByte)) & 1) << (length - 1 - i);
                Next();
            }
            return ret;
        }
    }
}
