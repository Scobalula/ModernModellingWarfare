using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// An enum to define Animation Transform Type
    /// </summary>
    public enum AnimationTransformType
    {
        /// <summary>
        /// Animation Data is same as it parent bones/animation
        /// </summary>
        Parent,

        /// <summary>
        /// Animation Data is relative to zero
        /// </summary>
        Absolute,

        /// <summary>
        /// Animation Data is relative to parent bind pose
        /// </summary>
        Relative,

        /// <summary>
        /// Animation Data is applied to existing animation data in the scene
        /// </summary>
        Additive,
    }
}
