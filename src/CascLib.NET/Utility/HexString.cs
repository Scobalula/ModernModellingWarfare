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
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CascLib.NET.Utility
{
    /// <summary>
    /// A class for handling Hex Strings
    /// </summary>
    internal static class HexString
    {
        /// <summary>
        /// Converts the hex string to a byte array
        /// </summary>
        /// <param name="hexString">Hex string to convert</param>
        /// <returns>Result as raw bytes</returns>
        public static byte[] ToByteArray(string hexString)
        {
            // 2 characters per byte
            Span<char> buffer = stackalloc char[2];
            // We're going to need at least the size of this input string
            var result = new List<byte>(hexString.Length);

            for (int i = 0, j = 0; i < 2 && j < hexString.Length; j++)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(hexString[j]))
                    continue;

                buffer[i++] = hexString[j];

                if (i == 2)
                {
                    i = 0;

                    if (byte.TryParse(buffer, NumberStyles.HexNumber, null, out byte b))
                    {
                        result.Add(b);
                    }
                }
            }

            return result.ToArray();
        }
    }
}
