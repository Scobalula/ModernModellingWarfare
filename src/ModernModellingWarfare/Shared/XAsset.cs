// ------------------------------------------------------------------------
// ModernModellingWarfare - Call of Duty Fast File Model Exporter
// Copyright (C) 2021 Philip/Scobalula
// ------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE' file, which is part of this source code package.
// ------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ModernModellingWarfare.Shared
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct XAsset
    {
        public uint Type;
        public void* Header;
    };
}
