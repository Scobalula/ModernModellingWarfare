// ------------------------------------------------------------------------
// ModernModellingWarfare - Call of Duty Fast File Model Exporter
// Copyright (C) 2021 Philip/Scobalula
// ------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE' file, which is part of this source code package.
// ------------------------------------------------------------------------
using System;

namespace ModernModellingWarfare.Assets
{
    internal unsafe static class ScriptableDefHandler
    {
        /// <summary>
        /// Asset Structure
        /// </summary>
        public struct AssetStruct
        {
            public unsafe sbyte* Name;
            public unsafe fixed byte Buffer[112];

            public unsafe bool IsBlank
            {
                get
                {
                    for (int index = 0; index < 112; ++index)
                    {
                        if (Buffer[index] != 0)
                            return false;
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// A pointer to the Asset Pointer
        /// </summary>
        public static AssetStruct** VarAssetPtr;

        /// <summary>
        /// A pointer to the Asset
        /// </summary>
        public static AssetStruct* VarAsset;

        /// <summary>
        /// Loads the Asset Pointer
        /// </summary>
        public static void LoadAssetPointer(Zone zone, bool atStreamStart)
        {
            var tempIndex = zone.DataBufferIndex;
            zone.LoadStream(VarAssetPtr, IntPtr.Size, atStreamStart);
            zone.DataBufferIndex = 1;

            if (!Zone.IsNullPointer(*VarAssetPtr))
            {
                if ((long)*VarAssetPtr == -1 || (long)*VarAssetPtr == -2 || (long)*VarAssetPtr == -3)
                {
                    VarAsset = (AssetStruct*)zone.AllocStreamPosition(7);
                    LoadAsset(zone, true);
                    *VarAssetPtr = VarAsset;
                }
                else
                {
                    *VarAssetPtr = (AssetStruct*)zone.CalculateStreamPosition(*VarAssetPtr);
                }
            }

            zone.DataBufferIndex = tempIndex;
        }

        /// <summary>
        /// Loads the Asset
        /// </summary>
        public static void LoadAsset(Zone zone, bool atStreamStart)
        {
            zone.LoadStream(VarAsset, sizeof(AssetStruct), true);
            zone.DataBufferIndex = 5;

            if (!VarAsset->IsBlank)
                throw new Exception("Unexpected Asset, expecting temporary Asset");

            if ((long)*VarAssetPtr == -3)
                zone.InsertPadding();

            VarAsset->Name = (long)VarAsset->Name == -1 || (long)VarAsset->Name == -2 || (long)VarAsset->Name == -2 ? zone.LoadXString(0UL, true) : (sbyte*)zone.CalculateStreamPosition(VarAsset->Name);
        }
    }
}
