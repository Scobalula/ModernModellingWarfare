// ------------------------------------------------------------------------
// ModernModellingWarfare - Call of Duty Fast File Model Exporter
// Copyright (C) 2021 Philip/Scobalula
// ------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE' file, which is part of this source code package.
// ------------------------------------------------------------------------
using System.Numerics;
using System.Runtime.InteropServices;

namespace ModernModellingWarfare.Shared
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct XQuat
    {
        public short X;
        public short Y;
        public short H;
        public short W;

        public Quaternion Unpacked => new(
            X / (float)short.MaxValue,
            Y / (float)short.MaxValue,
            H / (float)short.MaxValue,
            W / (float)short.MaxValue);
    }
}
