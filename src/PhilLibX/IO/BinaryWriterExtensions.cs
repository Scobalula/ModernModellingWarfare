using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PhilLibX.IO
{
    /// <summary>
    /// Provides extension methods for <see cref="BinaryWriter"/> objects
    /// </summary>
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Writes a null terminated string
        /// </summary>
        /// <param name="bw">Output Stream</param>
        /// <param name="value">Value to write</param>
        public static void WriteNullTerminatedString(this BinaryWriter bw, string value)
        {
            bw.Write(Encoding.UTF8.GetBytes(value));
            bw.Write((byte)0);
        }

        /// <summary>
        /// Reads a native data structure from the current stream and advances the current position of the stream by the size of the structure
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <returns>A structure of the given type from the current stream</returns>
        public static void WriteStruct<T>(this BinaryWriter writer, T obj) where T : unmanaged
        {
            writer.Write(MemoryMarshal.Cast<T, byte>(stackalloc T[1]
            {
                obj
            }));
        }

        /// <summary>
        /// Reads a native data structure from the current stream and advances the current position of the stream by the size of the array
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="count">The number of items to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <param name="position">Position of the data</param>
        /// <returns>A structure array of the given type from the current stream</returns>
        public static void WriteStructArray<T>(this BinaryWriter writer, T[] obj) where T : unmanaged
        {
            writer.Write(MemoryMarshal.Cast<T, byte>(obj));
        }
    }
}
