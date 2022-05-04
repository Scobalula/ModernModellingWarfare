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
using System.IO.Compression;
using System.Text;

namespace CascLib.NET.Utility
{
    /// <summary>
    /// A class to handle decompressing Zlib data
    /// </summary>
    internal static class ZLIB
    {
        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="input">Input compressed data</param>
        /// <param name="decompressedSize">Size of the decompressed data</param>
        /// <param name="dataOffset">Offset to the Zlib/Deflate data (defaults to 2 to skip Zlib header)</param>
        /// <returns>Resulting data decompressed</returns>
        public static byte[] Decompress(byte[] input, int decompressedSize, int dataOffset = 2)
        {
            using var stream = new MemoryStream(decompressedSize);
            using var compressed = new MemoryStream(input)
            {
                Position = dataOffset
            };
            using var deflater = new DeflateStream(compressed, CompressionMode.Decompress);
            deflater.CopyTo(stream);
            return stream.ToArray();
        }
    }
}
