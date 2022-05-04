using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX
{
    /// <summary>
    /// Interop Methods
    /// </summary>
    public unsafe static class Interop
    {
        #region LZ4
        const string LZ4LibraryName = "LZ4";

        [DllImport(LZ4LibraryName, EntryPoint = "LZ4_decompress_safe", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZ4_decompress_safe(byte* src, byte* dst, int compressedSize, int dstCapacity);

        [DllImport(LZ4LibraryName, EntryPoint = "LZ4_compressBound", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZ4_compressBound(int inputSize);

        [DllImport(LZ4LibraryName, EntryPoint = "LZ4_compress_default", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZ4_compress_default(byte* src, byte* dst, int compressedSize, int dstCapacity);
        #endregion

        #region Oodle
        const string OodleLibraryName = "Oodle";

        [DllImport(OodleLibraryName, EntryPoint = "OodleLZ_Decompress", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OodleLZ_Decompress(
            byte* compBuf, int compSize, byte* rawBuf, int rawSize,
            int fuzzSafe, /*OodleFuzzSafe*/
            int checkCrc, /*OodleCheckCRC*/
            int verbosity, /*OodleVerbosity*/
            byte* dictBuff, int dictSize,
            long unk, long unkCallback,
            byte* scratchBuff, int scratchSize,
            int threadPhase); /*OodleThreading*/
        #endregion

        #region LZO
        internal enum LZOkayResult
        {
            LookbehindOverrun = -4,
            OutputOverrun     = -3,
            InputOverrun      = -2,
            Error             = -1,
            Success           = 0,
            InputNotConsumed  = 1,
        }

        const string LZOLibraryName = "LZOkay";

        [DllImport(LZOLibraryName, EntryPoint = "LZOkayDecompress", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZOkayDecompress(byte* dest, ref int destLen, byte* source, int sourceLen);

        [DllImport(LZOLibraryName, EntryPoint = "LZOkayCompress", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LZOkayCompress(byte* dest, ref int destLen, byte* source, int sourceLen);
        #endregion

        #region ZLIB
        internal enum MiniZReturnStatus
        {
            OK               = 0,
            StreamEnd        = 1,
            NeedDict         = 2,
            ErrorNo          = -1,
            StreamError      = -2,
            DataError        = -3,
            MemoryError      = -4,
            BufferError      = -5,
            VersionError     = -6,
            ParamError       = -10000
        };

        const string MiniZLibraryName = "MiniZ";

        [DllImport(MiniZLibraryName, EntryPoint = "mz_uncompress", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MZ_uncompress(byte* dest, ref int destLen, byte* source, int sourceLen, int windowBits);

        [DllImport(MiniZLibraryName, EntryPoint = "mz_deflateBound", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MZ_deflateBound(IntPtr stream, int inputSize);

        [DllImport(MiniZLibraryName, EntryPoint = "mz_compress", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MZ_compress(byte* dest, ref int destLen, byte* source, int sourceLen);
        #endregion

        #region ZStandard
        const string ZStdLibraryName = "ZStandard";

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_decompress", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ZSTD_decompress(void* dst, int dstCapacity, void* src, int srcCapacity);

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_compress", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ZSTD_compress(void* dst, int dstCapacity, void* src, int srcCapacity, int compressionLevel);

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_getFrameContentSize")]
        internal static extern int ZSTD_getFrameContentSize(void* src, int srcSize);

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_getErrorName", CallingConvention = CallingConvention.Cdecl)]
        internal static extern sbyte* ZSTD_getErrorName(int errorCode);

        [DllImport(ZStdLibraryName, EntryPoint = "ZSTD_compressBound")]
        internal static extern int ZSTD_compressBound(int srcSize);
        #endregion

        #region Windows Kernel32
        public static class Kernel32
        {
            public struct NtModuleInfo
            {
                public IntPtr       BaseOfDll;
                public int          SizeOfImage;
                public IntPtr       EntryPoint;
            }

            [DllImport("kernel32", SetLastError = true)]
            public static extern bool ReadProcessMemory
            (
                IntPtr hProcess,
                long lpBaseAddress,
                byte[] lpBuffer,
                int nSize,
                out int lpNumberOfBytesRead
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadProcessMemory
            (
                IntPtr hProcess,
                long lpBaseAddress,
                byte* lpBuffer,
                int nSize,
                out int lpNumberOfBytesRead
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteProcessMemory
            (
                IntPtr hProcess,
                long lpBaseAddress,
                byte[] lpBuffer,
                int nSize,
                out int lpNumberOfBytesRead
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteProcessMemory
            (
                IntPtr hProcess,
                long lpBaseAddress,
                byte* lpBuffer,
                int nSize,
                out int lpNumberOfBytesRead
            );

            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess
            (
                int dwDesiredAccess,
                bool bInheritHandle,
                int dwProcessId
            );

            [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            public static extern bool IsWow64Process(IntPtr processHandle, out bool wow64Process);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "K32GetModuleInformation")]
            public static extern bool GetModuleInformation(IntPtr processHandle, IntPtr moduleHandle, out NtModuleInfo ntModuleInfo, int size);

            [Flags]
            public enum AllocationType
            {
                Commit = 0x1000,
                Reserve = 0x2000,
                Decommit = 0x4000,
                Release = 0x8000,
                Reset = 0x80000,
                Physical = 0x400000,
                TopDown = 0x100000,
                WriteWatch = 0x200000,
                LargePages = 0x20000000
            }

            [Flags]
            public enum MemoryProtection
            {
                Execute = 0x10,
                ExecuteRead = 0x20,
                ExecuteReadWrite = 0x40,
                ExecuteWriteCopy = 0x80,
                NoAccess = 0x01,
                ReadOnly = 0x02,
                ReadWrite = 0x04,
                WriteCopy = 0x08,
                GuardModifierflag = 0x100,
                NoCacheModifierflag = 0x200,
                WriteCombineModifierflag = 0x400
            }

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern IntPtr VirtualAllocEx(IntPtr hProcess,
                                IntPtr lpAddress,
                                ulong dwSize,
                                AllocationType flAllocationType,
                                MemoryProtection flProtect);
        }
        #endregion

        #region Windows PSAPI
        internal static class PSAPI
        {
            public enum ListModules : uint
            {
                Default,
                X86,
                X64,
                All,
            }

            [DllImport("psapi.dll", SetLastError = true)]
            public static extern bool EnumProcessModulesEx(IntPtr hProcess, IntPtr[] lphModule, int cb, out int lpcbNeeded, ListModules listFilter);

            [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize);
        }
        #endregion

        #region Resolver
        static Interop()
        {
            NativeLibrary.SetDllImportResolver(typeof(Interop).Assembly, DllImportResolver);
        }

        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            var path = $"Native\\{RuntimeInformation.ProcessArchitecture}{libraryName}";

            try
            {
                return NativeLibrary.Load(path, assembly, searchPath);
            }
            // If we didn't find the exception, we don't want a fatal 
            // error here, we want to fall back to default resolver
            catch (DllNotFoundException)
            {
                return IntPtr.Zero;
            }
            // Everything else, we might have something more serious
            catch
            {
                throw;
            }
        }
        #endregion
    }
}
