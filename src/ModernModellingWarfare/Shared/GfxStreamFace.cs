// ------------------------------------------------------------------------
// ModernModellingWarfare - Call of Duty Fast File Model Exporter
// Copyright (C) 2021 Philip/Scobalula
// ------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE' file, which is part of this source code package.
// ------------------------------------------------------------------------
using System.Runtime.InteropServices;

namespace ModernModellingWarfare.Shared
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GfxStreamFace
    {
        public ushort Index0;
        public ushort Index1;
        public ushort Index2;
    };
}
