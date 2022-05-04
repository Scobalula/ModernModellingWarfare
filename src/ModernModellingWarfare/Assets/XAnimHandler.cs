using PhilLibX;
using PhilLibX.Media3D;
using PhilLibX.Media3D.FileTranslators;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ModernModellingWarfare.Assets
{
    /// <summary>
    /// A class to handle parsing XAnims from a Modern Warfare 2019 Fast File
    /// </summary>
    internal unsafe static class XAnimHandler
    {
        /// <summary>
        /// XAnim Notification
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct XAnimNotifyInfo
        {
            public uint Name;
            public float Time;
        };

        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct XAnimDeltaPart
        {
            public void* Translations;
            public void* Quaternions;
            public void* Quaternions2;
        };

        /// <summary>
        /// XAnim Asset
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct XAnim
        {
            public sbyte* Name;
            public uint* BoneNames;
            public byte* DataByte;
            public short* DataShort;
            public int* DataInt;
            public short* RandomDataShort;
            public byte* RandomDataByte;
            public int* RandomDataInt;
            public byte* IndicesData;
            public XAnimNotifyInfo* Notifications;
            public void* Unk00;
            public XAnimDeltaPart* DeltaPart;
            public int RandomDataShortCount;
            public int RandomDataByteCount;
            public int IndexCount;
            public float Framerate;
            public float Frequency;
            public int DataByteCount;
            public ushort DataShortCount;
            public ushort DataIntCount;
            public ushort RandomDataIntCount;
            public ushort FrameCount;
            public byte Flags;
            public fixed byte BoneCounts[10];
            public byte NotifyCount;
            public byte Unk01;
            public byte Unk02;
            public byte Unk03;
            public ushort Unk04;
            public void* UnkPtr01;
            public void* UnkPtr02;
        };

        /// <summary>
        /// A pointer to pointer of the asset being loaded
        /// </summary>
        public static XAnim** VarAssetPtr;

        /// <summary>
        /// A pointer to the asset being loaded
        /// </summary>
        public static XAnim* VarAsset;

        /// <summary>
        /// The animation translator.
        /// </summary>
        public static SEAnimTranslator Translator => new();

        public static void LoadAssetPtr(Zone zone, bool atStreamStart)
        {
            zone.LoadStream(VarAssetPtr, IntPtr.Size, atStreamStart);
            zone.DataBufferIndex = 1;

            if (!Zone.IsNullPointer(*VarAssetPtr))
            {
                if ((long)*VarAssetPtr == -1 || (long)*VarAssetPtr == -2 || (long)*VarAssetPtr == -3)
                {
                    VarAsset = (XAnim*)zone.AllocStreamPosition(7);
                    LoadAsset(zone, true);
                    *VarAssetPtr = VarAsset;
                }
                else
                {
                    // TODO: Ref
                }
            }
        }

        public static void LoadAsset(Zone zone, bool atStreamStart)
        {
            zone.LoadStream(VarAsset, sizeof(XAnim), true);

            if (VarAsset->Unk00 != null)
                throw new IOException("Unk00 in XAnim was not a nullptr, this fast file is not supported.");
            if (VarAsset->UnkPtr01 != null)
                throw new IOException("UnkPtr01 in XAnim was not a nullptr, this fast file is not supported.");
            if (VarAsset->UnkPtr02 != null)
                throw new IOException("UnkPtr02 in XAnim was not a nullptr, this fast file is not supported.");

            zone.DataBufferIndex = 5;
            zone.PushStartPosition();

            VarAsset->Name = (long)VarAsset->Name == -1 || (long)VarAsset->Name == -2 || (long)VarAsset->Name == -2 ? zone.LoadXString(0UL, true) : (sbyte*)zone.CalculateStreamPosition(VarAsset->Name);

            Global.VerbosePrint($"Loading XAnim: {new string(VarAsset->Name)} @ {zone.FastFileStream.Position}...");

            if ((long)VarAsset->BoneNames != -1L && (long)VarAsset->BoneNames != -2L && (long)VarAsset->BoneNames != -3L)
            {
                VarAsset->BoneNames = (uint*)zone.CalculateStreamPosition(VarAsset->BoneNames);
            }
            else
            {
                VarAsset->BoneNames = (uint*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarAsset->BoneNames, VarAsset->BoneCounts[9] * 4, true);
            }
            if ((long)VarAsset->Notifications != -1L && (long)VarAsset->Notifications != -2L && (long)VarAsset->Notifications != -3L)
            {
                VarAsset->Notifications = (XAnimNotifyInfo*)zone.CalculateStreamPosition(VarAsset->Notifications);
            }
            else
            {
                VarAsset->Notifications = (XAnimNotifyInfo*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarAsset->Notifications, VarAsset->NotifyCount * sizeof(XAnimNotifyInfo), true);
            }
            if (VarAsset->DeltaPart != null)
            {
                // Some viewmodel animations contain delta parts, but don't have
                // any actual animation data? Ran into it a few times
                VarAsset->DeltaPart = (XAnimDeltaPart*)zone.AllocStreamPosition(7);
                zone.LoadStream(VarAsset->DeltaPart, sizeof(XAnimDeltaPart), true);

                if (VarAsset->DeltaPart->Quaternions2 != null)
                    throw new Exception("Delta Quaternions not supported.");
                if (VarAsset->DeltaPart->Translations != null)
                    throw new Exception("Delta Quaternions not supported.");
                if (VarAsset->DeltaPart->Quaternions != null)
                    throw new Exception("Delta Quaternions not supported.");
            }
            if ((long)VarAsset->DataByte != -1L && (long)VarAsset->DataByte != -2L && (long)VarAsset->DataByte != -3L)
            {
                VarAsset->DataByte = zone.CalculateStreamPosition(VarAsset->DataByte);
            }
            else
            {
                VarAsset->DataByte = zone.AllocStreamPosition(0);
                zone.LoadStream(VarAsset->DataByte, VarAsset->DataByteCount, true);
            }
            if ((long)VarAsset->DataShort != -1L && (long)VarAsset->DataShort != -2L && (long)VarAsset->DataShort != -3L)
            {
                VarAsset->DataShort = (short*)zone.CalculateStreamPosition(VarAsset->DataShort);
            }
            else
            {
                VarAsset->DataShort = (short*)zone.AllocStreamPosition(1);
                zone.LoadStream(VarAsset->DataShort, VarAsset->DataShortCount * 2, true);
            }
            if ((long)VarAsset->DataInt != -1L && (long)VarAsset->DataInt != -2L && (long)VarAsset->DataInt != -3L)
            {
                VarAsset->DataInt = (int*)zone.CalculateStreamPosition(VarAsset->DataInt);
            }
            else
            {
                VarAsset->DataInt = (int*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarAsset->DataInt, VarAsset->DataIntCount * 4, true);
            }
            if ((long)VarAsset->RandomDataShort != -1L && (long)VarAsset->RandomDataShort != -2L && (long)VarAsset->RandomDataShort != -3L)
            {
                VarAsset->RandomDataShort = (short*)zone.CalculateStreamPosition(VarAsset->RandomDataShort);
            }
            else
            {
                VarAsset->RandomDataShort = (short*)zone.AllocStreamPosition(1);
                zone.LoadStream(VarAsset->RandomDataShort, VarAsset->RandomDataShortCount * 2, true);
            }
            if ((long)VarAsset->RandomDataByte != -1L && (long)VarAsset->RandomDataByte != -2L && (long)VarAsset->RandomDataByte != -3L)
            {
                VarAsset->RandomDataByte = zone.CalculateStreamPosition(VarAsset->RandomDataByte);
            }
            else
            {
                VarAsset->RandomDataByte = zone.AllocStreamPosition(0);
                zone.LoadStream(VarAsset->RandomDataByte, VarAsset->RandomDataByteCount, true);
            }
            if ((long)VarAsset->IndicesData != -1L && (long)VarAsset->IndicesData != -2L && (long)VarAsset->IndicesData != -3L)
            {
                VarAsset->IndicesData = zone.CalculateStreamPosition(VarAsset->IndicesData);
            }
            else
            {
                VarAsset->IndicesData = zone.AllocStreamPosition(VarAsset->FrameCount >= 0x100 ? (ulong)1 : 0);
                zone.LoadStream(VarAsset->IndicesData, VarAsset->IndexCount * (VarAsset->FrameCount >= 0x100 ? 2 : 1), true);
            }

            Global.DebugPrint($"Loading XAnim: {new string(VarAsset->Name)} @ {zone.FastFileStream.Position}...");

            zone.PopStartPosition();
        }

        /// <summary>
        /// Converts the provided xanim to a generic animation object
        /// </summary>
        public static void ConvertXAnim(Zone zone, XAnim* xanim)
        {
            var indices = (ushort*)xanim->IndicesData;
            var dataByte = xanim->DataByte;
            var dataShort = xanim->DataShort;
            var dataInt = xanim->DataInt;
            var randomDataByte = xanim->RandomDataByte;
            var randomDataShort = xanim->RandomDataShort;
            var randomDataInt = xanim->RandomDataInt;
            var byteFrames = xanim->FrameCount < 0x100;

            var name = new string(xanim->Name);

            Printer.WriteLine("XANIM", $"Converting XAnim: {name}");

            var dir = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "exported_files", "xanims");
            Directory.CreateDirectory(dir);

            var watch = Stopwatch.StartNew();

            Global.DebugPrint($"Frames: {xanim->FrameCount}");
            Global.DebugPrint($"Framerate: {xanim->Framerate}");
            Global.DebugPrint($"Notifications: {xanim->NotifyCount}");
            Global.DebugPrint($"Bones: {xanim->BoneCounts[9]}");

            var currentBoneIndex = 0;
            var currentSize = xanim->BoneCounts[0];
            var anim = new Animation(name)
            {
                Framerate = xanim->Framerate,
                TransformType = AnimationTransformType.Relative,
            };
            var bones = new AnimationBone[xanim->BoneCounts[9]];

            for (int i = 0; i < xanim->BoneCounts[9]; i++)
            {
                bones[i] = new AnimationBone(zone.AssetList->StringList.GetByIndex(xanim->BoneNames[i]));
                anim.Bones.Add(bones[i]);
            }

            while (currentBoneIndex < currentSize)
            {
                var bone = bones[currentBoneIndex++];
                Global.DebugPrint($"Processing rotations for bone: {bone.Name}");
                bone.RotationFrames.Add(new(0, Quaternion.Identity));
            }

            currentSize += xanim->BoneCounts[1];

            while (currentBoneIndex < currentSize)
            {
                var bone = bones[currentBoneIndex++];
                Global.DebugPrint($"Processing rotations for bone: {bone.Name}");

                var tableSize = *dataShort++;

                if (tableSize >= 0x40 && !byteFrames)
                    dataShort += (tableSize - 1 >> 8) + 2;

                for (int i = 0; i < tableSize + 1; i++)
                {
                    int frame = 0;
                    int x = 0;
                    int y = 0;
                    int z = *randomDataShort++;
                    int w = *randomDataShort++;

                    if (byteFrames)
                    {
                        frame = *dataByte++;
                    }
                    else
                    {
                        frame = tableSize >= 0x40 ? *indices++ : *dataShort++;
                    }

                    if (frame > xanim->FrameCount)
                        Debugger.Break();

                    bone.RotationFrames.Add(new(frame, new(
                        x * 0.000030518509f,
                        y * 0.000030518509f,
                        z * 0.000030518509f,
                        w * 0.000030518509f)));
                }
            }

            currentSize += xanim->BoneCounts[2];

            while (currentBoneIndex < currentSize)
            {
                var bone = bones[currentBoneIndex++];
                Global.DebugPrint($"Processing rotations for bone: {bone.Name}");

                var tableSize = *dataShort++;

                if (tableSize >= 0x40 && !byteFrames)
                    dataShort += (tableSize - 1 >> 8) + 2;

                for (int i = 0; i < tableSize + 1; i++)
                {
                    int frame = 0;
                    int x = *randomDataShort++;
                    int y = *randomDataShort++;
                    int z = *randomDataShort++;
                    int w = *randomDataShort++;

                    if (byteFrames)
                    {
                        frame = *dataByte++;
                    }
                    else
                    {
                        frame = tableSize >= 0x40 ? *indices++ : *dataShort++;
                    }

                    if (frame > xanim->FrameCount)
                        Debugger.Break();

                    bone.RotationFrames.Add(new(frame, new(
                        x * 0.000030518509f,
                        y * 0.000030518509f,
                        z * 0.000030518509f,
                        w * 0.000030518509f)));
                }
            }

            currentSize += xanim->BoneCounts[3];

            while (currentBoneIndex < currentSize)
            {
                var bone = bones[currentBoneIndex++];
                Global.DebugPrint($"Processing rotations for bone: {bone.Name}");

                int frame = 0;
                int x = 0;
                int y = 0;
                int z = *dataShort++;
                int w = *dataShort++;

                bone.RotationFrames.Add(new(frame, new(
                    x * 0.000030518509f,
                    y * 0.000030518509f,
                    z * 0.000030518509f,
                    w * 0.000030518509f)));
            }

            currentSize += xanim->BoneCounts[4];

            while (currentBoneIndex < currentSize)
            {
                var bone = bones[currentBoneIndex++];
                Global.DebugPrint($"Processing rotations for bone: {bone.Name}");

                int frame = 0;
                int x = *dataShort++;
                int y = *dataShort++;
                int z = *dataShort++;
                int w = *dataShort++;

                bone.RotationFrames.Add(new(frame, new(
                    x * 0.000030518509f,
                    y * 0.000030518509f,
                    z * 0.000030518509f,
                    w * 0.000030518509f)));
            }

            currentBoneIndex = 0;
            currentSize = xanim->BoneCounts[5];

            while (currentBoneIndex++ < currentSize)
            {
                var bone = bones[*dataByte++];
                Global.DebugPrint($"Processing translations for bone: {bone.Name} from randomDataByte");

                var tableSize = *dataShort++;

                if (tableSize >= 0x40 && !byteFrames)
                    dataShort += (tableSize - 1 >> 8) + 2;

                var minsVecX = *(float*)dataInt++;
                var minsVecY = *(float*)dataInt++;
                var minsVecZ = *(float*)dataInt++;
                var frameVecX = *(float*)dataInt++;
                var frameVecY = *(float*)dataInt++;
                var frameVecZ = *(float*)dataInt++;

                for (int i = 0; i < tableSize + 1; i++)
                {
                    int frame = 0;
                    int x = *randomDataByte++;
                    int y = *randomDataByte++;
                    int z = *randomDataByte++;

                    if (byteFrames)
                    {
                        frame = *dataByte++;
                    }
                    else
                    {
                        frame = tableSize >= 0x40 ? *indices++ : *dataShort++;
                    }

                    if (frame > xanim->FrameCount)
                        Debugger.Break();

                    bone.TranslationFrames.Add(new(frame, new(
                        x * frameVecX + minsVecX,
                        y * frameVecY + minsVecY,
                        z * frameVecZ + minsVecZ)));
                }
            }

            currentBoneIndex = 0;
            currentSize = xanim->BoneCounts[6];

            while (currentBoneIndex++ < currentSize)
            {
                var bone = bones[*dataByte++];
                Global.DebugPrint($"Processing translations for bone: {bone.Name} from randomDataShort");

                var tableSize = *dataShort++;

                if (tableSize >= 0x40 && !byteFrames)
                    dataShort += (tableSize - 1 >> 8) + 2;

                var minsVecX = *(float*)dataInt++;
                var minsVecY = *(float*)dataInt++;
                var minsVecZ = *(float*)dataInt++;
                var frameVecX = *(float*)dataInt++;
                var frameVecY = *(float*)dataInt++;
                var frameVecZ = *(float*)dataInt++;

                for (int i = 0; i < tableSize + 1; i++)
                {
                    int frame = 0;
                    int x = (ushort)*randomDataShort++;
                    int y = (ushort)*randomDataShort++;
                    int z = (ushort)*randomDataShort++;

                    if (byteFrames)
                    {
                        frame = *dataByte++;
                    }
                    else
                    {
                        frame = tableSize >= 0x40 ? *indices++ : *dataShort++;
                    }

                    if (frame > xanim->FrameCount)
                        Debugger.Break();

                    bone.TranslationFrames.Add(new(frame, new(
                        x * frameVecX + minsVecX,
                        y * frameVecY + minsVecY,
                        z * frameVecZ + minsVecZ)));
                }
            }

            currentBoneIndex = 0;
            currentSize = xanim->BoneCounts[7];

            while (currentBoneIndex++ < currentSize)
            {
                var bone = bones[*dataByte++];
                Global.DebugPrint($"Processing translations for bone: {bone.Name} from dataInt");

                int frame = 0;

                bone.TranslationFrames.Add(new(frame, new(
                    *(float*)dataInt++,
                    *(float*)dataInt++,
                    *(float*)dataInt++)));
            }

            currentBoneIndex = 0;
            currentSize = xanim->BoneCounts[8];

            while (currentBoneIndex++ < currentSize)
            {
                var bone = bones[*dataByte++];
                Global.DebugPrint($"Processing translations for bone: {bone.Name}");
                bone.TranslationFrames.Clear();
            }


            for (int i = 0; i < xanim->NotifyCount; i++)
            {
                var notify = xanim->Notifications[i];
                anim.CreateNotification(zone.AssetList->StringList.GetByIndex(xanim->Notifications[i].Name)).Frames.Add(new AnimationFrame<Action>((int)(notify.Time * xanim->FrameCount), null));
            }

            if (dataByte != xanim->DataByte + xanim->DataByteCount)
                Global.DebugPrint($"dataByte != xanim->DataByte + xanim->DataByteCount");
            if (dataShort != xanim->DataShort + xanim->DataShortCount)
                Global.DebugPrint($"dataShort != xanim->DataShort + xanim->DataShortCount");
            if (dataInt != xanim->DataInt + xanim->DataIntCount)
                Global.DebugPrint($"dataInt != xanim->DataInt + xanim->DataIntCount");

            if (randomDataByte != xanim->RandomDataByte + xanim->RandomDataByteCount)
                Global.DebugPrint($"randomDataByte != xanim->RandomDataByte + xanim->RandomDataByteCount");
            if (randomDataShort != xanim->RandomDataShort + xanim->RandomDataShortCount)
                Global.DebugPrint($"randomDataShort != xanim->RandomDataShort + xanim->RandomDataShortCount");
            if (randomDataInt != xanim->RandomDataInt + xanim->RandomDataIntCount)
                Global.DebugPrint($"randomDataInt != xanim->RandomDataInt + xanim->RandomDataIntCount");

            if (randomDataInt != xanim->RandomDataInt + xanim->RandomDataIntCount)
                Global.DebugPrint($"randomDataInt != xanim->RandomDataInt + xanim->RandomDataIntCount");

            using var stream = File.Create(Path.Combine(dir, name + ".seanim"));

            Translator.Write(stream, name, anim, 2.54f, new());
        }
    }
}
