using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PhilLibX.IO
{
    /// <summary>
    /// Provides extension methods for <see cref="BinaryReader"/> objects
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads a native data structure from the current stream and advances the current position of the stream by the size of the structure
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <returns>A structure of the given type from the current stream</returns>
        public unsafe static T ReadStruct<T>(this BinaryReader reader) where T : unmanaged
        {
            Span<byte> buf = stackalloc byte[sizeof(T)];
            if (reader.Read(buf) < buf.Length)
                throw new IOException();
            return MemoryMarshal.Cast<byte, T>(buf)[0];
        }

        /// <summary>
        /// Reads a native data structure from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <returns>A structure of the given type from the current stream</returns>
        public unsafe static T ReadStruct<T>(this BinaryReader reader, long position, bool returnBack = false) where T : unmanaged
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadStruct<T>();
            if (returnBack)
                reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a native data structure from the current stream and advances the current position of the stream by the size of the array
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="count">The number of items to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <param name="position">Position of the data</param>
        /// <returns>A structure array of the given type from the current stream</returns>
        public unsafe static Span<T> ReadStructArray<T>(this BinaryReader reader, int count) where T : unmanaged
        {
            if (count == 0)
                return new Span<T>();
            Span<byte> buf = new byte[count * sizeof(T)];
            if (reader.Read(buf) < buf.Length)
                throw new IOException(); 
            return MemoryMarshal.Cast<byte, T>(buf);
        }

        /// <summary>
        /// Reads a native data structure from the current stream and advances the current position of the stream by the size of the array
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="count">The number of items to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <param name="position">Position of the data</param>
        /// <returns>A structure array of the given type from the current stream</returns>
        public static IEnumerable<T> EnumerateStructArray<T>(this BinaryReader reader, int count) where T : unmanaged
        {
            for (int i = 0; i < count; i++)
            {
                yield return reader.ReadStruct<T>();
            }
        }

        /// <summary>
        /// Reads a native data structure from the current stream and advances the current position of the stream by the size of the array
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="count">The number of items to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <param name="position">Position of the data</param>
        /// <returns>A structure array of the given type from the current stream</returns>
        public unsafe static void ReadStructArray<T>(this BinaryReader reader, ref Span<T> input) where T : unmanaged
        {
            if (input.Length == 0)
                return;

            var asBytes = MemoryMarshal.Cast<T, byte>(input);

            if (reader.Read(asBytes) < asBytes.Length)
                throw new IOException();
        }

        /// <summary>
        /// Reads a native data structure from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="count">The number of items to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <param name="position">Position of the data</param>
        /// <returns>A structure array of the given type from the current stream</returns>
        public unsafe static Span<T> ReadStructArray<T>(this BinaryReader reader, int count, long position, bool returnBack = false) where T : unmanaged
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadStructArray<T>(count);

            if(returnBack)
                reader.BaseStream.Position = temp;

            return result;
        }

        /// <summary>
        /// Returns the next available byte and does not advance the byte or character position.
        /// </summary>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <returns></returns>
        public static byte PeekByte(this BinaryReader reader)
        {
            long temp = reader.BaseStream.Position;
            var result = reader.ReadByte();
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Returns the next available byte at the position and does not advance the byte or character position.
        /// </summary>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <returns></returns>
        public static byte ReadByte(this BinaryReader reader, long position)
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadByte();
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="position">Position of the data</param>
        /// <returns>A 2-byte signed integer read from this stream.</returns>
        public static short ReadInt16(this BinaryReader reader, long position)
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadInt16();
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="position">Position of the data</param>
        /// <returns>A 2-byte signed integer read from this stream.</returns>
        public static ushort ReadUInt16(this BinaryReader reader, long position)
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadUInt16();
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="position">Position of the data</param>
        /// <returns>A 4-byte signed integer read from this stream.</returns>
        public static int ReadInt32(this BinaryReader reader, long position)
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadInt32();
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="position">Position of the data</param>
        /// <returns>A 4-byte signed integer read from this stream.</returns>
        public static uint ReadUInt32(this BinaryReader reader, long position)
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadUInt32();
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a 8-byte unsigned integer from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="position">Position of the data</param>
        /// <returns>A 8-byte signed integer read from this stream.</returns>
        public static long ReadInt64(this BinaryReader reader, long position)
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadInt64();
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a 8-byte unsigned integer from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="position">Position of the data</param>
        /// <returns>A 8-byte signed integer read from this stream.</returns>
        public static ulong ReadUInt64(this BinaryReader reader, long position)
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadUInt64();
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads the specified number of bytes from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <param name="reader">Current <see cref="BinaryReader"/></param>
        /// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <param name="position">Position of the data</param>
        /// <returns>A byte array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
        public static byte[] ReadBytes(this BinaryReader reader, int count, long position)
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = reader.ReadBytes(count);
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a UTF-8 string from the reader terminated by a null byte (also known as C string)
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="bufferSize">Initial size of the buffer</param>
        /// <returns>Resulting string</returns>
        public static string ReadUTF8NullTerminatedString(this BinaryReader reader)
        {
            var output = new StringBuilder(256);

            while (true)
            {
                var c = reader.ReadByte();

                if (c == 0)
                    break;

                output.Append(Convert.ToChar(c));
            }

            return output.ToString();
        }

        /// <summary>
        /// Reads a UTF-8 string from the reader terminated by a null byte (also known as C string)
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="position">Position in the reader where the data is located</param>
        /// <returns>Resulting string</returns>
        public static string ReadUTF8NullTerminatedString(this BinaryReader reader, long position)
        {
            if (!reader.BaseStream.CanSeek)
                throw new NotSupportedException("Stream does not support seeking");

            var temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = ReadUTF8NullTerminatedString(reader);
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a UTF-8 string from the reader terminated by a null byte (also known as C string)
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="bufferSize">Initial size of the buffer</param>
        /// <returns>Resulting string</returns>
        public static string ReadUTF16NullTerminatedString(this BinaryReader reader)
        {
            var output = new StringBuilder(256);

            while (true)
            {
                var c = reader.ReadUInt16();

                if (c == 0)
                    break;

                output.Append(Convert.ToChar(c));
            }

            return output.ToString();
        }

        /// <summary>
        /// Reads a UTF-8 string from the reader terminated by a null byte (also known as C string)
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="position">Position in the reader where the data is located</param>
        /// <returns>Resulting string</returns>
        public static string ReadUTF16NullTerminatedString(this BinaryReader reader, long position)
        {
            if (!reader.BaseStream.CanSeek)
                throw new NotSupportedException("Stream does not support seeking");

            var temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = ReadUTF16NullTerminatedString(reader);
            reader.BaseStream.Position = temp;
            return result;
        }

        /// <summary>
        /// Reads a UTF-8 string from the reader terminated by a null byte (also known as C string)
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="bufferSize">Initial size of the buffer</param>
        /// <returns>Resulting string</returns>
        public static string ReadUTF32NullTerminatedString(this BinaryReader reader)
        {
            var output = new StringBuilder(256);

            while (true)
            {
                var c = reader.ReadUInt32();

                if (c == 0)
                    break;

                output.Append(Convert.ToChar(c));
            }

            return output.ToString();
        }

        /// <summary>
        /// Reads a UTF-8 string from the reader terminated by a null byte (also known as C string)
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="position">Position in the reader where the data is located</param>
        /// <returns>Resulting string</returns>
        public static string ReadUTF32NullTerminatedString(this BinaryReader reader, long position)
        {
            if (!reader.BaseStream.CanSeek)
                throw new NotSupportedException("Stream does not support seeking");

            var temp = reader.BaseStream.Position;
            reader.BaseStream.Position = position;
            var result = ReadUTF32NullTerminatedString(reader);
            reader.BaseStream.Position = temp;
            return result;
        }
    }
}
