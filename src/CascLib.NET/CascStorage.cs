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
// File: CascStorage.cs
// Author: Philip/Scobalula
// Description: Main Casc Storage Handler
// ------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CascLib.NET.BlockTable;
using CascLib.NET.RootHandlers;
using CascLib.NET.Utility;

namespace CascLib.NET
{
    /// <summary>
    /// A class to handle interacting with a local Casc Storage
    /// </summary>
    public class CascStorage : IDisposable
    {
        /// <summary>
        /// Gets the Index Entries
        /// </summary>
        public Dictionary<string, CascKeyMappingTable.Entry> Entries { get; } = new Dictionary<string, CascKeyMappingTable.Entry>();

        /// <summary>
        /// Gets the Key Mapping Tables
        /// </summary>
        public List<CascKeyMappingTable> KeyMappingTables { get; } = new List<CascKeyMappingTable>();

        /// <summary>
        /// Gets the Root Handler
        /// </summary>
        public CascRootHandler RootHandler { get; private set; }

        /// <summary>
        /// Gets the Build Info
        /// </summary>
        public CascBuildInfo BuildInfo { get; } = new CascBuildInfo();

        /// <summary>
        /// Gets the Config
        /// </summary>
        public CascConfig Config { get; } = new CascConfig();

        /// <summary>
        /// Gets the Storage Path
        /// </summary>
        public string StoragePath { get; private set; }

        /// <summary>
        /// Gets the Data Path
        /// </summary>
        public string DataPath { get; private set; }

        /// <summary>
        /// Gets the Data Files
        /// </summary>
        internal BinaryReader[] DataFiles { get; private set; }

        /// <summary>
        /// Gets the Files within the Storage
        /// </summary>
        public CascFileInfo[] Files { get; private set; }

        /// <summary>
        /// Attempts to locate and load build info
        /// </summary>
        private void LoadBuildInfo()
        {
            foreach(var file in Directory.EnumerateFiles(StoragePath, ".build.info", SearchOption.AllDirectories))
            {
                // Break on first instance of it
                BuildInfo.Load(file);
                return;
            }

            throw new Exception("Failed to locate Build Info");
        }

        /// <summary>
        /// Attempts to locate and load build info
        /// </summary>
        private void LoadConfigInfo()
        {
            foreach (var file in Directory.EnumerateFiles(StoragePath, BuildInfo.Get("Build Key", null), SearchOption.AllDirectories))
            {
                // Break on first instance of it
                Config.Load(file);
                return;
            }

            throw new Exception("Failed to locate Build Info");
        }

        public CascStorage(string folder, CascRootHandler handler = null)
        {
            StoragePath = folder;
            DataPath = Path.Combine(StoragePath, "Data", "data");

            LoadBuildInfo();
            LoadConfigInfo();

            foreach (var file in Directory.EnumerateFiles(Path.Combine(StoragePath, "Data", "data"), "*.idx"))
                KeyMappingTables.Add(new CascKeyMappingTable(this, file));

            // We require the data file sizes to build our array
            var dataFileNames = Directory.GetFiles(Path.Combine(StoragePath, "Data", "data"), "data.*");

            DataFiles = new BinaryReader[dataFileNames.Length];

            foreach(var dataFileName in dataFileNames)
            {
                // Use the extension as our index
                var index = int.Parse(dataFileName.Split('.').Last(), System.Globalization.NumberStyles.Any);

                if (index > DataFiles.Length)
                    throw new ArgumentOutOfRangeException(nameof(index), "Invalid Data File Index");

                DataFiles[index] = new BinaryReader(
                    File.Open(dataFileName,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite));
            }


            Span<byte> headerBuf = stackalloc byte[4];

            if (Entries.TryGetValue(Convert.ToBase64String(HexString.ToByteArray(Config.Get("vfs-root").Values[1]), 0, 9), out var val))
            {
                using var stream = OpenFile(val);
                stream.Read(headerBuf);
                stream.Position = 0;

                if (handler == null)
                {
                    RootHandler = (BitConverter.ToUInt32(headerBuf)) switch
                    {
                        0x53465654 => new TVFSRootHandler(),
                        _ => throw new Exception(),
                    };
                }
                else
                {
                    RootHandler = handler;
                }

                RootHandler.Parse(stream);
            }
            else
            {
                throw new Exception("");
            }

            List<CascFileInfo> results = new List<CascFileInfo>(RootHandler.FileEntries.Count);

            foreach (var entry in RootHandler.FileEntries)
            {
                var info = new CascFileInfo
                {
                    FileName = entry.Key,
                    IsLocal = true
                };

                foreach (var spanInfo in entry.Value.Spans)
                {
                    // Check if the file exists, if not, it's either localized or online
                    if(Entries.TryGetValue(spanInfo.Base64EncodingKey, out var entry1))
                    {
                        info.FileSize += entry1.Size;
                    }
                    else
                    {
                        info.IsLocal = false;
                        info.FileSize = 0;
                        break;
                    }
                }

                results.Add(info);
            }

            Files = results.ToArray();
        }

