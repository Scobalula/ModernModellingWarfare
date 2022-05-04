// ------------------------------------------------------------------------
// ModernModellingWarfare - Call of Duty Fast File Model Exporter
// Copyright (C) 2022 Philip/Scobalula
// ------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE' file, which is part of this source code package.
// ------------------------------------------------------------------------
using System;

namespace ModernModellingWarfare.Assets
{
    /// <summary>
    /// A static class to handle Physics Assets
    /// </summary>
    internal unsafe static class XPhysicsAssetHandler
    {
        /// <summary>
        /// Asset Structure
        /// </summary>
        public struct XPhysicsAsset
        {
            public unsafe sbyte* Name;
            public unsafe fixed byte Buffer[80];

            public unsafe bool IsBlank
            {
                get
                {
                    for (int index = 0; index < 80; ++index)
                    {
                        if (Buffer[index] != 0)
                            return false;
                    }
                    return true;
                }
            }
        }

        public static XPhysicsAsset** VarAssetPtr;
        public static XPhysicsAsset* VarAsset;

        public static void LoadAssetPtr(Zone zone, bool atStreamStart)
        {
            var tempIndex = zone.DataBufferIndex;
            zone.LoadStream(VarAssetPtr, IntPtr.Size, atStreamStart);
            zone.DataBufferIndex = 1;

            if (!Zone.IsNullPointer(*VarAssetPtr))
            {
                if ((long)*VarAssetPtr == -1 || (long)*VarAssetPtr == -2 || (long)*VarAssetPtr == -3)
                {
                    VarAsset = (XPhysicsAsset*)zone.AllocStreamPosition(7);
                    LoadMushyAsset(zone, true);
                    *VarAssetPtr = VarAsset;
                }
                else
                {
                    *VarAssetPtr = *(XPhysicsAsset**)zone.CalculateStreamPosition(*VarAssetPtr);
                }
            }

            zone.DataBufferIndex = tempIndex;
        }

        public static void LoadMushyAsset(Zone zone, bool atStreamStart)
        {
            zone.LoadStream(VarAsset, sizeof(XPhysicsAsset), true);
            zone.DataBufferIndex = 5;

            if (!VarAsset->IsBlank)
                throw new Exception("Unexpected Asset, expecting temporary Asset");
            if ((long)*VarAssetPtr == -3)
                zone.InsertPadding();

            VarAsset->Name = (long)VarAsset->Name == -1 || (long)VarAsset->Name == -2 || (long)VarAsset->Name == -2 ? zone.LoadXString(0UL, true) : (sbyte*)zone.CalculateStreamPosition(VarAsset->Name);
        }
    }
}
