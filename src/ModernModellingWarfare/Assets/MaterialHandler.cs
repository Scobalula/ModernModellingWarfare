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
    /// <summary>
    /// A static class to handle Material Assets
    /// </summary>
    internal unsafe static class MaterialHandler
    {
        /// <summary>
        /// Asset Structure
        /// </summary>
        public struct Material
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
        public static Material** VarAssetPtr;

        /// <summary>
        /// A pointer to the Asset
        /// </summary>
        public static Material* VarAsset;

        /// <summary>
        /// Loads the Asset Pointer
        /// </summary>
        public static void LoadAssetPtr(Zone zone, bool atStreamStart)
        {
            var tempIndex = zone.DataBufferIndex;
            zone.LoadStream(VarAssetPtr, IntPtr.Size, atStreamStart);
            zone.DataBufferIndex = 1;

            if (!Zone.IsNullPointer(*VarAssetPtr))
            {
                if ((long)*VarAssetPtr == -1 || (long)*VarAssetPtr == -2 || (long)*VarAssetPtr == -3)
                {
                    VarAsset = (Material*)zone.AllocStreamPosition(7);
                    LoadAsset(zone, true);
                    *VarAssetPtr = VarAsset;
                }
                else
                {
                    *VarAssetPtr = *(Material**)zone.CalculateStreamPosition(*VarAssetPtr);
                }
            }

            zone.DataBufferIndex = tempIndex;
        }

        /// <summary>
        /// Loads the Asset
        /// </summary>
        public static void LoadAsset(Zone zone, bool atStreamStart)
        {
            // We only have this here as Fast Files storing weapons contain references to these
            // We don't really care about their layout as they should be blank
            zone.LoadStream(VarAsset, sizeof(Material), true);

            zone.DataBufferIndex = 5;

            if (!VarAsset->IsBlank)
                throw new Exception("Unexpected Material Asset");

            if ((long)*VarAssetPtr == -3)
                zone.InsertPadding();

            VarAsset->Name = (long)VarAsset->Name == -1 || (long)VarAsset->Name == -2 || (long)VarAsset->Name == -2 ? zone.LoadXString(0UL, true) : (sbyte*)zone.CalculateStreamPosition(VarAsset->Name);

            Global.DebugPrint($"Loading Material: {new string(VarAsset->Name)}");
        }
    }
}
