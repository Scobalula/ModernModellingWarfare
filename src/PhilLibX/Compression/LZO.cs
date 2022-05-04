using System;
using System.IO;
using System.IO.IsolatedStorage;
// TODO: Add count/offset methods for Compress

namespace PhilLibX.Compression
{
    public static unsafe class LZO
    {
        /// <summary>
        /// Calculates the maximum possible size for the given data size 
        /// </summary>
        /// <returns>Maximum possible size for the given data size </returns>
        public static int CompressBound(int size) => size + size / 16 + 64 + 3;

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="data">Data to decompress</param>
        /// <param name="decompressedSize">Size of the decompressed data</param>
        /// <returns>Decompressed buffer</returns>
        public static byte[] Decompress(byte[] data, int decompressedSize)
        {
            var rawBuf = new byte[decompressedSize];

            fixed (byte* a = &data[0])
            {
                var rawBufSize = decompressedSize;

                fixed (byte* b = &rawBuf[0])
                {
                    var result = (Interop.LZOkayResult)Interop.LZOkayDecompress(
                        b,
                        ref rawBufSize,
                        a,
                        data.Length);

                    if (result != Interop.LZOkayResult.Success)
                        throw new CompressionException($"Failed to decompress data: {result}");
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
        /// <returns>
        /// The total number of bytes decompressed into the buffer.
        /// This can be less than the number of bytes allocated in the buffer if that many bytes are not currently available,
        /// or zero (0) if the end of the stream has been reached.</returns>
        public static int Decompress(byte[] data, int dataOffset, int dataSize, byte[] decompressedData, int decompressedOffset, int decompressedSize)
        {
            fixed (byte* a = &data[0])
            {
                var rawBufSize = decompressedSize;

                fixed (byte* b = &decompressedData[0])
                {
                    var result = (Interop.LZOkayResult)Interop.LZOkayDecompress(
                        b + decompressedOffset,
                        ref rawBufSize,
                        a + dataOffset,
                        dataSize);

                    if (result != Interop.LZOkayResult.Success)
                        return 0;

                    return rawBufSize;
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
            var temp = new byte[CompressBound(data.Length)];

            fixed (byte* a = &data[0])
            {
                var outputSize = temp.Length;

                fixed (byte* b = &temp[0])
                {
                    var result = (Interop.LZOkayResult)Interop.LZOkayCompress(
                        b,
                        ref outputSize,
                        a,
                        data.Length);


                    if (result != Interop.LZOkayResult.Success)
                        throw new CompressionException($"Failed to compress data: {result}");

                    var output = new byte[outputSize];
                    Buffer.BlockCopy(temp, 0, output, 0, outputSize);

                    return output;
                }
            }
        }
    }
}
