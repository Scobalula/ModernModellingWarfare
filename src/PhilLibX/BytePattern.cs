using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PhilLibX
{
    /// <summary>
    /// A class to hold a byte pattern
    /// </summary>
    public class BytePattern : Pattern<byte>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BytePattern"/> class
        /// </summary>
        public BytePattern() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BytePattern"/> class with a size
        /// </summary>
        /// <param name="needle">Needle Pattern</param>
        public BytePattern(int size)
        {
            Needle = new byte[size];
            Mask = new byte[size];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BytePattern"/> class with a hex string
        /// </summary>
        /// <param name="hexString">Hex String with masks, for example: "1A FF ?? ?? 00"</param>
        public BytePattern(string hexString)
        {
            ParseString(hexString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BytePattern"/> class with a needle and mask
        /// </summary>
        /// <param name="needle">Needle Pattern</param>
        /// <param name="mask">Mask</param>
        public BytePattern(byte[] needle, byte[] mask)
        {
            if (needle.Length != mask.Length)
                throw new ArgumentException("Needle and Mask must be of the same size");

            Needle = needle;
            Mask = mask;
        }

        /// <summary>
        /// Parses the pattern from a hex string
        /// </summary>
        /// <param name="hexString">Hex String with masks, for example: "1A FF ?? ?? 00"</param>
        public void ParseString(string hexString)
        {
            // 2 characters per byte
            Span<char> buffer = stackalloc char[2];
            // We're going to need at least the size of this input string
            var pattern = new List<byte>(hexString.Length);
            var mask = new List<byte>(hexString.Length);

            for (int i = 0, j = 0; i < 2 && j < hexString.Length; j++)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(hexString[j]))
                    continue;
                buffer[i++] = hexString[j];

                if (i == 2)
                {
                    i = 0;

                    // Check if unknown vs hex
                    if (buffer[0] == '?' || buffer[1] == '?')
                    {
                        pattern.Add(0);
                        mask.Add(0xFF);
                    }
                    else if (byte.TryParse(buffer, NumberStyles.HexNumber, null, out byte b))
                    {
                        pattern.Add(b);
                        mask.Add(0);
                    }
                }
            }

            Needle = pattern.ToArray();
            Mask = mask.ToArray();
        }
    }
}
