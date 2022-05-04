using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    public class AnimationBoneSamplerLayer
    {
        /// <summary>
        /// Gets or Sets the main layer
        /// </summary>
        public AnimationSamplerLayer Layer { get; set; }

        /// <summary>
        /// Gets the underlying animation bone
        /// </summary>
        public AnimationBone AnimationBone { get; private set; }

        /// <summary>
        /// Gets or Sets the transform type
        /// </summary>
        public AnimationTransformType TransformType { get; set; }

        /// <summary>
        /// Gets or Sets the Translations Cursor
        /// </summary>
        public int CurrentTranslationsCursor { get; set; }

        /// <summary>
        /// Gets or Sets the Rotations Cursor
        /// </summary>
        public int CurrentRotationsCursor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animBone"></param>
        public AnimationBoneSamplerLayer(AnimationSamplerLayer layer, AnimationBone animBone, AnimationTransformType transformType)
        {
            Layer = layer;
            AnimationBone = animBone;
            TransformType = transformType;
        }
    }
}
