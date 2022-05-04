using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace PhilLibX.IO
{
    /// <summary>
    /// Provides extension methods for <see cref="Stream"/> objects
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="hexString">Hex String with masks, for example: "1A FF ?? ?? 00"</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, string hexString)
        {
            return Scan(stream, new BytePattern(hexString), stream.Seek(-stream.Length, SeekOrigin.End), stream.Seek(0, SeekOrigin.End), false);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="hexString">Hex String with masks, for example: "1A FF ?? ?? 00"</param>
        /// <param name="firstOccurence">Whether or not to stop at the first result</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, string hexString, bool firstOccurence)
        {
            return Scan(stream, new BytePattern(hexString), stream.Seek(-stream.Length, SeekOrigin.End), stream.Seek(0, SeekOrigin.End), firstOccurence);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="hexString">Hex String with masks, for example: "1A FF ?? ?? 00"</param>
        /// <param name="startPosition">Position to start searching from</param>
        /// <param name="endPosition">Position to end the search at</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, string hexString, long startPosition, long endPosition)
        {
            return Scan(stream, new BytePattern(hexString), startPosition, endPosition, false);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="hexString">Hex String with masks, for example: "1A FF ?? ?? 00"</param>
        /// <param name="startPosition">Position to start searching from</param>
        /// <param name="endPosition">Position to end the search at</param>
        /// <param name="firstOccurence">Whether or not to stop at the first result</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, string hexString, long startPosition, long endPosition, bool firstOccurence)
        {
            return Scan(stream, new BytePattern(hexString), startPosition, endPosition, firstOccurence);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="pattern">Pattern to search for</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, BytePattern pattern)
        {
            return Scan(stream, pattern, stream.Seek(-stream.Length, SeekOrigin.End), stream.Seek(0, SeekOrigin.End), false);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="pattern">Pattern to search for</param>
        /// <param name="firstOccurence">Whether or not to stop at the first result</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, BytePattern pattern, bool firstOccurence)
        {
            return Scan(stream, pattern, stream.Seek(-stream.Length, SeekOrigin.End), stream.Seek(0, SeekOrigin.End), firstOccurence);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="pattern">Pattern to search for</param>
        /// <param name="startPosition">Position to start searching from</param>
        /// <param name="endPosition">Position to end the search at</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, BytePattern pattern, long startPosition, long endPosition)
        {
            return Scan(stream, pattern, startPosition, endPosition, false);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="pattern">Pattern to search for</param>
        /// <param name="startPosition">Position to start searching from</param>
        /// <param name="endPosition">Position to end the search at</param>
        /// <param name="firstOccurence">Whether or not to stop at the first result</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, BytePattern pattern, long startPosition, long endPosition, bool firstOccurence)
        {
            return Scan(stream, pattern.Needle, pattern.Mask, startPosition, endPosition, firstOccurence, 0x10000);
        }

        /// <summary>
        /// Scans for the given pattern in the stream 
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="needle">Byte Array Needle to search for</param>
        /// <param name="mask">Mask array for unknown bytes/pattern matching</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, byte[] needle, byte[] mask)
        {
            return Scan(stream, needle, mask, stream.Seek(-stream.Length, SeekOrigin.End), stream.Seek(0, SeekOrigin.End), false, 0x10000);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="needle">Byte Array Needle to search for</param>
        /// <param name="mask">Mask array for unknown bytes/pattern matching</param>
        /// <param name="firstOccurence">Whether or not to stop at the first result</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, byte[] needle, byte[] mask, bool firstOccurence)
        {
            return Scan(stream, needle, mask, stream.Seek(-stream.Length, SeekOrigin.End), stream.Seek(0, SeekOrigin.End), firstOccurence, 0x10000);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/>  
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="needle">Byte Array Needle to search for</param>
        /// <param name="mask">Mask array for unknown bytes/pattern matching</param>
        /// <param name="startPosition">Position to start searching from</param>
        /// <param name="endPosition">Position to end the search at</param>
        /// <returns>Absolute positions of occurences</returns>
        public static long[] Scan(this Stream stream, byte[] needle, byte[] mask, long startPosition, long endPosition)
        {
            return Scan(stream, needle, mask, startPosition, endPosition, false, 0x10000);
        }

        /// <summary>
        /// Scans for the given pattern in the <see cref="Stream"/> 
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="needle">Byte Array Needle to search for</param>
        /// <param name="mask">Mask array for unknown bytes/pattern matching</param>
        /// <param name="startPosition">Position to start searching from</param>
        /// <param name="endPosition">Position to end the search at</param>
        /// <param name="firstOccurence">Whether or not to stop at the first result</param>
        /// <param name="bufferSize">The size of the scan buffer.</param>
        /// <returns>Absolute positions of occurences</returns>
        public unsafe static long[] Scan(this Stream stream, byte[] needle, byte[] mask, long startPosition, long endPosition, bool firstOccurence, int bufferSize)
        {
            if (startPosition == -1)
                throw new IOException();
            if (endPosition == -1)
                throw new IOException();

            stream.Position = startPosition;

            long readBegin = stream.Position;

            List<long> offsets = new List<long>();

            int needleIndex = 0;
            var buffer = new byte[bufferSize];

            fixed (byte* n = needle)
            fixed (byte* m = mask)
            fixed (byte* p = buffer)
            {
                while (true)
                {
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead <= 0)
                        break;

                    for (int i = 0; i < bytesRead; i++)
                    {
                        if (n[needleIndex] == p[i] || m[needleIndex] == 0xFF)
                        {
                            needleIndex++;

                            if (needleIndex == needle.Length)
                            {
                                offsets.Add(readBegin + i + 1 - needle.Length);

                                needleIndex = 0;

                                if (firstOccurence)
                                {
                                    return offsets.ToArray();
                                }
                            }
                        }
                        else
                        {
                            needleIndex = 0;
                        }
                    }

                    if (stream.Position > endPosition)
                        break;

                    readBegin += bytesRead;
                }
            }

            return offsets.ToArray();
        }
    }
}
