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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CascLib.NET.Utility
{
    /// <summary>
    /// A class to hold a Delimiter Seperated Value File
    /// </summary>
    internal class DSVFile
    {
        /// <summary>
        /// Gets or Sets the Delimiter
        /// </summary>
        public string Delimiter { get; set; }

        /// <summary>
        /// Gets or Sets the Comment Indicator
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets the Rows within the DSV File
        /// </summary>
        public List<List<string>> Rows { get; } = new List<List<string>>();

        /// <summary>
        /// Gets the Header of the DSV File
        /// </summary>
        public List<string> Header => Rows.FirstOrDefault();

        /// <summary>
        /// Initializes a new instance of the DSV File class
        /// </summary>
        public DSVFile()
        {
            Delimiter = ",";
        }

        /// <summary>
        /// Initializes a new instance of the DSV File class with the given Delimiter
        /// </summary>
        /// <param name="delimiter"></param>
        public DSVFile(string delimiter)
        {
            Delimiter = delimiter;
        }

        /// <summary>
        /// Initializes a new instance of the DSV File class with the given Delimiter
        /// </summary>
        /// <param name="delimiter"></param>
        public DSVFile(string file, string delimiter) : this(file, delimiter, null) { }

        /// <summary>
        /// Initializes a new instance of the DSV File class with the given Delimiter
        /// </summary>
        /// <param name="delimiter"></param>
        public DSVFile(string file, string delimiter, string comment)
        {
            Delimiter = delimiter;
            Comment = comment;
            using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            Load(stream);
        }

        /// <summary>
        /// Loads the DSV Data from the provided stream
        /// </summary>
        /// <param name="stream">Stream to load from</param>
        public void Load(Stream stream)
        {
            using var streamReader = new StreamReader(stream);

            string line;

            var supportsCommenting = !string.IsNullOrWhiteSpace(Comment);

            while ((line = streamReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || (supportsCommenting && line.StartsWith(Comment)))
                    continue;

                Rows.Add(line.Split(Delimiter).ToList());
            }
        }
    }
}
