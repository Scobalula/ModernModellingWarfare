using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// An enum to define Animation Transform Space
    /// </summary>
    public enum AnimationTransformSpace
    {
        /// <summary>
        /// Animation Data is same as it parent bones/animation
        /// </summary>
        Parent,

        /// <summary>
        /// Animation Data is relative to parent
        /// </summary>
        Local,

        /// <summary>
        /// Animation Data is relative to world origin
        /// </summary>
        World,
    }
}
