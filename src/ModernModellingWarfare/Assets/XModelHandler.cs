// ------------------------------------------------------------------------
// ModernModellingWarfare - Call of Duty Fast File Model Exporter
// Copyright (C) 2021 Philip/Scobalula
// ------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE' file, which is part of this source code package.
// ------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using ModernModellingWarfare.Shared;

namespace ModernModellingWarfare.Assets
{
    internal unsafe static class XModelHandler
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct XModel
        {
            public unsafe sbyte*                                                    Name;
            public ushort                                                           NumSurfaces;
            public byte                                                             NumLods;
            public byte                                                             MaxLODs;
            public fixed byte                                                       Padding0[8];
            public byte                                                             NumBones;
            public byte                                                             NumRootBones;
            public ushort                                                           UnkBoneCount;
            public fixed byte                                                       Padding1[96];
            public unsafe ScriptableDefHandler.AssetStruct*                         ScriptableMoverDef;
            public unsafe XAnimProceduralBonesHandler.AssetStruct*                  ProceduralBones;
            public unsafe XAnimDynamicBonesHandler.AssetStruct*                     DynamicBones;
            public unsafe uint*                                                     UnkPtr01;
            public unsafe uint*                                                     BoneNames;
            public unsafe byte*                                                     ParentList;
            public unsafe XQuat*                                                    Rotations;
            public unsafe Vector3*                                                  Translations;
            public unsafe byte*                                                     PartClassification;
            public unsafe DObjAnimMat*                                              BaseMatrices;
            public unsafe void*                                                     UnkPtr02;
            public unsafe void*                                                     UnkPtr03;
            public unsafe MaterialHandler.Material**                                MaterialHandles;
            public XModelLodInfo                                                    LodInfo0;
            public XModelLodInfo                                                    LodInfo1;
            public XModelLodInfo                                                    LodInfo2;
            public XModelLodInfo                                                    LodInfo3;
            public XModelLodInfo                                                    LodInfo4;
            public XModelLodInfo                                                    LodInfo5;
            public unsafe void*                                                     UnkPtr04;
            public unsafe void*                                                     UnkPtr05;
            public unsafe XPhysicsAssetHandler.XPhysicsAsset*                       PhysicsAsset;
            public unsafe void*                                                     UnkPtr06;
            public unsafe XModelDetailCollisionHandler.XModelDetailCollision*       DetailCollision;
            public unsafe void*                                                     UnkPtr07;
            public unsafe void*                                                     UnkPtr08;
            public unsafe void*                                                     UnkPtr09;
            public unsafe void*                                                     UnkPtr10;
            public unsafe UnkXModelStructure01*                                     UnkPtr11;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XModelLodInfo
        {
            public unsafe XModelSurfsHandler.XModelSurfaces*                        Surfaces;
            public unsafe XModelSurfsHandler.XModelSurface*                         Surface;
            public float                                                            Dist;
            public ushort                                                           SurfaceCount;
            public ushort                                                           SurfIndex;
            public unsafe fixed byte                                                Unk01[32];
            public int                                                              Unk02;
            public byte                                                             Unk03;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UnkXModelStructure02
        {
            public fixed byte                                                       Padding0[60];
            public unsafe MaterialHandler.Material*                                 UnkMaterial;
            public unsafe byte*                                                     UnkPtr;
            public fixed byte                                                       Padding1[28];
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct UnkXModelStructure01
        {
            public unsafe UnkXModelStructure02*                                     UnkStructures;
            public int                                                              UnkCount;
        }

        public static XModel** VarAssetPtr;
        public static XModel* VarAsset;

        public static void LoadAssetPtr(Zone zone, bool atStreamStart)
        {
            zone.LoadStream(VarAssetPtr, IntPtr.Size, atStreamStart);
            zone.DataBufferIndex = 1;

            if (!Zone.IsNullPointer(*VarAssetPtr))
            {
                if ((long)*VarAssetPtr == -1 || (long)*VarAssetPtr == -2 || (long)*VarAssetPtr == -3)
                {
                    VarAsset = (XModel*)zone.AllocStreamPosition(7);
                    LoadXModel(zone, true);
                    *VarAssetPtr = VarAsset;
                }
                else
                {
                }
            }
        }

        public static void LoadXModel(Zone zone, bool atStreamStart)
        {
            zone.LoadStream(VarAsset, sizeof(XModel), true);

            var name = VarAsset;

            if ((long)VarAsset->UnkPtr03 != 0)
            {
                Global.DebugPrint("UnkPtr03 in XModel was a non-zero");
                throw new Exception();
            }
            if ((long)VarAsset->UnkPtr06 != 0)
            {
                Global.DebugPrint("UnkPtr05 in XModel was a non-zero");
                throw new Exception();
            }
            if ((long)VarAsset->UnkPtr07 != 0)
            {
                Global.DebugPrint("UnkPtr06 in XModel was a non-zero");
                throw new Exception();
            }
            if ((long)VarAsset->UnkPtr08 != 0)
            {
                Global.DebugPrint("UnkPtr08 in XModel was a non-zero");
                throw new Exception();
            }
            if ((long)VarAsset->UnkPtr09 != 0)
            {
                Global.DebugPrint("UnkPtr09 in XModel was a non-zero");
                throw new Exception();
            }

            zone.DataBufferIndex = 5;
            zone.PushStartPosition();

            VarAsset->Name = (long)VarAsset->Name == -1 || (long)VarAsset->Name == -2 || (long)VarAsset->Name == -2 ? zone.LoadXString(0UL, true) : (sbyte*)zone.CalculateStreamPosition(VarAsset->Name);

            Global.VerbosePrint($"Loading XModel: {new string(VarAsset->Name)}");
            Global.VerbosePrint($"Zone Position: {zone.ZoneMemoryPosition}");
            Global.VerbosePrint($"Stream Position: {zone.FastFileStream.Position}");

            var sdTemp = ScriptableDefHandler.VarAssetPtr;
            ScriptableDefHandler.VarAssetPtr = &VarAsset->ScriptableMoverDef;
            ScriptableDefHandler.LoadAssetPointer(zone, false);
            ScriptableDefHandler.VarAssetPtr = sdTemp;

            var apbTemp = XAnimProceduralBonesHandler.VarAssetPtr;
            XAnimProceduralBonesHandler.VarAssetPtr = &VarAsset->ProceduralBones;
            XAnimProceduralBonesHandler.LoadAssetPtr(zone, false);
            XAnimProceduralBonesHandler.VarAssetPtr = apbTemp;

            var adbTemp = XAnimDynamicBonesHandler.VarAssetPtr;
            XAnimDynamicBonesHandler.VarAssetPtr = &VarAsset->DynamicBones;
            XAnimDynamicBonesHandler.LoadAssetPointer(zone, false);
            XAnimDynamicBonesHandler.VarAssetPtr = adbTemp;

            if ((long)VarAsset->UnkPtr01 != -1L && (long)VarAsset->UnkPtr01 != -2L && (long)VarAsset->UnkPtr01 != -3L)
            {
                VarAsset->UnkPtr01 = (uint*)zone.CalculateStreamPosition(VarAsset->UnkPtr01);
            }
            else
            {
                VarAsset->UnkPtr01 = (uint*)zone.AllocStreamPosition(3UL);
                zone.LoadStream(VarAsset->UnkPtr01, VarAsset->Padding0[6], true);
            }

            if ((long)VarAsset->BoneNames != -1L && (long)VarAsset->BoneNames != -2L && (long)VarAsset->BoneNames != -3L)
            {
                VarAsset->BoneNames = (uint*)zone.CalculateStreamPosition(VarAsset->BoneNames);
            }
            else
            {
                VarAsset->BoneNames = (uint*)zone.AllocStreamPosition(3UL);
                zone.LoadStream(VarAsset->BoneNames, (VarAsset->NumBones + VarAsset->UnkBoneCount) * 4, true);
            }
            // Parents
            if ((long)VarAsset->ParentList != -1L && (long)VarAsset->ParentList != -2L && (long)VarAsset->ParentList != -3L)
            {
                VarAsset->ParentList = zone.CalculateStreamPosition(VarAsset->ParentList);
            }
            else
            {
                VarAsset->ParentList = zone.AllocStreamPosition(0UL);
                zone.LoadStream(VarAsset->ParentList, VarAsset->NumBones + VarAsset->UnkBoneCount - VarAsset->NumRootBones, true);
            }
            // Quaternions
            if ((long)VarAsset->Rotations != -1L && (long)VarAsset->Rotations != -2L && (long)VarAsset->Rotations != -3L)
            {
                VarAsset->Rotations = (XQuat*)zone.CalculateStreamPosition(VarAsset->Rotations);
            }
            else
            {
                VarAsset->Rotations = (XQuat*)zone.AllocStreamPosition(1UL);
                zone.LoadStream(VarAsset->Rotations, (VarAsset->NumBones + VarAsset->UnkBoneCount - VarAsset->NumRootBones) * 8, true);
            }
            // Translations
            if ((long)VarAsset->Translations != -1L && (long)VarAsset->Translations != -2L && (long)VarAsset->Translations != -3L)
            {
                VarAsset->Translations = (Vector3*)zone.CalculateStreamPosition(VarAsset->Translations);
            }
            else
            {
                VarAsset->Translations = (Vector3*)zone.AllocStreamPosition(3UL);
                zone.LoadStream(VarAsset->Translations, (VarAsset->NumBones + VarAsset->UnkBoneCount - VarAsset->NumRootBones) * 12, true);
            }
            // Part Classifications
            if ((long)VarAsset->PartClassification != -1L && (long)VarAsset->PartClassification != -2L && (long)VarAsset->PartClassification != -3L)
            {
                VarAsset->PartClassification = zone.CalculateStreamPosition(VarAsset->PartClassification);
            }
            else
            {
                VarAsset->PartClassification = zone.AllocStreamPosition(0UL);
                zone.LoadStream(VarAsset->PartClassification, VarAsset->NumBones, true);
            }
            // Base Matrices
            if ((long)VarAsset->BaseMatrices != -1L && (long)VarAsset->BaseMatrices != -2L && (long)VarAsset->BaseMatrices != -3L)
            {
                VarAsset->BaseMatrices = (DObjAnimMat*)zone.CalculateStreamPosition(VarAsset->BaseMatrices);
            }
            else
            {
                VarAsset->BaseMatrices = (DObjAnimMat*)zone.AllocStreamPosition(15UL);
                zone.LoadStream(VarAsset->BaseMatrices, (VarAsset->NumBones + VarAsset->UnkBoneCount) * sizeof(DObjAnimMat), true);
            }
            // Unknown
            if ((long)VarAsset->UnkPtr02 != -1L && (long)VarAsset->UnkPtr02 != -2L && (long)VarAsset->UnkPtr02 != -3L)
            {
                VarAsset->UnkPtr02 = zone.CalculateStreamPosition(VarAsset->UnkPtr02);
            }
            else
            {
                VarAsset->UnkPtr02 = zone.AllocStreamPosition(3UL);
                zone.LoadStream(VarAsset->UnkPtr02, 48, true);
            }
            // Materials
            if (VarAsset->MaterialHandles != null)
            {
                VarAsset->MaterialHandles = (MaterialHandler.Material**)zone.AllocStreamPosition(7UL);
                zone.LoadStream(VarAsset->MaterialHandles, VarAsset->NumSurfaces * 8, true);

                for (int index = 0; index < VarAsset->NumSurfaces; ++index)
                {
                    var matTemp = MaterialHandler.VarAssetPtr;
                    MaterialHandler.VarAssetPtr = &VarAsset->MaterialHandles[index];
                    MaterialHandler.LoadAssetPtr(zone, false);
                    MaterialHandler.VarAssetPtr = matTemp;
                }
            }
            // LODs
            LoadLOD(zone, ref VarAsset->LodInfo0);
            LoadLOD(zone, ref VarAsset->LodInfo1);
            LoadLOD(zone, ref VarAsset->LodInfo2);
            LoadLOD(zone, ref VarAsset->LodInfo3);
            LoadLOD(zone, ref VarAsset->LodInfo4);
            LoadLOD(zone, ref VarAsset->LodInfo5);
            // Unknown (looks like BoneInfo for Hitbox)
            if ((long)VarAsset->UnkPtr04 != -1L && (long)VarAsset->UnkPtr04 != -2L && (long)VarAsset->UnkPtr04 != -3L)
            {
                VarAsset->UnkPtr04 = zone.CalculateStreamPosition(VarAsset->UnkPtr04);
            }
            else
            {
                VarAsset->UnkPtr04 = zone.AllocStreamPosition(3UL);
                zone.LoadStream(VarAsset->UnkPtr04, (VarAsset->NumBones + VarAsset->UnkBoneCount) * 28, true);
            }
            // Unknown
            if ((long)VarAsset->UnkPtr05 != -1L && (long)VarAsset->UnkPtr05 != -2L && (long)VarAsset->UnkPtr05 != -3L)
            {
                VarAsset->UnkPtr05 = (float*)zone.CalculateStreamPosition(VarAsset->UnkPtr05);
            }
            else
            {
                VarAsset->UnkPtr05 = (float*)zone.AllocStreamPosition(3UL);
                zone.LoadStream(VarAsset->UnkPtr05, VarAsset->NumSurfaces * 4, true);
            }

            var paTemp = XPhysicsAssetHandler.VarAssetPtr;
            XPhysicsAssetHandler.VarAssetPtr = &VarAsset->PhysicsAsset;
            XPhysicsAssetHandler.LoadAssetPtr(zone, false);
            XPhysicsAssetHandler.VarAssetPtr = paTemp;

            var xdcTemp = XModelDetailCollisionHandler.VarAssetPtr;
            XModelDetailCollisionHandler.VarAssetPtr = &VarAsset->DetailCollision;
            XModelDetailCollisionHandler.LoadAssetPtr(zone, false);
            XModelDetailCollisionHandler.VarAssetPtr = xdcTemp;

            if (VarAsset->UnkPtr11 != null)
            {
                VarAsset->UnkPtr11 = (UnkXModelStructure01*)zone.AllocStreamPosition(7UL);
                zone.LoadStream(VarAsset->UnkPtr11, sizeof(UnkXModelStructure01), true);

                if (VarAsset->UnkPtr11->UnkStructures != null)
                {
                    VarAsset->UnkPtr11->UnkStructures = (UnkXModelStructure02*)zone.AllocStreamPosition(7UL);
                    zone.LoadStream(VarAsset->UnkPtr11->UnkStructures, sizeof(UnkXModelStructure02) * VarAsset->UnkPtr11->UnkCount, true);

                    for (int index = 0; index < VarAsset->UnkPtr11->UnkCount; ++index)
                    {
                        var tempMatIndex = MaterialHandler.VarAssetPtr;
                        MaterialHandler.VarAssetPtr = &VarAsset->UnkPtr11->UnkStructures[index].UnkMaterial;
                        MaterialHandler.LoadAssetPtr(zone, false);
                        MaterialHandler.VarAssetPtr = tempMatIndex;

                        if ((IntPtr)VarAsset->UnkPtr11->UnkStructures[index].UnkPtr != IntPtr.Zero)
                        {
                            Global.DebugPrint("UnkPtr in UnkPtr11 was a non-zero");
                            throw new Exception();
                        }
                    }
                }
            }

            zone.PopStartPosition();
        }

        public static void LoadLOD(Zone zone, ref XModelLodInfo lod)
        {
            if (lod.Surfaces == null)
                return;

            var ptr = (XModelSurfsHandler.XModelSurfaces**)zone.CalculateStreamPosition(lod.Surfaces);

            lod.Surfaces = *ptr;
            lod.Surface = lod.Surfaces->Surfaces;
        }
    }
}
