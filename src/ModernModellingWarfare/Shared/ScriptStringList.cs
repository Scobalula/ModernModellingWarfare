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
    internal unsafe struct ScriptStringList
    {
        public int Count;
        public sbyte** Strings;

        public string GetByIndex(uint index)
        {
            if (index >= Count)
                throw new Exception("String index was outside bounds of Script String List");

            return new string(Strings[index]);
        }
    };
}
