using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using PhilLibX.Compression;
using PhilLibX.IO;

namespace ModernModellingWarfare
{
    /// <summary>
    /// A class to handle an XPAK File from Call of Duty
    /// </summary>
    internal unsafe class XPAKPackage : IDisposable
    {
        public class XPAKPackageEntry
        {
            /// <summary>
            /// Gets or Sets the Hash Key
            /// </summary>
            public ulong Key { get; set; }

            /// <summary>
            /// Gets or Sets the total size of the data
            /// </summary>
            public long Size { get; set; }

            /// <summary>
            /// Gets or sets the offset to the data
            /// </summary>
            public long Offset { get; set; }

            /// <summary>
            /// Gets or Sets the package this entry belongs to
            /// </summary>
            public XPAKPackage Package { get; set; }
        }

        /// <summary>
        /// XPAK Section Info
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        unsafe struct XPAKSection
        {
            public long Count;
            public long Offset;
            public long SizeInBytes;
        }

        /// <summary>
        /// XPAK Index Entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        unsafe struct XPAKIndexEntry
        {
            public ulong Key;
            public long Offset;
            public long SizeInBytes;
        }

        /// <summary>
        /// XPAK Magic Number 'KAPI'/'IPAK'
        /// </summary>
        public const uint MagicNumber = 0x4950414B;

        /// <summary>
        /// Gets or Sets the File Name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or Sets the Package Reader
        /// </summary>
        public BinaryReader Reader { get; set; }

        public static byte[] Decompress(BinaryReader chunkReader, long length, int rawSize)
        {
            byte[] output = new byte[rawSize];

            Span<int> commands = stackalloc int[30];

            var remaining = rawSize;

            fixed (byte* p = &output[0])
            {
                while (chunkReader.BaseStream.Position < chunkReader.BaseStream.Length)
                {
                    var count = chunkReader.ReadInt32();
                    var offset = chunkReader.ReadInt32();

                    chunkReader.ReadStructArray(ref commands);

                    for (int i = 0; i < count; i++)
                    {
                        var chunkSize = commands[i] & 0xFFFFFF;
                        var chunkComp = commands[i] >> 24;

                        if (chunkComp == 0x6)
                        {
                            var blockSize = remaining < 0x3FFE0 ? remaining : 0x3FFE0;
                            Oodle.Decompress(chunkReader.ReadBytes(chunkSize), 0, chunkSize, output, offset, blockSize);
                            remaining -= blockSize;
                            offset += blockSize;

                            chunkReader.BaseStream.Seek(((chunkSize + 3) & 0xFFFFFFFC) - chunkSize, SeekOrigin.Current);
                        }
                        else if (chunkComp == 0)
                        {
                            chunkReader.Read(output, offset, chunkSize);
                            remaining -= chunkSize;
                            offset += chunkSize;
                        }
                        else
                        {
                            chunkReader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                        }
                    }

                    chunkReader.BaseStream.Seek((chunkReader.BaseStream.Position + 0x7F) & 0xFFFFFF80, SeekOrigin.Begin);
                }
            }

            return output;
        }

        public byte[] Extract(XPAKPackageEntry xpakEntry, int rawSize)
        {
            // Read the entire buffer, and then process
            byte[] buffer = new byte[xpakEntry.Size];

            lock (Reader)
            {
                Reader.BaseStream.Position = xpakEntry.Offset;
                Reader.BaseStream.Read(buffer, 0, buffer.Length);
            }

            using var chunkReader = new BinaryReader(new MemoryStream(buffer));

            return Decompress(chunkReader, chunkReader.BaseStream.Length, rawSize);
        }

        public XPAKPackage(Stream stream)
        {
            Load(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public void Load(Stream stream)
        {
            Reader = new BinaryReader(stream);

            if (Reader.ReadInt32() != MagicNumber)
                throw new Exception();

            var version = Reader.ReadUInt32();

            // MW19: Skip Checksum
            if (version == 0xD0000)
                Reader.BaseStream.Seek(312, SeekOrigin.Begin);

            var dataSection  = Reader.ReadStruct<XPAKSection>();
            var indexSection = Reader.ReadStruct<XPAKSection>();

            if (dataSection.SizeInBytes == 0)
                return;
            if (indexSection.SizeInBytes == 0)
                return;

            foreach (var entry in Reader.ReadStructArray<XPAKIndexEntry>((int)indexSection.Count, indexSection.Offset))
            {
                MaterialCacheHandler.Entries[entry.Key] = new XPAKPackageEntry()
                {
                    Key    = entry.Key,
                    Offset = entry.Offset + dataSection.Offset,
                    Size   = entry.SizeInBytes & 0xFFFFFFFFFFFFFF,
                    Package = this,
                };
            }
        }

        public bool IsValid(string fileName, string extension, byte[] headerBuffer)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Reader?.Dispose();
        }
    }
}
