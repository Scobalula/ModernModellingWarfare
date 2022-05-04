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
using System.IO;
using System.Runtime.InteropServices;

namespace CascLib.NET.Utility
{
    internal static class IOExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static int ReadVariableSizeInt(this BinaryReader reader, int dataSize)
        {
            if (dataSize > 0xFFFFFF)
                return reader.ReadStruct<Int32BE>();
            if (dataSize > 0xFFFF)
                return reader.ReadStruct<Int24BE>();
            if (dataSize > 0xFF)
                return reader.ReadStruct<Int16BE>();
            else
                return reader.ReadByte();
        }

        public unsafe static T ReadStruct<T>(this BinaryReader reader) where T : unmanaged
        {
            Span<byte> buf = stackalloc byte[sizeof(T)];
            if (reader.Read(buf) < buf.Length)
                throw new IOException(); // TODO: Fill in
            return MemoryMarshal.Cast<byte, T>(buf)[0];
        }

        public unsafe static T ReadStruct<T>(this BinaryReader reader, T item) where T : unmanaged
        {
            Span<byte> buf = MemoryMarshal.Cast<T, byte>(new Span<T>());
            if (reader.Read(buf) < buf.Length)
                throw new IOException(); // TODO: Fill in
            return MemoryMarshal.Cast<byte, T>(buf)[0];
        }

        public unsafe static Span<T> ReadStructArray<T>(this BinaryReader reader, int count) where T : unmanaged
        {
            Span<byte> buf = new byte[count * sizeof(T)];
            if (reader.Read(buf) < buf.Length)
                throw new IOException(); // TODO: Fill in
            return MemoryMarshal.Cast<byte, T>(buf);
        }

        public static byte PeekByte(this BinaryReader reader)
        {
            long temp = reader.BaseStream.Position;
            var result = reader.ReadByte();
            reader.BaseStream.Position = temp;
            return result;
        }
    }
}
