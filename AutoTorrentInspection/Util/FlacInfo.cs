using System.IO;
using FlacLibSharp;

namespace AutoTorrentInspection.Util
{
    public class FlacInfo
    {
        private long RawLength { get; }

        private long TrueLength { get; }

        public double CompressRate { get; }

        public bool HasCover { get; }

        public string Encoder { get; }

        public FlacInfo(string path)
        {
            using (FlacFile flac = new FlacFile(path))
            {
                RawLength = flac.StreamInfo.Samples * (flac.StreamInfo.BitsPerSample / 8) * flac.StreamInfo.Channels;
                TrueLength = new FileInfo(path).Length;
                Encoder = flac.VorbisComment.Vendor;

                long metaDataLength = 0;

                foreach (MetadataBlock metaData in flac.Metadata)
                {
                    metaDataLength += metaData.Header.MetaDataBlockLength;
                    var picture = metaData as Picture;
                    if (picture != null)
                    {
                        HasCover = true;
                        metaDataLength += picture.Data.LongLength;
                    }
                    var padding = metaData as Padding;
                    if (padding != null)
                    {
                        metaDataLength += padding.EmptyBitCount / 8;
                    }
                }
                CompressRate = (TrueLength - metaDataLength) / (double) RawLength;
            }
        }
    }
}