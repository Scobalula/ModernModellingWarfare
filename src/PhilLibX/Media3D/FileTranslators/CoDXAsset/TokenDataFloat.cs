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
    public class TokenDataFloat : TokenData
    {
        /// <summary>
        /// Gets or Sets the data
        /// </summary>
        public float Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataFloat"/> class
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="token">Token</param>
        public TokenDataFloat(float data, Token token) : base(token)
        {
            Data = data;
        }
    }
}
