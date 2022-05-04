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
    public class TokenDataVector2 : TokenData
    {
        /// <summary>
        /// Gets or Sets the data
        /// </summary>
        public Vector2 Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataVector2"/> class
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="token">Token</param>
        public TokenDataVector2(Vector2 data, Token token) : base(token)
        {
            Data = data;
        }
    }
}
