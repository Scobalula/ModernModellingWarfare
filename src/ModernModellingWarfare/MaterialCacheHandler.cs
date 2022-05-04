using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CascLib.NET;
using ModernModellingWarfare.Assets;
using PhilLibX;
using PhilLibX.Imaging.DirectXTex;
using PhilLibX.IO;
using PhilLibX.Media3D;

namespace ModernModellingWarfare
{
    internal static class MaterialCacheHandler
    {
        internal static readonly ScratchImageFormat[] Formats =
        {
            ScratchImageFormat.ForceUInt,
            ScratchImageFormat.R8UNorm,
            ScratchImageFormat.A8UNorm,
            ScratchImageFormat.A8UNorm,
            ScratchImageFormat.R8G8UNorm,
            ScratchImageFormat.R8G8UNorm,
            ScratchImageFormat.R8G8B8A8UNorm,
            ScratchImageFormat.R8G8B8A8UNorm,
            ScratchImageFormat.ForceUInt,
            ScratchImageFormat.ForceUInt,
            ScratchImageFormat.R8SNorm,
            ScratchImageFormat.R8G8SNorm,
            ScratchImageFormat.R16UNorm,
            ScratchImageFormat.R16G16UNorm,
            ScratchImageFormat.R16G16B16A16UNorm,
            ScratchImageFormat.R16SNorm,
            ScratchImageFormat.R16Float,
            ScratchImageFormat.R16G16Float,
            ScratchImageFormat.R16G16B16A16Float,
            ScratchImageFormat.R32Float,
            ScratchImageFormat.R32G32Float,
            ScratchImageFormat.R32G32B32A32Float,
            ScratchImageFormat.D32Float,
            ScratchImageFormat.D32FloatS8X24UInt,
            ScratchImageFormat.R8UInt,
            ScratchImageFormat.R16UInt,
            ScratchImageFormat.R32UInt,
            ScratchImageFormat.R32G32UInt,
            ScratchImageFormat.R32G32B32A32UInt,
            ScratchImageFormat.R10G10B10A2UInt,
            ScratchImageFormat.B5G6R5UNorm,
            ScratchImageFormat.B5G6R5UNorm,
            ScratchImageFormat.R10G10B10A2UNorm,
            ScratchImageFormat.R9G9B9E5SharedExp,
            ScratchImageFormat.R11G11B10Float,
            ScratchImageFormat.BC1UNorm,
            ScratchImageFormat.BC1UNorm,
            ScratchImageFormat.BC2UNorm,
            ScratchImageFormat.BC2UNorm,
            ScratchImageFormat.BC3UNorm,
            ScratchImageFormat.BC3UNorm,
            ScratchImageFormat.BC4UNorm,
            ScratchImageFormat.BC5UNorm,
            ScratchImageFormat.BC5SNorm,
            ScratchImageFormat.BC6HUF16,
            ScratchImageFormat.BC6HSF16,
            ScratchImageFormat.BC7UNorm,
            ScratchImageFormat.BC7UNorm,
            ScratchImageFormat.R8G8B8A8SNorm
        };

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct GfxMip
        {
            public ulong HashID;
            public fixed byte Padding[24];
            public int Size;
            public ushort Width;
            public ushort Height;
        };

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct GfxImage
        {
            public long Name;
            public fixed byte Padding[12];
            public uint DXGIFormat;
            public fixed byte Padding2[12];
            public ushort LoadedMipWidth;
            public ushort LoadedMipHeight;
            public fixed byte Padding3[10];
            public byte LoadedMipLevels;
            public fixed byte Padding4[5];
            public GfxMip MipLevel0;
            public GfxMip MipLevel1;
            public GfxMip MipLevel2;
            public GfxMip MipLevel3;
            public fixed byte Padding5[16];
        };

        [StructLayout(LayoutKind.Sequential, Size = 120)]
        internal unsafe struct MaterialAsset
        {
            public long Name;
            public fixed byte Padding[20];
            public byte ImageCount;
            public fixed byte Padding2[35];
            public long TechsetPtr;
            public long ImageTable;
            public fixed byte Padding3[24];
            public long MaterialSettingsInfo;
            public long UnknownZero;
        };

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        internal struct MaterialTexture
        {
            public uint Type;
            public long Image;
        }

        internal class InGameImage
        {
            public string Name { get; set; }

            public string Type { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public ScratchImageFormat Format { get; set; }

            public int MipMapCount { get; set; }

            public List<GfxMip> MipMaps { get; set; }
        }

        internal class InGameMaterial
        {
            public string Name { get; set; }

