// ------------------------------------------------------------------------
// PhilLibX - My Utility Library
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

namespace PhilLibX.Compression
{
    public static unsafe class LZ4
    {
        /// <summary>
        /// Calculates the maximum possible size for the given data size 
        /// </summary>
        /// <returns>Maximum possible size for the given data size </returns>
        public static int CompressBound(int size) => Interop.LZ4_compressBound(size);

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="data">Data to decompress</param>
        /// <param name="decompressedSize">Size of the decompressed data</param>
        /// <returns>Decompressed buffer</returns>
        public static byte[] Decompress(byte[] data, int decompressedSize)
        {
            if (decompressedSize == 0)
                return Array.Empty<byte>();

            var rawBuf = new byte[decompressedSize];

            fixed (byte* a = &data[0])
            {
                var rawBufSize = decompressedSize;

                fixed (byte* b = &rawBuf[0])
                {
                    var result = Interop.LZ4_decompress_safe(
                        a,
                        b,
                        data.Length,
                        rawBufSize);

                    if (result != rawBufSize)
                        throw new CompressionException($"Failed to decompress LZ4 Data");
                }
            }

            return rawBuf;
        }

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="data">Data to decompress</param>
        /// <param name="dataOffset">Offset within the buffer of the compressed data</param>
        /// <param name="dataSize">Size of the compressed data</param>
        /// <param name="decompressedData">Buffer to output to</param>
        /// <param name="decompressedOffset">Offset within the buffer to place the decompressed data</param>
        /// <param name="decompressedSize">Size of the decompressed data</param>
        public static void Decompress(byte[] data, int dataOffset, int dataSize, byte[] decompressedData, int decompressedOffset, int decompressedSize)
        {
            fixed (byte* a = &data[0])
            {
                var rawBufSize = decompressedSize;

                fixed (byte* b = &decompressedData[0])
                {
                    var result = Interop.LZ4_decompress_safe(
                        a + dataOffset,
                        b + decompressedOffset,
                        data.Length,
                        rawBufSize);

                    if (result != rawBufSize)
                        throw new CompressionException($"Failed to decompress LZ4 Data");
                }
            }
        }

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="data">Data to decompress</param>
        /// <param name="decompressedSize">Size of the decompressed data</param>
        /// <returns>Decompressed buffer</returns>
        public static byte[] Compress(byte[] data)
        {
            var temp = new byte[Interop.LZ4_compressBound(data.Length)];

            fixed (byte* a = &data[0])
            {
                var outputSize = temp.Length;

                fixed (byte* b = &temp[0])
                {
                    var result = Interop.LZ4_compress_default(
                        a,
                        b,
                        data.Length,
                        outputSize);

                    if (result == 0)
                        throw new CompressionException($"Failed to decompress LZ4 Data");

                    var output = new byte[result];
                    Buffer.BlockCopy(temp, 0, output, 0, result);

                    return output;
                }
            }
        }

        /// <summary>
        /// Compresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <param name="inputOffset">Offset within the buffer of the data</param>
        /// <param name="inputSize">Size of the data</param>
        /// <param name="outputBuffer">Buffer to output to</param>
        /// <param name="outputOffset">Offset within the buffer to place the data</param>
        /// <param name="outputSize">Size of the data</param>
        /// The total number of bytes placed into the buffer.
        /// This can be less than the number of bytes allocated in the buffer if that many bytes are not currently available,
        /// or zero (0) if the end of the data has been reached.</returns>
        public static int Compress(byte[] inputBuffer, int inputOffset, int inputSize, byte[] outputBuffer, int outputOffset, int outputSize)
        {
            if (inputBuffer is null)
                throw new ArgumentNullException(nameof(inputBuffer), "Input Buffer cannot be null");
            if (inputOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(inputOffset), "Input offset must be a non-negative number");
            if (inputSize < 0 || inputSize > inputBuffer.Length - inputOffset)
                throw new ArgumentOutOfRangeException(nameof(inputSize), "Input size is outside the bounds of the buffer");

            if (outputBuffer is null)
                throw new ArgumentNullException(nameof(outputBuffer), "Output Buffer cannot be null");
            if (outputOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(outputOffset), "Output offset must be a non-negative number");
            if (outputSize < 0 || outputSize > outputBuffer.Length - outputOffset)
                throw new ArgumentOutOfRangeException(nameof(outputSize), "Output size is outside the bounds of the buffer");

            fixed (byte* a = &inputBuffer[0])
            {
                int outputSizeResult = outputSize;

                fixed (byte* b = &outputBuffer[0])
                {
                    var result = Interop.LZ4_compress_default(
                        a + inputOffset,
                        b + outputOffset,
                        inputSize,
                        outputSize);

                    return result;
                }
            }
        }
    }
}
