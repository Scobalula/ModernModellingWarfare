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
using System.Text;

namespace CascLib.NET
{
    /// <summary>
    /// 
    /// </summary>
    public class CascConfig
    {
        /// <summary>
        /// A class to handle parsing and storing a Config File Variable
        /// </summary>
        public class Variable
        {
            /// <summary>
            /// Gets the name of the Variable
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets the Values assigned to this variable
            /// </summary>
            public string[] Values { get; private set; }

            /// <summary>
            /// Initializes a new instance of a Config Variable
            /// </summary>
            /// <param name="name">Name of this Variable</param>
            /// <param name="values">Values assigned to this variable</param>
            public Variable(string name, string[] values)
            {
                Name = name;
                Values = values;
            }
        }

        public CascConfig()
        {

        }

        /// <summary>
        /// Gets the Variables stored in this config
        /// </summary>
        private Dictionary<string, Variable> Variables { get; } = new Dictionary<string, Variable>();

        public Variable Get(string varName)
        {
            if (Variables.TryGetValue(varName, out var value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Loads the DSV Data from the provided stream
        /// </summary>
        /// <param name="stream">Stream to load from</param>
        public void Load(string fileName)
        {
            using var streamReader = new StreamReader(File.OpenRead(fileName));

            string line;

            while ((line = streamReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var lineSplit = line.Split("=", StringSplitOptions.RemoveEmptyEntries);

                var variable = new Variable(lineSplit[0].Trim(), lineSplit[1].Split(' ', StringSplitOptions.RemoveEmptyEntries));

                Variables[variable.Name] = variable;
            }
        }
    }
}
