using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ModernModellingWarfare.Assets;
using ModernModellingWarfare.Shared;

namespace ModernModellingWarfare
{
    /// <summary>
    /// A class to hold a Modern Warfare zone.
    /// </summary>
    internal unsafe class Zone : IDisposable
    {
        /// <summary>
        /// Gets the data buffers, this contains the final processed blocks.
        /// </summary>
        public unsafe byte*[] DataBuffers { get; set; }

        /// <summary>
        /// Gets or Sets the data buffer pointers, for use during load.
        /// </summary>
        public unsafe byte*[] DataBufferPointers { get; set; }

        /// <summary>
        /// Gets or Sets the start positions, testing game_dx12_ship_replay shows this used for keeping pointers aligned with patch files.
        /// </summary>
        public Stack<int>[] StartPositions { get; set; }

        /// <summary>
        /// Gets or Sets the current buffer index.
        /// </summary>
        public int DataBufferIndex { get; set; }

        /// <summary>
        /// Gets or Sets the asset list.
        /// </summary>
        public XAssetList* AssetList;

        /// <summary>
        /// Gets or Sets the underlying fast file stream.
        /// </summary>
        public Stream FastFileStream { get; set; }

        /// <summary>
        /// Gets or Sets any warnings ran into during load.
        /// </summary>
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// Gets or Sets the current zone memory offset.
        /// </summary>
        public long ZoneMemoryPosition
        {
            get
            {
                return DataBufferPointers[DataBufferIndex] - DataBuffers[DataBufferIndex];
            }
        }

        /// <summary>
        /// Checks if the provided pointer is a null value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if null, otherwise false.</returns>
        public static bool IsNullPointer(void* value) => IsNullPointer((long)value);

        /// <summary>
        /// Checks if the provided pointer is a null value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if null, otherwise false.</returns>
        public static bool IsNullPointer(long value)
        {
            return value == 0;
        }

        /// <summary>
        /// Loads data from the underlying stream.
        /// </summary>
        /// <param name="pointer">Pointer to where to read the data into.</param>
        /// <param name="size">The size of the data.</param>
        /// <param name="atStreamStart">Whether or not we are reading from the stream.</param>
        /// <exception cref="IOException">Unable to read the data.</exception>
        public unsafe void LoadStream(void* pointer, int size, bool atStreamStart)
        {
            if (atStreamStart)
            {
                if (FastFileStream.Read(new Span<byte>(pointer, size)) != size)
                    throw new IOException("An attempt was made to read from the end of the Fast File");

                DataBufferPointers[DataBufferIndex] += size;
            }
        }

        /// <summary>
        /// Allocates the provided memory in the current buffer aligned to the given alignment.
        /// </summary>
        /// <param name="alignment">Alignment to align to.</param>
        /// <returns>Resuling aligned pointer.</returns>
        public unsafe byte* AllocStreamPosition(ulong alignment)
        {
            DataBufferPointers[DataBufferIndex] = (byte*)(~(long)alignment & (long)DataBufferPointers[DataBufferIndex] + (long)alignment);
            return DataBufferPointers[DataBufferIndex];
        }

        /// <summary>
        /// Initializes the memory buffers.
        /// </summary>
        /// <param name="size">Size of the memory buffers.</param>
        public unsafe void InitializeBuffers(int size)
        {
            DataBuffers = new byte*[11];
            DataBufferPointers = new byte*[11];
            StartPositions = new Stack<int>[11];

            for (int index = 0; index < DataBuffers.Length; ++index)
            {
                DataBuffers[index] = (byte*)(void*)Marshal.AllocHGlobal(size);
                DataBufferPointers[index] = DataBuffers[index];
                StartPositions[index] = new Stack<int>();
            }
        }

        /// <summary>
        /// Pushes the start of the asset for maintaining alignments during patching.
        /// </summary>
        public unsafe void PushStartPosition() => StartPositions[DataBufferIndex].Push((int)(DataBufferPointers[DataBufferIndex] - DataBuffers[DataBufferIndex]));

        /// <summary>
        /// Pops the start of the asset for maintaining alignments during patching.
        /// </summary>
        public void PopStartPosition() => StartPositions[DataBufferIndex].Pop();

        /// <summary>
        /// Calculates the position of the relative pointer.
        /// </summary>
        /// <param name="pointer">The relative pointer.</param>
        /// <returns>Resulting data location.</returns>
        public unsafe byte* CalculateStreamPosition(void* pointer) => CalculateStreamPosition((long)pointer);

        /// <summary>
        /// Calculates the position of the relative pointer.
        /// </summary>
        /// <param name="pointer">The relative pointer.</param>
        /// <returns>Resulting data location.</returns>
        public unsafe byte* CalculateStreamPosition(long pointer)
        {
            if (pointer == 0L)
                return null;
            long num1 = pointer;
            long index = num1 >> 32 & 15L;
            int num2 = (int)(num1 - 1L);
            if ((num1 & 68719476736L) != 0L)
                num2 += StartPositions[index].Peek();
            long num3 = (long)(DataBuffers[index] + num2);
            return DataBuffers[index] + num2;
        }

        /// <summary>
        /// Inserts 8 bytes padding into the current stream.
        /// </summary>
        public unsafe void InsertPadding()
        {
            // Padding used when we hit an asset not in this .ff?
            // Regardless, debugging game_dx12_ship_replay shows 
            // this inserted when something with -3 ptr is hit
            AllocStreamPosition(7UL);
            DataBufferPointers[DataBufferIndex] += 8;
        }

