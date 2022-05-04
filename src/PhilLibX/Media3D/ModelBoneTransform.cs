using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold a Model Bone Transform
    /// </summary>
    public class ModelBoneTransform
    {
        /// <summary>
        /// Gets or Sets the bone position
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or Sets the bone rotation
        /// </summary>
        public Quaternion Rotation { get; set; }
    }
}
