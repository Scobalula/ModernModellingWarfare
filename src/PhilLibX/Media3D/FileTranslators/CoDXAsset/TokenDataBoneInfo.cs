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
    public class TokenDataBoneInfo : TokenData
    {
        /// <summary>
        /// Gets or Sets the name of the bone
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the index
        /// </summary>
        public int BoneIndex { get; set; }

        /// <summary>
        /// Gets or Sets the parent index
        /// </summary>
        public int BoneParentIndex { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataBoneInfo"/> class
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="parentIndex">Parent index</param>
        /// <param name="name">Bone name</param>
        /// <param name="token">Token</param>
        public TokenDataBoneInfo(int index, int parentIndex, string name, Token token) : base(token)
        {
            BoneIndex = index;
            BoneParentIndex = parentIndex;
            Name = name;
        }
    }
}