        /// <summary>
        /// Loads a null terminated string.
        /// </summary>
        /// <param name="alignment">Alignment of the string.</param>
        /// <param name="noComma">Whether to skip the starting comma.</param>
        /// <returns>Resulting string.</returns>
        /// <exception cref="IOException">Unable to read the data.</exception>
        public unsafe sbyte* LoadXString(ulong alignment, bool noComma)
        {
            sbyte* dataBufferPointer = (sbyte*)DataBufferPointers[DataBufferIndex];
            do
            {
                int num = FastFileStream.ReadByte();
                *DataBufferPointers[DataBufferIndex] = num >= 0 ? (byte)num : throw new IOException("An attempt was made to read from the end of the Fast File");
            }
            while (*DataBufferPointers[DataBufferIndex]++ != 0);
            return dataBufferPointer;
        }

        public void Load(Stream stream)
        {
            FastFileStream = stream;

            // TODO: Use header sizes now that I know them
            // but since transient files are small, this shouldn't
            // be a problem
            InitializeBuffers((int)stream.Length * 2);

            DataBufferIndex = 1;

            // Begin by reading the XAsset List (same header as other CoDs)
            AssetList = (XAssetList*)AllocStreamPosition(8UL);
            LoadStream(AssetList, 32, true);

            // Next is string list
            DataBufferIndex = 5;
            AssetList->StringList.Strings = (sbyte**)AllocStreamPosition(7UL);
            LoadStream(AssetList->StringList.Strings, 8 * AssetList->StringList.Count, true);
            for (int index = 0; index < AssetList->StringList.Count; ++index)
                if ((long)AssetList->StringList.Strings[index] == -2)
                    AssetList->StringList.Strings[index] = LoadXString(0UL, true);

            // Juicy assets
            AssetList->Assets = (XAsset*)AllocStreamPosition(7UL);
            LoadStream(AssetList->Assets, 16 * (int)AssetList->AssetCount, true);

            // We've only implemented assets stored within the streamed character/weapon
            // fast files, and there is still some data we're not parsing as I haven't 
            // ran into it, parsing every asset is incredibly complex, there are abour 120
            // asset types in MW, and hundreds of nested structures within them
            for (int i = 0; i < AssetList->AssetCount; i++)
            {
                switch (AssetList->Assets[i].Type)
                {
                    case 8:
                        {
                            Global.VerbosePrint($"Reading XModel Surface at: 0x{FastFileStream.Position:X}");
                            var temp = XModelSurfsHandler.VarAssetPtr;
                            XModelSurfsHandler.VarAssetPtr = (XModelSurfsHandler.XModelSurfaces**)&AssetList->Assets[i].Header;
                            XModelSurfsHandler.LoadAssetPtr(this, false);
                            XModelSurfsHandler.VarAssetPtr = temp;
                            break;
                        }
                    case 9:
                        {
                            Global.VerbosePrint($"Reading XModel at: 0x{FastFileStream.Position:X}");
                            var temp = XModelHandler.VarAssetPtr;
                            XModelHandler.VarAssetPtr = (XModelHandler.XModel**)&AssetList->Assets[i].Header;
                            XModelHandler.LoadAssetPtr(this, false);
                            XModelHandler.VarAssetPtr = temp;
                            break;
                        }
                    case 101:
                        {
                            Global.VerbosePrint($"Reading XModel Detail Collision at: 0x{FastFileStream.Position:X}");
                            var temp = XModelDetailCollisionHandler.VarAssetPtr;
                            XModelDetailCollisionHandler.VarAssetPtr = (XModelDetailCollisionHandler.XModelDetailCollision**)&AssetList->Assets[i].Header;
                            XModelDetailCollisionHandler.LoadAssetPtr(this, false);
                            XModelDetailCollisionHandler.VarAssetPtr = temp;
                            break;
                        }
                    case 7:
                        {
                            Global.VerbosePrint($"Reading XAnim at: 0x{FastFileStream.Position:X}");
                            var temp = XAnimHandler.VarAssetPtr;
                            XAnimHandler.VarAssetPtr = (XAnimHandler.XAnim**)&AssetList->Assets[i].Header;
                            XAnimHandler.LoadAssetPtr(this, false);
                            XAnimHandler.VarAssetPtr = temp;
                            break;
                        }
                    default:
                        Warnings.Add(string.Format("Hit an unknown XAsset: {0}", AssetList->Assets[i].Type));

                        // Null out remaining assets since we don't want to actually kill the export
                        // since XModels are probably processed at this stage
                        // For example we could hit streaminginfo at the very end
                        for (int j = i; j < AssetList->AssetCount; j++)
                        {
                            AssetList->Assets[i].Header = null;
                            AssetList->Assets[i].Type = uint.MaxValue;
                        }
                        return;
                }
            }
        }

        /// <summary>
        /// Dumps the raw zone memory to individual files.
        /// </summary>
        public void DumpZoneMemory()
        {
            for (int index = 0; index < DataBuffers.Length; ++index)
            {
                File.WriteAllBytes($"buffer_{index}.dat", new Span<byte>(DataBuffers[index], (int)FastFileStream.Length).ToArray());
            }
        }

        /// <summary>
        /// Disposes of the zone.
        /// </summary>
        public void Dispose()
        {
            for (int index = 0; index < 8; ++index)
                Marshal.FreeHGlobal((IntPtr)DataBuffers[index]);
        }
    }
}
