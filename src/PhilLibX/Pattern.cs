using System;
using System.Collections.Generic;
using System.Text;

namespace PhilLibX
{
    /// <summary>
    /// Represents a Pattern with a Needle and a Mask for use in pattern matching and search/find methods
    /// </summary>
    /// <typeparam name="T">Type to search for</typeparam>
    public abstract class Pattern<T>
    {
        /// <summary>
        /// Gets or Sets the Needle
        /// </summary>
        public T[] Needle { get; set; }

        /// <summary>
        /// Gets or Sets the Mask
        /// </summary>
        public T[] Mask { get; set; }
    }
}
