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
using CascLib.NET.Utility;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CascLib.NET.RootHandlers
{
    /// <summary>
    /// A class to handle TVFS Root Files
    /// </summary>
    public class TVFSRootHandler : CascRootHandler
    {
        /// <summary>
        /// TVFS Header
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TVFSHeader
        {
            public uint Signature;
            public byte FormatVersion;
            public byte HeaderSize;
            public byte EncodingKeySize;
            public byte PatchKeySize;
            public Int32BE Flags;
            public Int32BE PathTableOffset;
            public Int32BE PathTableSize;
            public Int32BE VFSTableOffset;
            public Int32BE VFSTableSize;
            public Int32BE CFTTableOffset;
            public Int32BE CFTTableSize;
            public ushort MaxDepth;
        }

        /// <summary>
        /// Gets or Sets the Base File Reader
        /// </summary>
        public BinaryReader FileReader { get; set; }

        /// <summary>
        /// Gets or Sets the Path Table Reader
        /// </summary>
        public BinaryReader PathTableReader { get; set; }

        /// <summary>
        /// Gets or Sets the VFS Table Reader
        /// </summary>
        public BinaryReader VFSTableReader { get; set; }

        /// <summary>
        /// Gets or Sets the CFT Table Reader
        /// </summary>
        public BinaryReader CFTTableReader { get; set; }

        /// <summary>
        /// Gets or Sets the TVFS Header
        /// </summary>
        private TVFSHeader Header { get; set; }

        public List<Entry> Entries { get; } = new List<Entry>();

        /// <summary>
        /// Adds a new entry to the File System
        /// </summary>
        /// <param name="name"></param>
        /// <param name="vfsInfoPos"></param>
        private void AddEntry(string name, long vfsInfoPos)
        {
            VFSTableReader.BaseStream.Position = vfsInfoPos;

            var entry = new Entry(name);

            var spanCount     = VFSTableReader.ReadByte();

            for(int i = 0; i < spanCount; i++)
            {
                var refFileOffset = VFSTableReader.ReadStruct<Int32BE>();
                var sizeOfSpan    = VFSTableReader.ReadStruct<Int32BE>();
                var cftOffset     = VFSTableReader.ReadVariableSizeInt(Header.CFTTableSize);

                CFTTableReader.BaseStream.Position = cftOffset;

                entry.Spans.Add(new Entry.SpanInfo(CFTTableReader.ReadBytes(Header.EncodingKeySize)));
            }

            FileEntries[name] = entry;
        }

        /// <summary>
        /// Path Table Node Flags
        /// </summary>
        enum PathTableNodeFlags : int
        {
            None              = 0x0000,
            PathSeparatorPre  = 0x0001,
            PathSeparatorPost = 0x0002,
            IsNodeValue       = 0x0004,
        }

        struct PathTableNode
        {
            /// <summary>
            /// Gets or Sets the Entry Name
            /// </summary>
            public char[] Name { get; set; }

            /// <summary>
            /// Gets or Sets the Flags
            /// </summary>
            public PathTableNodeFlags Flags { get; set; }

            /// <summary>
            /// Gets or Sets the Value
            /// </summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Parses a Path Node from the Path Table
        /// </summary>
        /// <returns></returns>
        private PathTableNode ParsePathNode()
        {
            var entry = new PathTableNode();

            var buf = PathTableReader.PeekByte();

            if (buf == 0)
            {
                entry.Flags |= PathTableNodeFlags.PathSeparatorPre;
                PathTableReader.BaseStream.Position++;
                buf = PathTableReader.PeekByte();
            }

            if(buf < 0x7F && buf != 0xFF)
            {
                PathTableReader.BaseStream.Position++;
                entry.Name = PathTableReader.ReadChars(buf);
                buf = PathTableReader.PeekByte();
            }

            if(buf == 0)
            {
                entry.Flags |= PathTableNodeFlags.PathSeparatorPost;
                PathTableReader.BaseStream.Position++;
                buf = PathTableReader.PeekByte();
            }

            if(buf == 0xFF)
            {
                PathTableReader.BaseStream.Position++;
                entry.Value = PathTableReader.ReadStruct<Int32BE>();
                entry.Flags |= PathTableNodeFlags.IsNodeValue;
            }
            else
            {
                entry.Flags |= PathTableNodeFlags.PathSeparatorPost;
            }

            return entry;
        }

        /// <summary>
        /// Parses
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="reader"></param>
        /// <param name="builder"></param>
        public void Parse(CascStorage storage, long end, StringBuilder builder = null)
        {
            var currentSize = builder.Length;

            while(PathTableReader.BaseStream.Position < end)
            {
                var entry = ParsePathNode();

                // Build name with the given flags
                if (entry.Flags.HasFlag(PathTableNodeFlags.PathSeparatorPre))
                    builder.Append('\\');
                builder.Append(entry.Name);
                if (entry.Flags.HasFlag(PathTableNodeFlags.PathSeparatorPost))
                    builder.Append('\\');

                if (entry.Flags.HasFlag(PathTableNodeFlags.IsNodeValue))
                {
                    if ((entry.Value & 0x80000000) != 0)
                    {
                        var folderSize = entry.Value & 0x7FFFFFFF;
                        var folderStart = PathTableReader.BaseStream.Position;
                        var folderEnd = folderStart + folderSize - 4;

                        Parse(null, folderEnd, builder);
                    }
                    else
                    {
                        AddEntry(builder.ToString(), entry.Value);
                    }

                    // Reset the builder back to our original position
                    builder.Remove(currentSize, builder.Length - currentSize);
                }
            }
        }

        /// <summary>
        /// Parses the data from the Casc Root File
        /// </summary>
        /// <param name="stream">Stream to load</param>
        public override void Parse(Stream stream)
        {
            stream.Position = 0;
            using var reader = new BinaryReader(stream);
            Header = reader.ReadStruct<TVFSHeader>();


            reader.BaseStream.Position = Header.PathTableOffset;
            PathTableReader            = new BinaryReader(new MemoryStream(reader.ReadBytes(Header.PathTableSize)));
            reader.BaseStream.Position = Header.VFSTableOffset;
            VFSTableReader             = new BinaryReader(new MemoryStream(reader.ReadBytes(Header.VFSTableSize)));
            reader.BaseStream.Position = Header.CFTTableOffset;
            CFTTableReader             = new BinaryReader(new MemoryStream(reader.ReadBytes(Header.CFTTableSize)));

            // File.WriteAllBytes("test_vfs.dat", ((MemoryStream)PathTableReader.BaseStream).ToArray());

            Parse(
                null,
                PathTableReader.BaseStream.Position + PathTableReader.BaseStream.Length,
                new StringBuilder(255));
        }

        //public void Dispose()
        //{
        //    FileReader?.Dispose();
        //    PathTableReader?.Dispose();
        //    VFSTableReader?.Dispose();
        //    CFTTableReader?.Dispose();
        //}


        public TVFSRootHandler() { }

        public TVFSRootHandler(Stream stream)
        {
            Parse(stream);
        }
    }
}
