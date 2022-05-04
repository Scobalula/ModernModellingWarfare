// ------------------------------------------------------------------------
// PhilLibX - My Utility Library
// Copyright(c) 2021 Philip/Scobalula
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ------------------------------------------------------------------------
// File: ProcessStream.cs
// Author: Philip/Scobalula
// Description: Provides a Stream for a foreign Process, supporting both synchronous and asynchronous read and write operations.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PhilLibX.IO
{
    /// <summary>
    /// Provides a <see cref="Stream"/> for a foreign Process, supporting both synchronous and asynchronous read and write operations.
    /// </summary>
    public class ProcessStream : Stream
    {
        /// <summary>
        /// Process Stream Access Rights
        /// </summary>
        [Flags]
        public enum AccessRightsFlags
        {
            /// <summary>
            /// Open the Process with Read Access
            /// </summary>
            Read = 0x0010,

            /// <summary>
            /// Open the Process with Write Access
            /// </summary>
            Write = 0x0020,
        }

        /// <summary>
        /// Internal Process Value
        /// </summary>
        protected Process InternalProcess;

        /// <summary>
        /// Internal Process Handle
        /// </summary>
        protected IntPtr InternalHandle;

        /// <summary>
        /// Gets or Sets the Position
        /// </summary>
        protected long InternalPosition;

        /// <summary>
        /// Gets or Set the Access Rights
        /// </summary>
        public AccessRightsFlags AccessRights { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => InternalHandle != IntPtr.Zero && AccessRights.HasFlag(AccessRightsFlags.Read);

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => InternalHandle != IntPtr.Zero;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => InternalHandle != IntPtr.Zero && AccessRights.HasFlag(AccessRightsFlags.Write);

        /// <summary>
        /// Gets or Sets whether or not we have Strict Access (throw exceptions on failed Read/Write)
        /// </summary>
        public bool StrictAccess { get; set; }

        /// <summary>
        /// Gets or Sets whether or not we allow null pointers
        /// </summary>
        public bool AllowNullPointers { get; set; }

        /// <summary>
        /// Gets or Sets the Cache
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
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override long Length => MainModuleImageSize;

        /// <summary>
        /// Gets or Sets the position within the current stream
        /// </summary>
        public override long Position
        {
            get
            {
                return InternalPosition;
            }
            set
            {
                InternalPosition = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Base Address of the Process' Main Module
        /// </summary>
        public long MainModuleBaseAddress
        {
            get
            {
                if(OperatingSystem.IsWindows())
                {
                    var ntSizeof = Marshal.SizeOf<Interop.Kernel32.NtModuleInfo>();
                    var modules = new IntPtr[1];
                    if (Interop.PSAPI.EnumProcessModulesEx(InternalHandle, modules, IntPtr.Size * modules.Length, out _, Interop.PSAPI.ListModules.All) == true)
                    {
                        Interop.Kernel32.GetModuleInformation(InternalHandle, modules[0], out var info, ntSizeof);

                        return info.BaseOfDll.ToInt64();
                    }
                }
                else
                {
                    throw new NotSupportedException($"ProcessStream is not supported on this platform");
                }

                return -1;
            }
        }

        /// <summary>
        /// Gets or Sets the size of the code section of the Process' Main Module
        /// </summary>
        public int MainModuleImageSize
        {
            get
            {
                var ntSizeof = Marshal.SizeOf<Interop.Kernel32.NtModuleInfo>();
                var modules = new IntPtr[1];
                if (Interop.PSAPI.EnumProcessModulesEx(InternalHandle, modules, IntPtr.Size * modules.Length, out _, Interop.PSAPI.ListModules.All) == true)
                {
                    Interop.Kernel32.GetModuleInformation(InternalHandle, modules[0], out var info, ntSizeof);

                    return info.SizeOfImage;
                }

                return -1;
            }
        }

        /// <summary>
        /// Gets the process handle provided by the operating system
        /// </summary>
        public IntPtr Handle => InternalHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessStream"/> class
        /// </summary>
        /// <param name="process"></param>
        public ProcessStream(Process process, AccessRightsFlags accessRights, int cacheSize = 256)
        {
            // TODO: At the very least, implement a Linux version possibly but since this is only really 
            // for in-game extraction and some data dumping, it's not a priority.
            if (!OperatingSystem.IsWindows())
                throw new NotSupportedException($"ProcessStream is not supported on this platform");

            AccessRights = accessRights;
            InternalHandle = Interop.Kernel32.OpenProcess((int)AccessRights | 0x1000 | 0xFFFF | 0x1F0FFF, false, process.Id);
            StrictAccess = true;
            AllowNullPointers = true;
            Cache = new byte[cacheSize];
        }

        /// <summary>
        /// Clears all buffers for this stream
        /// </summary>
        public override void Flush() { }

        /// <summary>
        /// Reads the bytes from the current stream and writes them to another stream.
        /// </summary>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">The size of the buffer. This value must be greater than zero. The default size is 81920.</param>
        public override void CopyTo(Stream destination, int bufferSize) => throw new NotSupportedException("Cannot Copy a Process Stream");

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the operation failed.</returns>
        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            // Validate arguments, important since we are using unsafe!
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Non-negative number required");
            if (offset > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset is outside the bounds of the array");
            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count), "Count is outside the bounds of the array");

            // If we allow null points, return an empty buffer
            if (AllowNullPointers && InternalPosition == 0)
            {
                Array.Clear(buffer, offset, count);
                return count;
            }


            fixed (byte* b = &buffer[offset])
            {
                // We cannot satisfy the hunger via cache, just read from memory
                if (Cache == null || count >= Cache.Length)
                {
                    if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, InternalPosition, b, count, out int bytesRead) && StrictAccess)
                        throw new Win32Exception();
                    InternalPosition += count;
                    return count;
                }

                // We're going to read from the cache
                fixed (byte* c = &Cache[0])
                {
                    var toRead = count;

                    while (true)
                    {
                        var readPos = Position;
                        var cacheAvailable = CacheEndPosition - readPos;

                        if (cacheAvailable > 0)
                        {
                            // We can take from the cache
                            if (CacheStartPosition <= readPos && CacheEndPosition > readPos)
                            {
                                var p = (int)(readPos - CacheStartPosition);
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

                                toRead -= n;
                                Position += n;
                                offset += n;
                            }
                        }

                        // We've satisfied what we need
                        if (toRead == 0)
                            break;

                        CacheStartPosition = InternalPosition;
                        CacheEndPosition = CacheStartPosition + Cache.Length;

                        if (OperatingSystem.IsWindows())
                        {
                            if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, InternalPosition, c, Cache.Length, out int bytesRead) && StrictAccess)
                                throw new Win32Exception();
                        }
                        else
                        {
                            throw new NotSupportedException($"ProcessStream is not supported on this platform");
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Writes a block of bytes to the process stream.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write to the stream.</param>
        /// <param name="offset"The zero-based byte offset in array from which to begin copying bytes to the stream.></param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset/count", "Offset and Count are outside the bounds of the array");
            }

            unsafe
            {
                fixed (byte* b = buffer)
                {
                    var z = Interop.Kernel32.WriteProcessMemory(InternalHandle, InternalPosition, b + offset, count, out int bytesRead);
                    InternalPosition += bytesRead;
                }
            }
        }

        /// <summary>
        /// Sets the current position of this stream to the given value.
        /// </summary>
        /// <param name="offset">The point relative to origin from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for offset, using a value of type SeekOrigin.</param>
        /// <returns>The new position in the stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    InternalPosition = offset;
                    return InternalPosition;
                case SeekOrigin.Current:
                    InternalPosition += offset;
                    return InternalPosition;
                case SeekOrigin.End:
                    // Processes technically have no end, so we will use the end of the
                    // main module
                    InternalPosition = (MainModuleBaseAddress + MainModuleImageSize) + offset;
                    return InternalPosition;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Sets the length of this stream to the given value.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value) => throw new NotSupportedException("Cannot set length of a Process Stream");

        /// <summary>
        /// Reads bytes from a Processes Memory and returns a byte array of read data.
        /// </summary>
        /// <param name="processHandle">A handle to the process with memory that is being read. The handle must have PROCESS_VM_READ access to the process.</param>
        /// <param name="address">The address of the data to be read.</param>
        /// <param name="numBytes">The number of bytes to be read from the specified process.</param>
        /// <returns>Bytes read</returns>
        public byte[] ReadBytes(long address, int numBytes)
        {
            byte[] buffer = new byte[numBytes];

            if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, numBytes, out _) && StrictAccess)
                throw new Win32Exception();

            return buffer;
        }

        /// <summary>
        /// Reads 64Bit Integer from the Processes Memory
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <returns>Resulting Data</returns>
        public unsafe long ReadInt64(long address)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return 0;

                var buffer = stackalloc byte[sizeof(long)];

                if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, sizeof(long), out int bytesRead) && StrictAccess)
                    throw new Win32Exception();

                return *(long*)buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads an unsigned 64Bit Integer from the Processes Memory
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <returns>Resulting Data</returns>
        public unsafe ulong ReadUInt64(long address)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return 0;

                var buffer = stackalloc byte[sizeof(ulong)];

                if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, sizeof(ulong), out int bytesRead) && StrictAccess)
                    throw new Win32Exception();

                return *(ulong*)buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads 32Bit Integer from the Processes Memory
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <returns>Resulting Data</returns>
        public unsafe int ReadInt32(long address)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return 0;

                var buffer = stackalloc byte[sizeof(int)];

                if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, sizeof(int), out int bytesRead) && StrictAccess)
                    throw new Win32Exception();

                return *(int*)buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads 32Bit Integer from the Processes Memory
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <returns>Resulting Data</returns>
        public unsafe uint ReadUInt32(long address)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return 0;

                var buffer = stackalloc byte[sizeof(uint)];

                if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, sizeof(uint), out int bytesRead) && StrictAccess)
                    throw new Win32Exception();

                return *(uint*)buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads a 16Bit Integer from the Processes Memory
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <returns>Resulting Data</returns>
        public unsafe short ReadInt16(long address)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return 0;

                var buffer = stackalloc byte[sizeof(short)];

                if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, sizeof(short), out int bytesRead) && StrictAccess)
                    throw new Win32Exception();

                return *(short*)buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads an unsigned 16Bit Integer from the Processes Memory
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <returns>Resulting Data</returns>
        public unsafe ushort ReadUInt16(long address)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return 0;

                var buffer = stackalloc byte[sizeof(ushort)];

                if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, sizeof(ushort), out int bytesRead) && StrictAccess)
                    throw new Win32Exception();

                return *(ushort*)buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads a 4 byte single precision floating point number from the Processes Memory
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <returns>Resulting Data</returns>
        public unsafe float ReadSingle(long address)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return 0;

                var buffer = stackalloc byte[sizeof(float)];

                if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, sizeof(float), out int bytesRead) && StrictAccess)
                    throw new Win32Exception();

                return *(float*)buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads an 8 byte double precision floating point number from the Processes Memory
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <returns>Resulting Data</returns>
        public unsafe double ReadDouble(long address)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return 0;

                var buffer = stackalloc byte[sizeof(double)];

                if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, sizeof(double), out int bytesRead) && StrictAccess)
                    throw new Win32Exception();

                return *(double*)buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads a UTF-8 string from the reader terminated by a null byte (also known as C string)
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <param name="bufferSize">Initial size of the buffer</param>
        /// <returns>Resulting data</returns>
        public unsafe string ReadUTF8NullTerminatedString(long address, int bufferSize = 256)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return string.Empty;

                var result = stackalloc byte[bufferSize];
                var output = new StringBuilder(256);

                while (true)
                {
                    Interop.Kernel32.ReadProcessMemory(InternalHandle, address, result, bufferSize, out int bytesRead);
                    address += bufferSize;

                    for (int i = 0; i < bufferSize; i++)
                    {
                        if (result[i] == 0x0)
                            return output.ToString();

                        output.Append(Convert.ToChar(result[i]));
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads a UTF-16 string from the reader terminated by a null byte (also known as C string)
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <param name="bufferSize">Initial size of the buffer</param>
        /// <returns>Resulting data</returns>
        public unsafe string ReadUTF16NullTerminatedString(long address, int bufferSize = 256)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return string.Empty;

                var result = stackalloc ushort[bufferSize * sizeof(ushort)];
                var output = new StringBuilder(256);

                while (true)
                {
                    Interop.Kernel32.ReadProcessMemory(InternalHandle, address, (byte*)result, bufferSize * sizeof(ushort), out int bytesRead);
                    address += bufferSize * sizeof(ushort);

                    for (int i = 0; i < bufferSize; i++)
                    {
                        if (result[i] == 0x0)
                            return output.ToString();

                        output.Append(Convert.ToChar(result[i]));
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads a UTF-16 string from the reader terminated by a null byte (also known as C string)
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <param name="bufferSize">Initial size of the buffer</param>
        /// <returns>Resulting data</returns>
        public unsafe string ReadUTF32NullTerminatedString(long address, int bufferSize = 256)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return string.Empty;

                var result = stackalloc uint[bufferSize * sizeof(uint)];
                var output = new StringBuilder(256);

                while (true)
                {
                    Interop.Kernel32.ReadProcessMemory(InternalHandle, address, (byte*)result, bufferSize * sizeof(uint), out int bytesRead);
                    address += bufferSize * sizeof(uint);

                    for (int i = 0; i < bufferSize; i++)
                    {
                        if (result[i] == 0x0)
                            return output.ToString();

                        output.Append(Convert.ToChar(result[i]));
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads a buffer from the reader terminated by a null byte
        /// </summary>
        /// <param name="address">The address of the data to be read.</param>
        /// <param name="bufferSize">Initial size of the buffer</param>
        /// <returns>Resulting data</returns>
        public unsafe byte[] ReadNullTerminatedByteArray(long address, int bufferSize = 256)
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return Array.Empty<byte>();

                var result = stackalloc byte[bufferSize];
                var list = new List<byte>(255);

                while (true)
                {
                    Interop.Kernel32.ReadProcessMemory(InternalHandle, address, result, bufferSize, out int bytesRead);
                    address += bufferSize;

                    for (int i = 0; i < bufferSize; i++)
                    {
                        if (result[i] == 0x0)
                            return list.ToArray();

                        list.Add(result[i]);
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads a native data structure from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="address">The address of the data to be read.</param>
        /// <returns>A structure of the given type from the current stream</returns>
        public unsafe T ReadStruct<T>(long address) where T : unmanaged
        {
            if (OperatingSystem.IsWindows())
            {
                if (address == 0)
                    return new T();

                var buffer = stackalloc byte[sizeof(T)];

                if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, buffer, sizeof(T), out int bytesRead) && StrictAccess)
                    throw new Win32Exception();

                return *(T*)buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }

        /// <summary>
        /// Reads a native data structure from the current stream at the given position and returns the stream back to its original position.
        /// </summary>
        /// <typeparam name="T">The structure type to read</typeparam>
        /// <param name="address">The address of the data to be read.</param>
        /// <param name="count">The number of items to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <returns>A structure array of the given type from the current stream</returns>
        public unsafe T[] ReadStructArray<T>(long address, int count) where T : unmanaged
        {
            if (OperatingSystem.IsWindows())
            {
                if (count == 0)
                    return Array.Empty<T>();

                var buffer = new T[count];

                if (address == 0)
                    return buffer;

                fixed(T* p = &buffer[0])
                    if (!Interop.Kernel32.ReadProcessMemory(InternalHandle, address, (byte*)p, count * sizeof(T), out int bytesRead) && StrictAccess)
                        throw new Win32Exception();

                return buffer;
            }
            else
            {
                throw new NotSupportedException($"ProcessStream is not supported on this platform");
            }
        }
    }
}