        public CascFileStream OpenFile(string fileName)
        {
            if(RootHandler.FileEntries.TryGetValue(fileName, out var entry))
            {
                long virtualOffset = 0;

                var spans = new List<CascFileSpan>();

                foreach (var span in entry.Spans)
                {
                    var newSpan = new CascFileSpan();

                    // Locate entry by EKey
                    if (Entries.TryGetValue(span.Base64EncodingKey, out var e))
                    {
                        newSpan.SpanReader = DataFiles[e.ArchiveIndex];

                        newSpan.SpanReader.BaseStream.Position = e.Offset;

                        var spanner = newSpan.SpanReader.ReadStruct<CascSpanHeader>();
                        var header = newSpan.SpanReader.ReadStruct<BlockTableHeader>();

                        if (header.Signature != 0x45544C42)
                        {
                            throw new Exception();
                        }

                        var blteFrames = newSpan.SpanReader.ReadStructArray<BlockTableEntry>(header.FrameCount);
                        var archiveOffset = newSpan.SpanReader.BaseStream.Position;

                        newSpan.ArchiveOffset = archiveOffset;
                        newSpan.VirtualStartOffset = virtualOffset;
                        newSpan.VirtualEndOffset = virtualOffset;

                        foreach (var blteFrame in blteFrames)
                        {
                            var frame = new CascFileFrame
                            {

                                // Track archive offset
                                ArchiveOffset = archiveOffset,
                                EncodedSize = blteFrame.EncodedSize,
                                ContentSize = blteFrame.ContentSize,
                                VirtualStartOffset = virtualOffset
                            };

                            frame.VirtualEndOffset = virtualOffset + frame.ContentSize;

                            newSpan.VirtualEndOffset += frame.ContentSize;

                            archiveOffset += blteFrame.EncodedSize;
                            virtualOffset += blteFrame.ContentSize;

                            newSpan.Frames.Add(frame);
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }

                    spans.Add(newSpan);
                }

                return new CascFileStream(spans, virtualOffset);

            }

            return null;
        }

        public CascFileStream OpenFile(CascKeyMappingTable.Entry entry)
        {
            long virtualOffset = 0;

            var spans = new List<CascFileSpan>();
            var newSpan = new CascFileSpan();

            spans.Add(newSpan);

            newSpan.SpanReader = DataFiles[entry.ArchiveIndex];

            newSpan.SpanReader.BaseStream.Position = entry.Offset;

            var spanner = newSpan.SpanReader.ReadStruct<CascSpanHeader>();
            var header = newSpan.SpanReader.ReadStruct<BlockTableHeader>();

            if (header.Signature != 0x45544C42)
            {
                throw new Exception();
            }

            var blteFrames = newSpan.SpanReader.ReadStructArray<BlockTableEntry>(header.FrameCount);
            var archiveOffset = newSpan.SpanReader.BaseStream.Position;

            newSpan.ArchiveOffset = archiveOffset;
            newSpan.VirtualStartOffset = virtualOffset;
            newSpan.VirtualEndOffset = virtualOffset;

            foreach (var blteFrame in blteFrames)
            {
                var frame = new CascFileFrame();

                // Track archive offset
                frame.ArchiveOffset = archiveOffset;
                frame.EncodedSize = blteFrame.EncodedSize;
                frame.ContentSize = blteFrame.ContentSize;
                frame.VirtualStartOffset = virtualOffset;
                frame.VirtualEndOffset = virtualOffset + frame.ContentSize;

                newSpan.VirtualEndOffset += frame.ContentSize;


                archiveOffset += blteFrame.EncodedSize;

                // Append to virtual offset
                virtualOffset += blteFrame.ContentSize;

                newSpan.Frames.Add(frame);
            }

            return new CascFileStream(spans, virtualOffset);
        }

        public void Dispose()
        {
        }
    }
}
