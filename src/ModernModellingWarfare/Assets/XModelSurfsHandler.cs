// ------------------------------------------------------------------------
// ModernModellingWarfare - Call of Duty Fast File Model Exporter
// Copyright (C) 2022 Philip/Scobalula
// ------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE' file, which is part of this source code package.
// ------------------------------------------------------------------------
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ModernModellingWarfare.Assets
{
    internal unsafe static class XModelSurfsHandler
    {
        #region Structures
        [DllImport("msvcrt.dll", EntryPoint = "memcpy_s", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern IntPtr MemCpySafe(void* dest, int destSize, void* src, int count);

        [StructLayout(LayoutKind.Sequential)]
        public struct XModelMeshBufferInfo
        {
            public unsafe byte*                         Buffer;
            public int                                  BufferSize;
            public int                                  Streamed;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XRigidVertList
        {
            public ushort                               BoneIndex;
            public ushort                               VertexCount;
            public ushort                               FaceCount;
            public ushort                               FaceIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XModelSurface
        {
            public ushort                               Flags;
            public ushort                               VertexCount;
            public ushort                               FaceCount;
            public ushort                               UnkCount;
            public byte                                 RigidWeightCount;
            public byte                                 UnkSubDivRelatedCount;
            public unsafe fixed byte                    Padding1[6];
            public uint                                 UnkHash;
            public unsafe fixed ushort                  BlendVertCounts[8];
            public int                                  BlendWeightsSize;
            public int                                  VerticesOffset;
            public int                                  UnkDataOffset;
            public int                                  UVsOffset;
            public int                                  NormalsOffset;
            public int                                  TriOffset;
            public int                                  Unk01Offset;
            public int                                  ColorOffset;
            public int                                  Unk02Offset;
            public int                                  UnkO3Offset;
            public int                                  UnkO4Offset;
            public int                                  Unk05Offset;
            public int                                  Unk06Offset;
            public unsafe XModelMeshBufferInfo*         MeshBuffer;
            public unsafe void*                         UnkPtr03;
            public unsafe XRigidVertList*               RigidWeights;
            public unsafe ushort*                       BlendWeights;
            public unsafe UnkSubDivRelatedStruct01*     UnkSubDiv;
            public unsafe fixed byte                    Padding00[32];
            public float                                XOffset;
            public float                                YOffset;
            public float                                ZOffset;
            public float                                Scale;
            public float                                Min;
            public float                                Max;
            public unsafe void*                         UnkPtr00;
            public unsafe void*                         UnkPtr01;
            public unsafe void*                         UnkPtr02;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct XModelSurfaces
        {
            public sbyte*                               Name;
            public XModelSurface*                       Surfaces;
            public unsafe fixed byte                    StreamInfo[32];
            public unsafe XModelMeshBufferInfo*         MeshBuffer;
            public ushort                               SurfaceCount;
            public unsafe fixed byte                    Padding[38];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UnkSubDivRelatedStruct02
        {
            public byte*                                UnkPtr00;
            public int                                  UnkCount01;
            public int                                  UnkCount02;
            public int                                  UnkCount03;
            public int                                  UnkCount04;
            public int                                  UnkCount05;
            public int                                  UnkCount06;
            public int                                  UnkCount07;
            public int                                  UnkCount08;
            public int                                  UnkCount09;
            public int                                  UnkCount10;
            public int                                  UnkCount11;
            public int                                  UnkCount12;
            public int                                  UnkCount13;
            public int                                  UnkCount14;
            public int                                  UnkCount15;
            public int                                  UnkCount16;
            public int                                  UnkCount17;
            public ushort*                              UnkPtr01;
            public ushort*                              UnkPtr02;
            public uint*                                UnkPtr03;
            public uint*                                UnkPtr04;
            public uint*                                UnkPtr05;
            public uint*                                UnkPtr06;
            public uint*                                UnkPtr07;
            public uint*                                UnkPtr08;
            public float*                               UnkPtr09;
            public fixed byte                           Padding0[414];
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct UnkSubDivRelatedStruct01
        {
            public UnkSubDivRelatedStruct02*            UnkSubDivPtr00;
            public unsafe fixed byte                    Padding[116];
        };

        public static UnkSubDivRelatedStruct02*         VarSubDivRelatedUnk;
        public static XRigidVertList*                   VarXRigidVertList;
        public static ushort*                           VarBlendVertsPtr;
        public static XModelMeshBufferInfo*             VarXModelMeshBuffer;
        public static XModelSurface*                    VarXSurfacePtr;
        public static XModelSurfaces**                  VarAssetPtr;
        public static XModelSurfaces*                   VarAsset;
        #endregion

        /// <summary>
        /// Loads the data of the given type.
        /// </summary>
        /// <param name="zone">Current zone we are loading from.</param>
        public static void LoadAssetPtr(Zone zone, bool atStreamStart)
        {
            zone.LoadStream(VarAssetPtr, IntPtr.Size, atStreamStart);
            zone.DataBufferIndex = 1;

            if (!Zone.IsNullPointer(*VarAssetPtr))
            {
                if ((long)*VarAssetPtr == -1 || (long)*VarAssetPtr == -2 || (long)*VarAssetPtr == -3)
                {
                    VarAsset = (XModelSurfaces*)zone.AllocStreamPosition(7);
                    LoadXModelSurfs(zone, true);
                    *VarAssetPtr = VarAsset;
                }
                else
                {
                    *VarAssetPtr = (XModelSurfaces*)zone.CalculateStreamPosition(*VarAssetPtr);
                }
            }
        }

        /// <summary>
        /// Loads the xmodel surface.
        /// </summary>
        /// <param name="zone">Current zone we are loading from.</param>
        /// <param name="atStreamStart"></param>
        public static void LoadXModelSurfs(Zone zone, bool atStreamStart)
        {
            zone.LoadStream(VarAsset, sizeof(XModelSurfaces), true);
            zone.DataBufferIndex = 5;
            zone.PushStartPosition();

            VarAsset->Name = (long)VarAsset->Name == -1 || (long)VarAsset->Name == -2 || (long)VarAsset->Name == -3 ? zone.LoadXString(0UL, true) : (sbyte*)zone.CalculateStreamPosition(VarAsset->Name);

            Global.DebugPrint($"Loading XModel Surfs: {new string(VarAsset->Name)}");

            if ((long)VarAsset->Surfaces != -1 && (long)VarAsset->Surfaces != -2 && (long)VarAsset->Surfaces != -2)
            {
                VarAsset->Surfaces = (XModelSurface*)zone.CalculateStreamPosition(VarAsset->Surfaces);
            }
            else
            {
                VarXSurfacePtr = (XModelSurface*)zone.AllocStreamPosition(15UL);
                VarAsset->Surfaces = VarXSurfacePtr;
                zone.LoadStream(VarXSurfacePtr, sizeof(XModelSurface) * VarAsset->SurfaceCount, true);

                for (int i = 0; i < VarAsset->SurfaceCount; i++)
                {
                    // Fix up pre-update structs, shift forward 4 bytes
                    if (VarXSurfacePtr->Unk05Offset == 0)
                    {
                        // Using MemCpy as .NET methods have weird behaviour in "optimized" mode....
                        MemCpySafe((byte*)VarXSurfacePtr + 16, sizeof(XModelSurface) - 16, (byte*)VarXSurfacePtr + 12, 56);
                    }

                    LoadXSurface(zone);
                    VarXSurfacePtr++;
                }
            }

            zone.PopStartPosition();
        }

        /// <summary>
        /// Loads an xsurface.
        /// </summary>
        /// <param name="zone">Current zone we are loading from.</param>
        public static void LoadXSurface(Zone zone)
        {
            Global.DebugPrint($"Loading XSurface: {zone.FastFileStream.Position}");

            if (VarXSurfacePtr->UnkPtr01 != null)
            {
                Global.DebugPrint("UnkPtr01 in XModel Surface was a non-zero");
                throw new Exception();
            }
            if (VarXSurfacePtr->UnkPtr02 != null)
            {
                Global.DebugPrint("UnkPtr02 in XModel Surface was a non-zero");
                throw new Exception();
            }
            if (VarXSurfacePtr->UnkPtr03 != null)
            {
                Global.DebugPrint("UnkPtr03 in XModel Surface was a non-zero");
                throw new Exception();
            }

            if (VarXSurfacePtr->MeshBuffer != null)
            {
                if ((long)VarXSurfacePtr->MeshBuffer != -1 && (long)VarXSurfacePtr->MeshBuffer != -2 && (long)VarXSurfacePtr->MeshBuffer != -2)
                {
                    VarXSurfacePtr->MeshBuffer = (XModelMeshBufferInfo*)zone.CalculateStreamPosition(VarXSurfacePtr->MeshBuffer);
                }
                else
                {
                    VarXModelMeshBuffer = (XModelMeshBufferInfo*)zone.AllocStreamPosition(7UL);
                    zone.LoadStream(VarXModelMeshBuffer, sizeof(XModelMeshBufferInfo), true);
                    LoadXModelMeshBuffer(zone);
                    VarXSurfacePtr->MeshBuffer = VarXModelMeshBuffer;
                }
            }
            if (VarXSurfacePtr->RigidWeights != null)
            {
                if ((long)VarXSurfacePtr->RigidWeights != -1 && (long)VarXSurfacePtr->RigidWeights != -2 && (long)VarXSurfacePtr->RigidWeights != -2)
                {
                    VarXSurfacePtr->RigidWeights = (XRigidVertList*)zone.CalculateStreamPosition(VarXSurfacePtr->RigidWeights);
                }
                else
                {
                    VarXRigidVertList = (XRigidVertList*)zone.AllocStreamPosition(1UL);
                    zone.LoadStream(VarXRigidVertList, sizeof(XRigidVertList) * VarXSurfacePtr->RigidWeightCount, true);
                    VarXSurfacePtr->RigidWeights = VarXRigidVertList;
                }
            }

            if (VarXSurfacePtr->BlendWeights != null)
            {
                if ((long)VarXSurfacePtr->BlendWeights != -1 && (long)VarXSurfacePtr->BlendWeights != -2 && (long)VarXSurfacePtr->BlendWeights != -2)
                {
                    VarXModelMeshBuffer->Buffer = zone.CalculateStreamPosition(VarXSurfacePtr->MeshBuffer);
                }
                else
                {
                    VarBlendVertsPtr = (ushort*)zone.AllocStreamPosition(3UL);
                    zone.LoadStream(VarBlendVertsPtr, (int)(VarXSurfacePtr->BlendWeightsSize & 0xFFFFFFFE), true);
                    VarXSurfacePtr->BlendWeights = VarBlendVertsPtr;
                }
            }
            if (VarXSurfacePtr->UnkSubDiv != null)
            {
                VarXSurfacePtr->UnkSubDiv = (UnkSubDivRelatedStruct01*)zone.AllocStreamPosition(0xF);
                LoadUnkSubDivBase(zone);
            }

            if ((long)VarXSurfacePtr->UnkPtr00 != -1 && (long)VarXSurfacePtr->UnkPtr00 != -2 && (long)VarXSurfacePtr->UnkPtr00 != -2)
            {
                VarXSurfacePtr->UnkPtr00 = zone.CalculateStreamPosition(VarXSurfacePtr->UnkPtr00);
            }
            else
            {
                VarXSurfacePtr->UnkPtr00 = zone.AllocStreamPosition(3UL);
                zone.LoadStream(VarXSurfacePtr->UnkPtr00, 24 * VarXSurfacePtr->RigidWeightCount, true);
            }
        }

        /// <summary>
        /// Loads xmodel mesh buffer.
        /// </summary>
        /// <param name="zone">Current zone we are loading from.</param>
        public static void LoadXModelMeshBuffer(Zone zone)
        {
            // We don't support streamed in this, use Greyhound if possible
            if ((VarXModelMeshBuffer->Streamed & 1) != 0 || VarXModelMeshBuffer->Buffer == null)
                return;

            if ((long)VarXModelMeshBuffer->Buffer != -3 && (long)VarXModelMeshBuffer->Buffer != -2 && (long)VarXModelMeshBuffer->Buffer != -1)
            {
                VarXModelMeshBuffer->Buffer = zone.CalculateStreamPosition(VarXModelMeshBuffer->Buffer);
            }
            else
            {
                VarXModelMeshBuffer->Buffer = zone.AllocStreamPosition(0xF);
                zone.LoadStream(VarXModelMeshBuffer->Buffer, VarXModelMeshBuffer->BufferSize, true);
            }
        }

        /// <summary>
        /// Loads the data of the given type.
        /// </summary>
        /// <param name="zone">Current zone we are loading from.</param>
        public static void LoadUnkSubDivBase(Zone zone)
        {
            // According to some strings in-game, this is subdiv related stuff
            zone.LoadStream(VarXSurfacePtr->UnkSubDiv, sizeof(UnkSubDivRelatedStruct01), true);

            if(VarXSurfacePtr->UnkSubDiv->UnkSubDivPtr00 != null)
            {
                VarSubDivRelatedUnk = (UnkSubDivRelatedStruct02*)zone.AllocStreamPosition(0x7);

                VarXSurfacePtr->UnkSubDiv->UnkSubDivPtr00 = VarSubDivRelatedUnk;

                zone.LoadStream(VarSubDivRelatedUnk, VarXSurfacePtr->UnkSubDivRelatedCount * sizeof(UnkSubDivRelatedStruct02), true);

                for (int index = 0; index < VarXSurfacePtr->UnkSubDivRelatedCount; ++index)
                {
                    LoadUnkSubDivStructure(zone);
                    ++VarSubDivRelatedUnk;
                }
            }
        }

        /// <summary>
        /// Loads the data of the given type.
        /// </summary>
        /// <param name="zone">Current zone we are loading from.</param>
        public static void LoadUnkSubDivStructure(Zone zone)
        {
            if(VarSubDivRelatedUnk->UnkPtr00 != null)
            {
                if ((long)VarSubDivRelatedUnk->UnkPtr00 != -1 && (long)VarSubDivRelatedUnk->UnkPtr00 != -2 && (long)VarSubDivRelatedUnk->UnkPtr00 != -2)
                {
                    VarSubDivRelatedUnk->UnkPtr00 = zone.CalculateStreamPosition(VarXSurfacePtr->MeshBuffer);
                }
                else
                {
                    VarSubDivRelatedUnk->UnkPtr00 = zone.AllocStreamPosition(3);
                    zone.LoadStream(VarSubDivRelatedUnk->UnkPtr00, VarXSurfacePtr->RigidWeightCount * 16, true);
                }
            }
            if (VarSubDivRelatedUnk->UnkPtr01 != null)
            {
                VarSubDivRelatedUnk->UnkPtr01 = (ushort*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarSubDivRelatedUnk->UnkPtr01, 6 * VarSubDivRelatedUnk->UnkCount01 * 2, true);
            }
            if (VarSubDivRelatedUnk->UnkPtr02 != null)
            {
                VarSubDivRelatedUnk->UnkPtr02 = (ushort*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarSubDivRelatedUnk->UnkPtr02, 16 * VarSubDivRelatedUnk->UnkCount02 * 2, true);
            }
            if (VarSubDivRelatedUnk->UnkPtr03 != null)
            {
                VarSubDivRelatedUnk->UnkPtr03 = (uint*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarSubDivRelatedUnk->UnkPtr03, 4 * VarSubDivRelatedUnk->UnkCount02, true);
            }
            if (VarSubDivRelatedUnk->UnkPtr04 != null)
            {
                VarSubDivRelatedUnk->UnkPtr04 = (uint*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarSubDivRelatedUnk->UnkPtr04, (VarSubDivRelatedUnk->UnkCount06 >> 2) * 4, true);
            }
            if (VarSubDivRelatedUnk->UnkPtr05 != null)
            {
                VarSubDivRelatedUnk->UnkPtr05 = (uint*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarSubDivRelatedUnk->UnkPtr05, 2 * VarSubDivRelatedUnk->UnkCount07 * 4, true);
            }
            if (VarSubDivRelatedUnk->UnkPtr06 != null)
            {
                VarSubDivRelatedUnk->UnkPtr06 = (uint*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarSubDivRelatedUnk->UnkPtr06, (VarSubDivRelatedUnk->UnkCount12 >> 2) * 4, true);
            }
            if (VarSubDivRelatedUnk->UnkPtr07 != null)
            {
                VarSubDivRelatedUnk->UnkPtr07 = (uint*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarSubDivRelatedUnk->UnkPtr07, (VarSubDivRelatedUnk->UnkCount14 >> 2) * 4, true);
            }
            if (VarSubDivRelatedUnk->UnkPtr08 != null)
            {
                VarSubDivRelatedUnk->UnkPtr08 = (uint*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarSubDivRelatedUnk->UnkPtr08, VarSubDivRelatedUnk->UnkCount15 * 4, true);
            }
            if (VarSubDivRelatedUnk->UnkPtr09 != null)
            {
                VarSubDivRelatedUnk->UnkPtr09 = (float*)zone.AllocStreamPosition(3);
                zone.LoadStream(VarSubDivRelatedUnk->UnkPtr09, 8 * VarSubDivRelatedUnk->UnkCount02 * 4, true);
            }
        }
    }
}