            public List<InGameImage> Images { get; set; }
        }

        public static CascStorage Storage { get; set; }

        public static List<XPAKPackage> Packages { get; set; }

        public static Dictionary<ulong, XPAKPackage.XPAKPackageEntry> Entries { get; set; }

        public static string GameDirectory { get; set; }

        private static BinaryReader ProcessReader { get; set; }

        private static long[] AssetPoolsPointers { get; set; }

        private static int[] AssetPoolsHeaderSizes { get; set; }

        private static int[] AssetPoolsSizes { get; set; }

        private static Dictionary<string, InGameMaterial> InGameMaterials { get; set; } = new Dictionary<string, InGameMaterial>();

        /// <summary>
        /// Gets or Sets the current image extension.
        /// </summary>
        public static string ImageExtension { get; set; }

        public static bool Initialize()
        {
            try
            {
                ImageExtension = ".png";
                var processes = Process.GetProcessesByName("ModernWarfare");

                if (processes.Length >= 1)
                {
                    try
                    {
                        var process = processes[0];

                        Packages = new List<XPAKPackage>();
                        Entries = new Dictionary<ulong, XPAKPackage.XPAKPackageEntry>();
                        GameDirectory = Path.GetDirectoryName(process.MainModule.FileName);

                        Printer.WriteLine("MATERIAL", $"Attempting to initialize in-game exporting...");

                        ProcessReader = new BinaryReader(new ProcessStream(process, ProcessStream.AccessRightsFlags.Read));

                        ((ProcessStream)ProcessReader.BaseStream).StrictAccess = false;
                        var assetPoolsAddress = ProcessReader.BaseStream.Scan("49 63 C3 48 8B 8C C6 ?? ?? ?? ?? 48 85 C9", true);
                        ((ProcessStream)ProcessReader.BaseStream).StrictAccess = true;

                        if (assetPoolsAddress.Length == 0)
                            throw new Exception("Failed to locate Asset Pools via scan");

                        AssetPoolsPointers = ProcessReader.ReadStructArray<long>(117, ProcessReader.ReadInt32(assetPoolsAddress[0] + 0x07) + ((ProcessStream)ProcessReader.BaseStream).MainModuleBaseAddress).ToArray();
                        AssetPoolsHeaderSizes = ProcessReader.ReadStructArray<int>(117, ProcessReader.ReadInt32(assetPoolsAddress[0] + 0x1A) + ((ProcessStream)ProcessReader.BaseStream).MainModuleBaseAddress).ToArray();
                        AssetPoolsSizes = ProcessReader.ReadStructArray<int>(117, ProcessReader.ReadInt32(assetPoolsAddress[0] + 0x13) + ((ProcessStream)ProcessReader.BaseStream).MainModuleBaseAddress).ToArray();

                        if (ProcessReader.ReadUTF8NullTerminatedString(ProcessReader.ReadInt64(AssetPoolsPointers[9])) != "axis_guide_createfx")
                            throw new Exception("This version of Modern Warfare may not be supported, or the Scan failed.");


                        var assets = ProcessReader.ReadStructArray<MaterialAsset>(AssetPoolsSizes[0xB], AssetPoolsPointers[0xB]);
                        var start = AssetPoolsPointers[0xB];
                        var end = AssetPoolsPointers[0xB] + AssetPoolsSizes[0xB] * AssetPoolsHeaderSizes[0xB];

                        foreach (var asset in ProcessReader.ReadStructArray<MaterialAsset>(AssetPoolsSizes[0xB], AssetPoolsPointers[0xB]))
                        {
                            if (asset.Name >= start && asset.Name < end)
                                continue;
                            if (asset.Name == 0)
                                continue;
                            if (asset.ImageTable == 0)
                                continue;

                            var mtl = new InGameMaterial()
                            {
                                Name = Path.GetFileNameWithoutExtension(ProcessReader.ReadUTF8NullTerminatedString(asset.Name)),
                                Images = new List<InGameImage>(asset.ImageCount)
                            };

                            foreach (var entry in ProcessReader.ReadStructArray<MaterialTexture>(asset.ImageCount, asset.ImageTable))
                            {
                                var imageAsset = ProcessReader.ReadStruct<GfxImage>(entry.Image);

                                mtl.Images.Add(new InGameImage()
                                {
                                    Name = ProcessReader.ReadUTF8NullTerminatedString(imageAsset.Name),
                                    Type = entry.Type.ToString("x"),
                                    Width = imageAsset.LoadedMipWidth,
                                    Height = imageAsset.LoadedMipHeight,
                                    Format = Formats[imageAsset.DXGIFormat],
                                    MipMapCount = imageAsset.LoadedMipLevels,
                                    MipMaps = new List<GfxMip>()
                            {
                                imageAsset.MipLevel0,
                                imageAsset.MipLevel1,
                                imageAsset.MipLevel2,
                                imageAsset.MipLevel3,
                            }
                                });
                            }

                            InGameMaterials[mtl.Name] = mtl;
                        }

                        SaveCache();
                        Printer.WriteLine("MATERIAL", $"Successfully loaded {InGameMaterials.Count} materials from in-game");
                    }
                    catch (Exception e)
                    {
                        Global.VerbosePrint(e.ToString());
                    }
                }
                else
                {
                    LoadCache();
                }

                LoadPackages();
            }
            catch
            {

            }

            return true;
        }

