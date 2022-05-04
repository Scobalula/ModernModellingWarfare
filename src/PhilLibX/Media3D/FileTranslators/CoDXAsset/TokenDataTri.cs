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
    public class TokenDataTri : TokenData
    {
        /// <summary>
        /// Gets or Sets the index
        /// </summary>
        public int A { get; set; }

        /// <summary>
        /// Gets or Sets the weight
        /// </summary>
        public int B { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataTri"/> class
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="weight">Weight</param>
        public TokenDataTri(int a, int b, Token token) : base(token)
        {
            A = a;
            B = b;
        }
    }
}
