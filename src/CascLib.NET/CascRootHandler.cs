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
using System.IO;

namespace CascLib.NET
{
    public class CascRootHandler
    {
        /// <summary>
        /// A class to hold a Root Entry
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// A class to hold Span Info
            /// </summary>
            public class SpanInfo
            {
                /// <summary>
                /// Gets the Content Key
                /// </summary>
                public byte[] ContentKey { get; private set; }

                /// <summary>
                /// Gets the Encoding Key
                /// </summary>
                public byte[] EncodingKey { get; private set; }

                /// <summary>
                /// Gets the Size of this Span
                /// </summary>
                public int Size { get; set; }

                /// <summary>
                /// Gets the Content Key as Base 64 string
                /// </summary>
                public string Base64ContentKey { get; private set; }

                /// <summary>
                /// Gets the Encoding Key as Base 64 string
                /// </summary>
                public string Base64EncodingKey { get; private set; }

                public SpanInfo(byte[] eKey)
                {
                    EncodingKey = eKey;

                    Base64EncodingKey = Convert.ToBase64String(EncodingKey);
                }

                public SpanInfo(byte[] cKey, byte[] eKey, int size)
                {
                    ContentKey = cKey;

                    Base64ContentKey = Convert.ToBase64String(ContentKey);
                }
            }

            /// <summary>
            /// Gets or Sets the Entry Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or Sets the Spans
            /// </summary>
            public List<SpanInfo> Spans { get; } = new List<SpanInfo>();

            public Entry(string name)
            {
                Name = name;
            }

            public Entry(string name, List<SpanInfo> spans)
            {
                Name = name;
            }
        }

        /// <summary>
        /// Gets the File Entries
        /// </summary>
        public Dictionary<string, Entry> FileEntries { get; } = new Dictionary<string, Entry>();

        /// <summary>
        /// Parses the data from the Casc Root File
        /// </summary>
        /// <param name="stream">Stream to load</param>
        public virtual void Parse(Stream stream) { }
    }
}