        public static void SaveCache()
        {
            Directory.CreateDirectory($"{Global.WorkingDirectory}\\Data");

            using var writer = new BinaryWriter(File.Create($"{Global.WorkingDirectory}\\Data\\MaterialCache.scab"));

            writer.Write(GameDirectory);
            var vals = InGameMaterials.Values;
            writer.Write(vals.Count);
            foreach (var mtl in InGameMaterials.Values)
            {
                writer.Write(mtl.Name);
                writer.Write(mtl.Images.Count);

                foreach (var img in mtl.Images)
                {
                    writer.Write(img.Name);
                    writer.Write(img.Type);
                    writer.Write(img.Width);
                    writer.Write(img.Height);
                    writer.Write((int)img.Format);
                    writer.Write(img.MipMapCount);
                    writer.WriteStruct(img.MipMaps[0]);
                    writer.WriteStruct(img.MipMaps[1]);
                    writer.WriteStruct(img.MipMaps[2]);
                    writer.WriteStruct(img.MipMaps[3]);
                }
            }
        }

        public static void LoadCache()
        {
            try
            {
                if (File.Exists($"{Global.WorkingDirectory}\\Data\\MaterialCache.scab") && File.Exists($"{Global.WorkingDirectory}\\GameDirectory.txt"))
                {
                    Printer.WriteLine("MATERIAL", $"Attempting to load the material cache");

                    using var reader = new BinaryReader(File.OpenRead($"{Global.WorkingDirectory}\\Data\\MaterialCache.scab"));

                    GameDirectory = reader.ReadString();
                    GameDirectory = File.ReadAllText($"{Global.WorkingDirectory}\\GameDirectory.txt");
                    InGameMaterials = new Dictionary<string, InGameMaterial>();

                    var mtlCount = reader.ReadInt32();

                    for (int i = 0; i < mtlCount; i++)
                    {
                        var mtl = new InGameMaterial()
                        {
                            Name = reader.ReadString(),
                            Images = new List<InGameImage>(),
                        };

                        var imgCount = reader.ReadInt32();

                        for (int j = 0; j < imgCount; j++)
                        {
                            mtl.Images.Add(new InGameImage()
                            {
                                Name = reader.ReadString(),
                                Type = reader.ReadString(),
                                Width = reader.ReadInt32(),
                                Height = reader.ReadInt32(),
                                Format = (ScratchImageFormat)reader.ReadInt32(),
                                MipMapCount = reader.ReadInt32(),
                                MipMaps = new List<GfxMip>()
                                {
                                    reader.ReadStruct<GfxMip>(),
                                    reader.ReadStruct<GfxMip>(),
                                    reader.ReadStruct<GfxMip>(),
                                    reader.ReadStruct<GfxMip>()
                                }
                            });
                        }

                        InGameMaterials[mtl.Name] = mtl;
                    }

                    Printer.WriteLine("MATERIAL", $"Successfully loaded {InGameMaterials.Count} materials from the material cache");
                }
            }
            catch (Exception e)
            {
                Global.VerbosePrint(e.ToString());
            }
        }

        public static void LoadPackages()
        {
            Packages?.ForEach(x => x.Dispose());
            Packages = new List<XPAKPackage>();
            Storage?.Dispose();
            Storage = new CascStorage(GameDirectory);
            Entries = new Dictionary<ulong, XPAKPackage.XPAKPackageEntry>();

            foreach (var file in Storage.Files)
            {
                if (file.IsLocal && Path.GetExtension(file.FileName) == ".xpak")
                {
                    Packages.Add(new XPAKPackage(Storage.OpenFile(file.FileName)));
                }
            }

            foreach (var file in Directory.EnumerateFiles(Path.Combine(GameDirectory, "xpak_cache")))
            {
                if (Path.GetExtension(file) == ".xpak")
                {
                    Packages.Add(new XPAKPackage(File.OpenRead(file)));
                }
            }

            Printer.WriteLine("MATERIAL", $"Successfully loaded {Entries.Count} objects from XPAKs");
        }

