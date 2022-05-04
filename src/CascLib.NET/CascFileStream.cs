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
using CascLib.NET.BlockTable;
using CascLib.NET.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CascLib.NET
{
    public class CascFileStream : Stream
    {
        /// <summary>
        /// Gets or Sets the File Spans
        /// </summary>
        public List<CascFileSpan> Spans { get; private set; }

        /// <summary>
        /// Internal File Size
        /// </summary>
        private readonly long InternalSize;

        /// <summary>
        /// Internal File Position
        /// </summary>
        private long InternalPosition;

        /// <summary>
        /// Whether the file is open or not (has been disposed)
        /// </summary>
        private bool IsOpen;

        /// <summary>
        /// Gets or Sets the Buffer Cache
        /// </summary>
        private byte[] Cache { get; set; }

        /// <summary>
        /// Gets or Sets the start position of the Cache
        /// </summary>
        private long CacheStartPosition { get; set; }

        /// <summary>
        /// Gets or Sets the start position of the Cache
        /// </summary>
        private long CacheEndPosition { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => IsOpen;

        /// <summary>
        /// Gets a value that indicates whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => IsOpen;

        /// <summary>
        /// Gets a value that indicates whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length => InternalSize;

        /// <summary>
        /// Gets or sets the current position of this stream.
        /// </summary>
        public override long Position
        {
            get
            {
                return InternalPosition;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spans"></param>
        internal CascFileStream(List<CascFileSpan> spans, long size, bool loadAllBytes = false)
        {
            Spans        = spans;
            InternalSize = size;
            IsOpen = true;

            // TODO: Implement Load All
            if(loadAllBytes)
            {
                Cache = new byte[size];

                foreach(var span in Spans)
                {
                    foreach(var frame in span.Frames)
                    {
                        span.SpanReader.BaseStream.Position = frame.ArchiveOffset;
                    }
                }
            }
        }

        /// <summary>
        /// Clears buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        public override void Flush() { }

        /// <summary>
        /// Reads a block of bytes from the strean and writes the data in a given buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // TODO: Fill in Exception Messages
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "");
            if (buffer.Length - offset < count)
                throw new ArgumentException("");

            var readStartPos = Position;

            // Our we outside the file?
            if (readStartPos >= Length)
                return 0;

            var toRead = count;
            var consumed = 0;

            while (true)
            {
                var readEndPos = readStartPos + offset;
                var cacheAvailable = CacheEndPosition - readStartPos;

                if (cacheAvailable > 0 && Cache != null)
                {
                    // We can take from the cache
                    if(CacheStartPosition <= readStartPos && CacheEndPosition > readStartPos)
                    {
                        var p = (int)(readStartPos - CacheStartPosition);
                        var n = (int)Math.Min(toRead, cacheAvailable);

                        if (n <= 8)
                        {
                            int byteCount = n;
                            while (--byteCount >= 0)
                                buffer[offset + byteCount] = Cache[p + byteCount];
                        }
                        else
                        {
                            Buffer.BlockCopy(Cache, p, buffer, offset, n);
                        }

                        toRead   -= n;
                        Position += n;
                        offset   += n;
                        consumed += n;
                    }
                }

                // We've satisfied what we need
                if (toRead == 0)
                    break;

                readStartPos = Position;

                // Our we outside the file?
                if (readStartPos >= Length)
                    break;

                // Find next span that is at the current position and request buffer
                var span = Spans.FirstOrDefault(x => readStartPos >= x.VirtualStartOffset && readStartPos < x.VirtualEndOffset);
                if (span == null)
                    throw new IOException();
                var frame = span.Frames.FirstOrDefault(x => readStartPos >= x.VirtualStartOffset && readStartPos < x.VirtualEndOffset);
                if (frame == null)
                    throw new IOException();

                // Lock the span reader for 
                lock (span.SpanReader)
                {
                    span.SpanReader.BaseStream.Position = frame.ArchiveOffset;

                    CacheStartPosition = frame.VirtualStartOffset;
                    CacheEndPosition = CacheStartPosition + frame.ContentSize;

                    var type = (BlockTableEncoderType)span.SpanReader.ReadByte();

                    switch(type)
                    {
                        case BlockTableEncoderType.Raw:
                            Cache = new byte[frame.ContentSize];
                            span.SpanReader.Read(Cache);
                            break;
                        case BlockTableEncoderType.ZLib:
                            // Skip header
                            Cache = ZLIB.Decompress(span.SpanReader.ReadBytes(frame.EncodedSize - 1), frame.ContentSize);
                            break;
                        default:
                            throw new Exception("Unsupported Block Table Type");
                    }
                }
            }

            return consumed;
        }

        /// <summary>
        /// Sets the current position of this stream to the given value.
        /// </summary>
        /// <param name="offset">The point relative to origin from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for offset, using a value of type <see cref="SeekOrigin"/>.</param>
        /// <returns>
        /// The new position in the stream.
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:      InternalPosition = offset; break;
                case SeekOrigin.Current:    InternalPosition += offset; break;
                case SeekOrigin.End:        InternalPosition = Length - offset; break;
            }

            return Position;
        }

        /// <summary>
        /// Sets the length of this stream to the given value.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("Cannot set the length of a Casc Stream");
        }

        /// <summary>
        /// Writes a block of bytes to the stream.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write to the stream.</param>
        /// <param name="offset">The zero-based byte offset in array from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Cannot write to a Casc Stream");
        }

        public override void Close()
        {
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    IsOpen = false;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
