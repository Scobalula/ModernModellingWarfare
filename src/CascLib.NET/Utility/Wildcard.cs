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
using System.Text;
using System.Text.RegularExpressions;

namespace CascLib.NET.Utility
{
    /// <summary>
    /// A class for dealing with Wildcards
    /// </summary>
    internal static class Wildcard
    {
        /// <summary>
        /// Converts a wildcard string to <see cref="Regex"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Regex ToRegex(string input)
        {
            // From fnmatch.py from Python
            var result = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '*')
                {
                    result.Append(".*");
                }
                else if (input[i] == '?')
                {
                    result.Append('.');
                }
                //else if (input[i] == '[')
                //{
                //    result.Append('.');
                //}
                else if (char.IsLetterOrDigit(input[i]))
                {
                    result.Append(input[i]);
                }
            }

            result.Append("\\Z(?ms)");

            return new Regex(result.ToString());
        }
    }
}
