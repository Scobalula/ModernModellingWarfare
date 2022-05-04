using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D.FileTranslators.CoDXAsset
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    public class TokenDataUVSet : TokenData
    {
        /// <summary>
        /// Gets or Sets the uvs
        /// </summary>
        public List<Vector2> UVs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataUVSet"/> class
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="token">Token</param>
        public TokenDataUVSet(Token token) : base(token)
        {
            UVs = new();
        }
    }
}
