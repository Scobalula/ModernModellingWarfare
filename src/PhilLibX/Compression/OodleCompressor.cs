// ------------------------------------------------------------------------
// PhilLibX - My Utility Library
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
// File: OodleCompressor.cs
// Author: Philip/Scobalula
// Description: Currently supported Oodle compressors, based off https://github.com/jamesbloom/ozip/blob/master/ozip.cpp and strings in Oodle DLL
// ------------------------------------------------------------------------

namespace PhilLibX.Compression
{
    /// <summary>
    /// Oodle Compressors
    /// </summary>
    public enum OodleCompressor
    {
        LZH       = 0,
        LZHLW     = 1,
        LZNIB     = 2,
        None      = 3,
        LZB16     = 4,
        LZBLW     = 5,
        LZA       = 6,
        LZNA      = 7,
        Kraken    = 8,
        Mermaid   = 9,
        BitKnit   = 10,
        Selkie    = 11,
        Hydra     = 12,
        Leviathan = 13,
        Invalid   = -1,
    }
}
