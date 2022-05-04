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
    public class TokenData
    {
        /// <summary>
        /// Gets the token
        /// </summary>
        public Token Token { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenData"/> class
        /// </summary>
        /// <param name="token">Token</param>
        public TokenData(Token token)
        {
            Token = token;
        }

        public override string ToString()
        {
            return Token.Name;
        }
    }
}
