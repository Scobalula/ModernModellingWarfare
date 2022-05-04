using System;
using System.Diagnostics;

namespace PhilLibX.Compression
{
    public static unsafe class ZLIB
    {
        /// <summary>
        /// Calculates the maximum possible size for the given data size 
        /// </summary>
        /// <returns>Maximum possible size for the given data size </returns>
        public static int CompressBound(int size) => Interop.MZ_deflateBound(IntPtr.Zero, size);

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <param name="outputSize">Size of the data</param>
        /// <returns>Decompressed buffer</returns>
        public static byte[] Decompress(byte[] inputBuffer, int outputSize) =>
            Decompress(inputBuffer, outputSize, true);

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <param name="outputSize">Size of the data</param>
        /// <param name="hasZlibHeader">Whether or not the data has a zlib header or is raw deflate</param>
        /// <returns>Decompressed buffer</returns>
        public static byte[] Decompress(byte[] inputBuffer, int outputSize, bool hasZlibHeader)
        {
            var rawBuf = new byte[outputSize];

            fixed (byte* a = &inputBuffer[0])
            {
                var rawBufSize = outputSize;

                fixed (byte* b = &rawBuf[0])
                {
                    var result = (Interop.MiniZReturnStatus)Interop.MZ_uncompress(
                        b,
                        ref rawBufSize,
                        a,
                        inputBuffer.Length,
                        hasZlibHeader ? 15 : -15);

                    if (result != Interop.MiniZReturnStatus.OK)
                        throw new CompressionException($"Failed to decompress Data: {result}");
                }
            }

            return rawBuf;
        }

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <param name="inputOffset">Offset within the buffer of the data</param>
        /// <param name="inputSize">Size of the data</param>
        /// <param name="outputBuffer">Buffer to output to</param>
        /// <param name="outputOffset">Offset within the buffer to place the data</param>
        /// <param name="outputSize">Size of the data</param>
        /// <param name="hasZlibHeader">Whether or not the data has a zlib header or is raw deflate</param>
        /// The total number of bytes placed into the buffer.
        /// This can be less than the number of bytes allocated in the buffer if that many bytes are not currently available,
        /// or zero (0) if the end of the data has been reached.</returns>
        public static int Decompress(byte[] data, int dataOffset, int dataSize, byte[] decompressedData, int decompressedOffset, int decompressedSize)
            => Decompress(data, dataOffset, dataSize, decompressedData, decompressedOffset, decompressedSize, true);

        /// <summary>
        /// Decompresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <param name="inputOffset">Offset within the buffer of the data</param>
        /// <param name="inputSize">Size of the data</param>
        /// <param name="outputBuffer">Buffer to output to</param>
        /// <param name="outputOffset">Offset within the buffer to place the data</param>
        /// <param name="outputSize">Size of the data</param>
        /// <param name="hasZlibHeader">Whether or not the data has a zlib header or is raw deflate</param>
        /// <returns>The total number of bytes placed into the buffer.
        /// This can be less than the number of bytes allocated in the buffer if that many bytes are not currently available,
        /// or zero (0) if the end of the data has been reached.</returns>
        public static int Decompress(byte[] inputBuffer, int inputOffset, int inputSize, byte[] outputBuffer, int outputOffset, int outputSize, bool hasZlibHeader)
        {
            if (inputBuffer is null)
                throw new ArgumentNullException(nameof(inputBuffer), "Input Buffer cannot be null");
            if (inputOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(inputOffset), "Input offset must be a non-negative number");
            if ((uint)inputSize > inputBuffer.Length - inputOffset)
                throw new ArgumentOutOfRangeException(nameof(inputSize), "Input size is outside the bounds of the buffer");

            if (outputBuffer is null)
                throw new ArgumentNullException(nameof(outputBuffer), "Output Buffer cannot be null");
            if (outputOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(outputOffset), "Output offset must be a non-negative number");
            if (outputSize < 0 || outputSize > outputBuffer.Length - outputOffset)
                throw new ArgumentOutOfRangeException(nameof(outputSize), "Output size is outside the bounds of the buffer");

            fixed (byte* a = &inputBuffer[0])
            {
                var rawBufSize = outputSize;

                fixed (byte* b = &outputBuffer[0])
                {
                    var result = (Interop.MiniZReturnStatus)Interop.MZ_uncompress(
                        b + outputOffset,
                        ref rawBufSize,
                        a + inputOffset,
                        inputSize,
                        hasZlibHeader ? 15 : -15);

                    if (result != Interop.MiniZReturnStatus.OK)
                        return 0;

                    return rawBufSize;
                }
            }
        }

        /// <summary>
        /// Compresses the provided data
        /// </summary>
        /// <param name="inputBuffer">Input buffer</param>
        /// <returns>Compressed buffer</returns>
        public static byte[] Compress(byte[] inputBuffer)
        {
            var temp = new byte[CompressBound(inputBuffer.Length)];

            fixed (byte* a = &inputBuffer[0])
            {
                var outputSizeResult = temp.Length;

                fixed (byte* b = &temp[0])
                {
                    var result = (Interop.MiniZReturnStatus)Interop.MZ_compress(
                        b,
                        ref outputSizeResult,
                        a,
                        inputBuffer.Length);

                    if (result != Interop.MiniZReturnStatus.OK)
                        throw new CompressionException($"Failed to decompress ZLIB Data");

                    var output = new byte[outputSizeResult];
                    Buffer.BlockCopy(temp, 0, output, 0, outputSizeResult);

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
                    var result = (Interop.MiniZReturnStatus)Interop.MZ_compress(
                        b + inputOffset,
                        ref outputSizeResult,
                        a + inputOffset,
                        inputSize);

                    if (result != Interop.MiniZReturnStatus.OK)
                        return 0;

                    return outputSizeResult;
                }
            }
        }
    }
}