        private static ScratchImageFileFormat GetImageFormatForExtension(string extension)
        {
            // DDS Files
            if (extension.Equals(".dds", StringComparison.CurrentCultureIgnoreCase))
                return ScratchImageFileFormat.DDS;
            // PNG Files
            if (extension.Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                return ScratchImageFileFormat.PNG;
            // BMP Files
            if (extension.Equals(".bmp", StringComparison.CurrentCultureIgnoreCase))
                return ScratchImageFileFormat.BMP;
            // Targa Files
            if (extension.Equals(".tga", StringComparison.CurrentCultureIgnoreCase))
                return ScratchImageFileFormat.TGA;
            // JPG Files
            if (extension.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase))
                return ScratchImageFileFormat.JPG;
            // JPEG Files
            if (extension.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase))
                return ScratchImageFileFormat.JPG;
            // TIF Files
            if (extension.Equals(".tif", StringComparison.CurrentCultureIgnoreCase))
                return ScratchImageFileFormat.TIF;
            // TIFF Files
            if (extension.Equals(".tiff", StringComparison.CurrentCultureIgnoreCase))
                return ScratchImageFileFormat.TIF;
            // Return DDS by default
            return ScratchImageFileFormat.DDS;
        }

        public static bool TryExportMaterialImages(Material material, string dir)
        {
            dir = $"{dir}\\{material.Name}";

            if (InGameMaterials.TryGetValue(material.Name, out var ingameMtl))
            {
                foreach (var ingameImg in ingameMtl.Images)
                {
                    var result = $"{dir}\\{ingameImg.Name}{Global.ImageExtension}";

                    switch (ingameImg.Type)
                    {
                        case "0":
                            material.Images["DiffuseMap"] = result;
                            break;
                    }

                    if (!File.Exists(result))
                    {
                        try
                        {
                            // Calculate the largest image mip
                            int largestMip = 0;
                            int largestWidth = 0;
                            int largestHeight = 0;
                            int largestSize = 0;
                            ulong largestHash = 0;
                            bool exists = false;

                            // Loop and calculate
                            for (int i = 0; i < ingameImg.MipMaps.Count; i++)
                            {
                                // Compare widths
                                if (ingameImg.MipMaps[i].Width > largestWidth && Entries.ContainsKey(ingameImg.MipMaps[i].HashID))
                                {
                                    largestMip = i;
                                    largestWidth = ingameImg.MipMaps[i].Width;
                                    largestHeight = ingameImg.MipMaps[i].Height;
                                    largestHash = ingameImg.MipMaps[i].HashID;
                                    largestSize = i == 0 ? ingameImg.MipMaps[i].Size >> 4 : (ingameImg.MipMaps[i].Size >> 4) - (ingameImg.MipMaps[i - 1].Size >> 4);
                                    exists = Entries.ContainsKey(ingameImg.MipMaps[i].HashID);
                                }
                            }

                            if (largestHash == 0 || largestSize == 0)
                                continue;

                            if (exists && Entries.TryGetValue(largestHash, out var entry))
                            {
                                using var image = new ScratchImage(new ScratchImageMetadata()
                                {
                                    Width = (ulong)largestWidth,
                                    Height = (ulong)largestHeight,
                                    Depth = 1,
                                    ArraySize = 1,
                                    MiscFlags = ScratchImageFlags.None,
                                    MiscFlags2 = ScratchImageFlags2.NONE,
                                    Dimension = ScratchImageDimension.Texture2D,
                                    MipLevels = 1,
                                    Format = ingameImg.Format
                                }, entry.Package.Extract(entry, largestSize));

                                Directory.CreateDirectory(dir);

                                using var stream = File.Create(result);
                                image.ConvertImage(ScratchImageFormat.R8G8B8A8UNorm);
                                image.Save(stream, GetImageFormatForExtension(Global.ImageExtension));

                                Printer.WriteLine("MATERIAL", $"Exported Image: {ingameImg.Name}");
                            }
                        }
                        catch (Exception e)
                        {
                            Printer.WriteLine("MATERIAL", $"Failed to export Image: {ingameImg.Name}: {e.Message}");
                        }
                    }
                }
            }

            return true;
        }
    }
}