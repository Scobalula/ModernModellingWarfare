// ------------------------------------------------------------------------
// CascLib.NET - A pure C# Casc Storage Handler
// Copyright(c) 2021 Philip/Scobalula
// ------------------------------------------------------------------------
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// ------------------------------------------------------------------------
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CascLib.NET
{
    /// <summary>
    /// A class to handle loading and storing Key Mapping Tables (.idx)
    /// </summary>
    public class CascKeyMappingTable
    {
        /// <summary>
        /// A class to handle parsing and storing a Key Mapping Table Entry
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// Gets the Parent Key Mapping Table
            /// </summary>
            public CascKeyMappingTable Parent { get; private set; }

            /// <summary>
            /// Gets or Sets the Encoding Key
            /// </summary>
            public byte[] EncodingKey { get; private set; }

            /// <summary>
            /// Gets or Sets the Offset
            /// </summary>
            public long Offset { get; private set; }

            /// <summary>
            /// Gets the Size of the Data
            /// </summary>
            public int Size { get; private set; }

            /// <summary>
            /// Gets or Sets the Archive Index
            /// </summary>
            public int ArchiveIndex { get; private set; }

            /// <summary>
            /// Initializes a new Key Mapping Table Entry
            /// </summary>
            /// <param name="buffer">Entry Buffer</param>
            /// <param name="table">Parent Table</param>
            public Entry(Span<byte> buffer, CascKeyMappingTable table)
            {
                long packedOffsetAndIndex = 0;

                packedOffsetAndIndex = (packedOffsetAndIndex << 0x08) | buffer[0 + table.EncodingKeyLength];
                packedOffsetAndIndex = (packedOffsetAndIndex << 0x08) | buffer[1 + table.EncodingKeyLength];
                packedOffsetAndIndex = (packedOffsetAndIndex << 0x08) | buffer[2 + table.EncodingKeyLength];
                packedOffsetAndIndex = (packedOffsetAndIndex << 0x08) | buffer[3 + table.EncodingKeyLength];
                packedOffsetAndIndex = (packedOffsetAndIndex << 0x08) | buffer[4 + table.EncodingKeyLength];

                Size = (Size << 0x08) | buffer[3 + table.EncodingKeyLength + table.StorageOffsetLength];
                Size = (Size << 0x08) | buffer[2 + table.EncodingKeyLength + table.StorageOffsetLength];
                Size = (Size << 0x08) | buffer[1 + table.EncodingKeyLength + table.StorageOffsetLength];
                Size = (Size << 0x08) | buffer[0 + table.EncodingKeyLength + table.StorageOffsetLength];

                ArchiveIndex = (int)(packedOffsetAndIndex >> table.FileOffsetBits);
                Offset       = packedOffsetAndIndex & table.FileOffsetMask;

                EncodingKey = buffer.Slice(0, table.EncodingKeyLength).ToArray();

                Parent = table;
            }
        }

        /// <summary>
        /// Gets the Version
        /// </summary>
        public ushort Version { get; private set; }

        /// <summary>
        /// Gets the Bucket Index
        /// </summary>
        public byte BucketIndex { get; private set; }

        /// <summary>
        /// Gets the Extra Byte
        /// </summary>
        public byte ExtraByte { get; private set; }

        /// <summary>
        /// Gets the Encoded Size Length
        /// </summary>
        public byte EncodedSizeLength { get; private set; }

        /// <summary>
        /// Gets the Storage Offset Length
        /// </summary>
        public byte StorageOffsetLength { get; private set; }

        /// <summary>
        /// Gets the length of the EKey
        /// </summary>
        public byte EncodingKeyLength { get; private set; }

        /// <summary>
        /// Gets the File Offset Bits
        /// </summary>
        public byte FileOffsetBits { get; private set; }

        /// <summary>
        /// Gets the File Offset Mask
        /// </summary>
        public long FileOffsetMask { get; private set; }

        /// <summary>
        /// Gets the File Size
        /// </summary>
        public long FileSize { get; private set; }

        /// <summary>
        /// Initializes a Key Mapping Table from a File
        /// </summary>
        /// <param name="storage">Parent Storage</param>
        /// <param name="fileName">File to load</param>
        public CascKeyMappingTable(CascStorage storage, string fileName)
        {
            using var reader = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 0x40000));

            var headerSize = reader.ReadInt32();
            var headerHash = reader.ReadInt32();

            Version                    = reader.ReadUInt16();
            BucketIndex                = reader.ReadByte();
            ExtraByte                  = reader.ReadByte();
            EncodedSizeLength          = reader.ReadByte();
            StorageOffsetLength        = reader.ReadByte();
            EncodingKeyLength          = reader.ReadByte();
            FileOffsetBits             = reader.ReadByte();
            FileOffsetMask             = (1 << FileOffsetBits) - 1;
            FileSize                   = reader.ReadInt64();

            if (EncodedSizeLength != 4 && EncodingKeyLength != 9 && StorageOffsetLength != 5)
                throw new Exception("Invalid Data Sizes in Key Mapping Table");

            // Align reader to next block
            reader.BaseStream.Position = (reader.BaseStream.Position + 0x17) & 0xFFFFFFF0;

            var tableSize = reader.ReadInt32();
            var tableHash = reader.ReadInt32();

            var entrySize = EncodedSizeLength + StorageOffsetLength + EncodingKeyLength;

            Span<byte> entryBuffer = stackalloc byte[entrySize];

            for (int bytesConsumed = 0; bytesConsumed < tableSize; bytesConsumed += entrySize)
            {
                var buffer = reader.Read(entryBuffer);

                var entry = new Entry(entryBuffer, this);

                storage.Entries[Convert.ToBase64String(entry.EncodingKey)] = entry;
            }
        }
    }
}
