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
        public Dictionary<string, string> VorbisComment { get; set; }

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
                    uint blockHeader = fs.ReadUInt();
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
            long minBlockSize = fs.ReadUShort();
            long maxBlockSize = fs.ReadUShort();
            long minFrameSize = fs.ReadInt24();
            long maxFrameSize = fs.ReadInt24();
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
            int vendorLength = (int) fs.ReadUInt(true);
            var vendorRawStringData = fs.ReadBytes(vendorLength);
            var vendor = Encoding.UTF8.GetString(vendorRawStringData, 0, vendorLength);
            info.Encoder = vendor;
            OnLog?.Invoke($" | Vendor: {vendor}");
            int userCommentListLength = (int) fs.ReadUInt(true);
            for (int i = 0; i < userCommentListLength; ++i)
            {
                int commentLength = (int) fs.ReadUInt(true);
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

        private static void ParsePicture(Stream fs, ref FlacInfo info)
        {
            int pictureType = (int) fs.ReadUInt();
            int mimeStringLength = (int) fs.ReadUInt();
            string mimeType = Encoding.ASCII.GetString(fs.ReadBytes(mimeStringLength), 0, mimeStringLength);
            int descriptionLength = (int) fs.ReadUInt();
            string description = Encoding.UTF8.GetString(fs.ReadBytes(descriptionLength), 0, descriptionLength);
            int pictureWidth = (int) fs.ReadUInt();
            int pictureHeight = (int) fs.ReadUInt();
            int colorDepth = (int) fs.ReadUInt();
            int indexedColorCount = (int) fs.ReadUInt();
            int pictureDataLength = (int) fs.ReadUInt();
            fs.Seek(pictureDataLength, SeekOrigin.Current);
            info.TrueLength -= pictureDataLength;
            info.HasCover = true;
            OnLog?.Invoke($" | picture type: {mimeType}");
            OnLog?.Invoke($" | attribute: {pictureWidth}px*{pictureHeight}px@{colorDepth}-bit");
        }

        private static ushort ReadUShort(this Stream fs)
        {
            var buffer = fs.ReadBytes(2).Reverse().ToArray();
            return BitConverter.ToUInt16(buffer, 0);
        }

        private static int ReadInt24(this Stream fs)
        {
            var buffer = fs.ReadBytes(3);
            int ret = 0;
            for (int i = 0; i < 3; ++i)
            {
                ret |= buffer[i] << ((2 - i) * 8);
            }
            return ret;
        }

        private static uint ReadUInt(this Stream fs, bool littleEndian = false)
        {
            var buffer = fs.ReadBytes(4);
            if (!littleEndian) buffer = buffer.Reverse().ToArray();
            return BitConverter.ToUInt32(buffer, 0);
        }

        private static byte[] ReadBytes(this Stream fs, int length)
        {
            var ret = new byte[length];
            fs.Read(ret, 0, length);
            return ret;
        }
    }

    public class BitReader
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

        public long GetBits(int length)
        {
            long ret = 0;
            for (int i = 0; i < length; ++i)
            {
                ret |= ((long)(_buffer[_bytePosition] >> (7 - _bitPositionInByte) & 1) << (length - 1 - i));
                Next();
            }
            return ret;
        }
    }
}
