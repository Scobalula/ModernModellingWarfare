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
using CascLib.NET.Utility;
using System;
using System.Collections.Generic;

namespace CascLib.NET
{
    /// <summary>
    /// A class to hold Casc Build Info
    /// </summary>
    public class CascBuildInfo
    {
        /// <summary>
        /// A class to handle parsing and storing a Build File Variable
        /// </summary>
        public class Variable
        {
            /// <summary>
            /// Gets the name of the Variable
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the Variable Data Type
            /// </summary>
            public string Type { get; private set; }

            /// <summary>
            /// Gets the Value assigned to this variable
            /// </summary>
            public string Value { get; private set; }

            /// <summary>
            /// Initializes a new instance of a Config Variable
            /// </summary>
            /// <param name="name">Name of this Variable</param>
            /// <param name="type">Variable Data Type</param>
            /// <param name="value">Value assigned to this variable</param>
            public Variable(string name, string type, string value)
            {
                Name = name;
                Type = type;
                Value = value;
            }
        }

        /// <summary>
        /// Gets the Variables stored in this config
        /// </summary>
        private Dictionary<string, Variable> Variables { get; } = new Dictionary<string, Variable>();

        public string Get(string varName, string defaultValue)
        {
            if(Variables.TryGetValue(varName, out var value))
            {
                return value.Value;
            }
            else
            {
                return defaultValue;
            }
        }

        public CascBuildInfo() { }
        public CascBuildInfo(string fileName)
        {
        }

        public void Load(string fileName)
        {
            var dsv = new DSVFile(fileName, "|", "#");

            if (dsv.Rows.Count < 2)
                throw new Exception();

            var header = dsv.Rows[0];
            var data = dsv.Rows[1];

            if (header.Count != data.Count)
                throw new Exception();
            
            for(int i = 0; i < data.Count; i++)
            {
                var info = header[i];

                var split = info.Split("!");

                Variables[split[0]] = new Variable(split[0], split[1], data[i]);
            }
        }
    }
}
