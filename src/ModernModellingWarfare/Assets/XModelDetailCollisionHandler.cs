// ------------------------------------------------------------------------
// ModernModellingWarfare - Call of Duty Fast File Model Exporter
// Copyright (C) 2021 Philip/Scobalula
// ------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE' file, which is part of this source code package.
// ------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace ModernModellingWarfare.Assets
{
    internal unsafe static class XModelDetailCollisionHandler
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct XModelDetailCollision
        {
            public unsafe sbyte*            Name;
            public int                      UnkSize;
            public unsafe byte*             UnkPtr;
            public int                      UnkCount;
            public unsafe uint*             UnkData;
        }

        public static XModelDetailCollision** VarAssetPtr;
        public static XModelDetailCollision* VarAsset;

        public static void LoadAssetPtr(Zone zone, bool atStreamStart)
        {
            var tempIndex = zone.DataBufferIndex;
            zone.LoadStream(VarAssetPtr, IntPtr.Size, atStreamStart);
            zone.DataBufferIndex = 1;

            if (!Zone.IsNullPointer(*VarAssetPtr))
            {
                if ((long)*VarAssetPtr == -1 || (long)*VarAssetPtr == -2 || (long)*VarAssetPtr == -3)
                {
                    VarAsset = (XModelDetailCollision*)zone.AllocStreamPosition(7);
                    LoadXModelDetailCollision(zone, true);
                    *VarAssetPtr = VarAsset;
                }
                else
                {
                    *VarAssetPtr = *(XModelDetailCollision**)zone.CalculateStreamPosition(*VarAssetPtr);
                }
            }

            zone.DataBufferIndex = tempIndex;
        }

        public static void LoadXModelDetailCollision(Zone zone, bool atStreamStart)
        {
            zone.LoadStream(VarAsset, sizeof(XModelDetailCollision), atStreamStart);
            zone.DataBufferIndex = 5;
            zone.PushStartPosition();

            if ((long)*VarAssetPtr == -3)
                zone.InsertPadding();

            VarAsset->Name = (long)VarAsset->Name == -1L || (long)VarAsset->Name == -2L || (long)VarAsset->Name == -3L ? zone.LoadXString(0UL, true) : (sbyte*)zone.CalculateStreamPosition(VarAsset->Name);
            if ((long)VarAsset->UnkPtr != -1L && (long)VarAsset->UnkPtr != -2L && (long)VarAsset->UnkPtr != -3L)
            {
                VarAsset->UnkPtr = zone.CalculateStreamPosition(VarAsset->UnkPtr);
            }
            else
            {
                VarAsset->UnkPtr = zone.AllocStreamPosition(15UL);
                zone.LoadStream(VarAsset->UnkPtr, VarAsset->UnkSize, atStreamStart);
            }
            if ((long)VarAsset->UnkData != -1L && (long)VarAsset->UnkData != -2L && (long)VarAsset->UnkData != -3L)
            {
                VarAsset->UnkData = (uint*)zone.CalculateStreamPosition(VarAsset->UnkData);
            }
            else
            {
                VarAsset->UnkData = (uint*)zone.AllocStreamPosition(3UL);
                zone.LoadStream(VarAsset->UnkData, VarAsset->UnkCount * 4, atStreamStart);
            }
            zone.PopStartPosition();
        }
    }
}
