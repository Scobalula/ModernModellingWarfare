using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D.FileTranslators.CoDXAsset
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    public class TokenDataBoneWeight : TokenData
    {
        /// <summary>
        /// Gets or Sets the index
        /// </summary>
        public int BoneIndex { get; set; }

        /// <summary>
        /// Gets or Sets the weight
        /// </summary>
        public float BoneWeight { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataBoneWeight"/> class
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="weight">Weight</param>
        public TokenDataBoneWeight(int index, float weight, Token token) : base(token)
        {
            BoneIndex = index;
            BoneWeight = weight;
        }
    }
}
