using PhilLibX.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ModernModellingWarfare
{
    public class FastFileStream : Stream, IDisposable
    {
        private struct XFileBlock
        {
            public long VirtualStartOffset { get; set; }

            public long VirtualEndOffset { get; set; }

            public long ArchiveOffset { get; set; }

            public int EncodedSize { get; set; }

            public int ContentSize { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FFHeader
        {
            public ulong Magic;
            public uint Version;
            public uint GameVersion;
            public unsafe fixed byte ResidentHeader[32892];
            public int DecodedSize;
            public uint UnkValue;
        }

        [StructLayout(LayoutKind.Sequential, Size = 12)]
        public struct BlockHeader
        {
            public int EncodedSize;
            public int ContentSize;
        }

        /// <inheritdoc/>
        private byte[] Cache;

        /// <inheritdoc/>
        private long CacheStartPosition;

        /// <inheritdoc/>
        private long CacheEndPosition;

        /// <inheritdoc/>
        public long InternalPosition;

        /// <inheritdoc/>
        public long InternalLength;

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => InternalLength;

        /// <inheritdoc/>
        public override long Position
        {
            get => InternalPosition;
            set => InternalPosition = value;
        }

        public Stream InternalStream { get; set; }

        private FFHeader Header { get; set; }

        public override void Flush()
        {
        }

        public new void Dispose()
        {
            InternalStream?.Dispose();
        }

        private List<XFileBlock> Blocks { get; } = new List<XFileBlock>();

        public FastFileStream(string name)
        {
            InternalStream = File.OpenRead(name);
            Initialize();
        }

        private unsafe void Initialize()
        {
            Span<byte> span1 = stackalloc byte[Unsafe.SizeOf<FFHeader>()];
            if (InternalStream.Read(span1) < span1.Length)
                throw new IOException();

            Header = MemoryMarshal.Cast<byte, FFHeader>(span1)[0];

            if (Header.Magic != 3472329607102224201UL)
                throw new Exception();
            if (Header.Version != 0xB)
                throw new Exception();

            Span<byte> blockHeaderBuffer = stackalloc byte[Unsafe.SizeOf<BlockHeader>()];
            int consumed = 0;

            while (consumed != Header.DecodedSize)
            {
                Global.DebugPrint($"Registering Fast File Block @ {InternalStream.Position}");

                if (InternalStream.Read(blockHeaderBuffer) < blockHeaderBuffer.Length)
                    throw new IOException();

                BlockHeader blockHeader = MemoryMarshal.Cast<byte, BlockHeader>(blockHeaderBuffer)[0];

                Blocks.Add(new XFileBlock()
                {
                    VirtualStartOffset = consumed,
                    VirtualEndOffset   = consumed + blockHeader.ContentSize,
                    ArchiveOffset      = InternalStream.Position,
                    ContentSize        = blockHeader.ContentSize,
                    EncodedSize        = blockHeader.EncodedSize
                });

                consumed += blockHeader.ContentSize;
                InternalStream.Position += (blockHeader.EncodedSize + 3) & 1152921504606846972L;
            }

            InternalLength = consumed;
            InternalPosition = 0L;

            Global.DebugPrint($"Registered {Blocks.Count} Blocks in Fast File");
        }


        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "");
            if (buffer.Length - offset < count)
                throw new ArgumentException("");

            long readStartPos = Position;

            if (readStartPos >= Length)
                return 0;

            int remaining = count;
            int consumed = 0;

            while (true)
            {
                long num3 = readStartPos;
                long cacheRemaining = CacheEndPosition - readStartPos;

                // Check if we can take from the cache
                if (cacheRemaining > 0L && Cache != null && CacheStartPosition <= readStartPos && CacheEndPosition > readStartPos)
                {
                    int srcOffset = (int)(readStartPos - CacheStartPosition);
                    int numBytes = (int)Math.Min(remaining, cacheRemaining);
                    if (numBytes <= 8)
                    {
                        int idx = numBytes;
                        while (--idx >= 0)
                            buffer[offset + idx] = Cache[srcOffset + idx];
                    }
                    else
                    {
                        Buffer.BlockCopy(Cache, srcOffset, buffer, offset, numBytes);
                    }

                    remaining -= numBytes;
                    Position  += numBytes;
                    offset    += numBytes;
                    consumed  += numBytes;
                }

                // We still have some buffer left
                if (remaining != 0)
                {
                    readStartPos = Position;

                    if (readStartPos < Length)
                    {
                        // Locate a block that is within our range
                        var xfileBlock = Blocks.FirstOrDefault(x => readStartPos >= x.VirtualStartOffset && readStartPos < x.VirtualEndOffset);

                        // We can never have an archive offset of 0
                        if (xfileBlock.ArchiveOffset != 0)
                        {
                            InternalStream.Position = xfileBlock.ArchiveOffset;
                            Cache                   = new byte[xfileBlock.ContentSize];
                            CacheStartPosition      = xfileBlock.VirtualStartOffset;
                            CacheEndPosition        = xfileBlock.VirtualEndOffset;
                            byte[] buffer1          = new byte[xfileBlock.EncodedSize];

                            if (InternalStream.Read(buffer1) == buffer1.Length)
                            {
                                Oodle.Decompress(buffer1, 0, buffer1.Length, Cache, 0, Cache.Length);
                            }
                            else
                            {
                                throw new IOException("Fast File block extends outside range of the file");
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return consumed;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    InternalPosition = offset;
                    break;
                case SeekOrigin.Current:
                    InternalPosition += offset;
                    break;
                case SeekOrigin.End:
                    InternalPosition = Length - offset;
                    break;
            }
            return Position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException("Fast File's cannot be edited.");

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("Fast File's cannot be edited.");
    }
}
