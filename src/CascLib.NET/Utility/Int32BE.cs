// ------------------------------------------------------------------------
// CascLib.NET - A pure C# Casc Storage Handler
// Copyright(c) 2021 Philip/Scobalula
// ------------------------------------------------------------------------
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// ------------------------------------------------------------------------
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ------------------------------------------------------------------------
using System.Runtime.InteropServices;

namespace CascLib.NET.Utility
{
    /// <summary>
    /// A struct to represent a Big Endian 32bit Int
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct Int32BE
    {
        [FieldOffset(0)]
        private readonly byte B0;
        [FieldOffset(1)]
        private readonly byte B1;
        [FieldOffset(2)]
        private readonly byte B2;
        [FieldOffset(3)]
        private readonly byte B3;

        public static implicit operator int(Int32BE d) => (d.B0 << 24) | (d.B1 << 16) | (d.B2 << 8) | d.B3;

        public override string ToString()
        {
            return ((int)this).ToString();
        }
    }
}
