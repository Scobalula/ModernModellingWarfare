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
// File: CompressionException.cs
// Author: Philip/Scobalula
// Description: The exception that is thrown when a compression error occurs.
// ------------------------------------------------------------------------
using System;

namespace PhilLibX.Compression
{
    /// <summary>
    /// The exception that is thrown when an LZ4 error occurs.
    /// </summary>
    public class CompressionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionException"/> class
        /// </summary>
        public CompressionException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionException"/> class
        /// </summary>
        /// <param name="message">The error that has occured</param>
        public CompressionException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionException"/> class
        /// </summary>
        /// <param name="message">The error that has occured</param>
        /// <param name="inner">The exception that is the cause of the current exception</param>
        public CompressionException(string message, Exception inner) : base(message, inner) { }
    }
}
