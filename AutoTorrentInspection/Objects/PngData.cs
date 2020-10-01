using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoTorrentInspection.Objects
{
    public class PngInfo
    {
        public long FileSize { get; set; }
        public double CompressRate { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public byte BitDepth { get; set; }
        public PngColourType ColourType { get; set; }
        public byte CompressionMethod { get; set; }
        public byte FilterMethod { get; set; }
        public byte InterlaceMethod { get; set; }

        public long GetRawSize()
        {
            if (ColourType == PngColourType.IndexedColour)
            {
                return -FileSize;
            }
            var size = (long)Width * Height;
            var channel = (ColourType & PngColourType.Truecolour) != 0 ? 3 : 1;
            channel += (ColourType & PngColourType.GreyscaleWithAlpha) != 0 ? 1 : 0;
            var rawSize = (long) Math.Floor(BitDepth / 8.0M * size * channel);
            return rawSize;
        }
    }

    [Flags]
    public enum PngColourType
    {
        Greyscale=0,
        Truecolour=2,
        IndexedColour=3,
        GreyscaleWithAlpha=4,
        TruecolourWithAlpha=6
    }

    public static class PngData
    {
        public static PngInfo GetMetadataFrom(string pngPath)
        {
            var pngInfo = new PngInfo();
            using (var fs = File.OpenRead(pngPath))
            {
                pngInfo.FileSize = fs.Length;
                var identifier = fs.ReadBytes(8);
                if (identifier.SequenceEqual(new byte[8] { 0x98, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }))
                {
                    throw new InvalidDataException($"Except an png but get an {Encoding.ASCII.GetString(identifier)}" +
                                                   $"{Environment.NewLine}File name: {Path.GetFileName(pngPath)}");
                }

                var length = fs.BEInt32();
                var chunkTypeCode = Encoding.ASCII.GetString(fs.ReadBytes(4));
                Debug.Assert(length == 13);
                Debug.Assert(chunkTypeCode == "IHDR");
                pngInfo.Width = fs.BEInt32();
                pngInfo.Height = fs.BEInt32();
                pngInfo.BitDepth = (byte) fs.ReadByte();
                pngInfo.ColourType = (PngColourType) fs.ReadByte();
                pngInfo.CompressionMethod = (byte) fs.ReadByte();
                pngInfo.FilterMethod = (byte) fs.ReadByte();
                pngInfo.InterlaceMethod = (byte) fs.ReadByte();
            }

            var rawSize = pngInfo.GetRawSize();
            // 通过单个文件例子测算的非压缩png的额外开销比例
            pngInfo.CompressRate = pngInfo.FileSize * 0.99820 / rawSize;
            return pngInfo;
        }
    }
}
