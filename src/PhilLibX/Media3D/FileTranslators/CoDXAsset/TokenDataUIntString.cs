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
    public class TokenDataUIntString : TokenData
    {
        /// <summary>
        /// Gets or Sets the value
        /// </summary>
        public uint IntegerValue { get; set; }

        /// <summary>
        /// Gets or Sets the value
        /// </summary>
        public string StringValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataUIntString"/> class
        /// </summary>
        /// <param name="intVal">Integer Value</param>
        /// <param name="strVal">String value</param>
        /// <param name="token">Token</param>
        public TokenDataUIntString(uint intVal, string strVal, Token token) : base(token)
        {
            IntegerValue = intVal;
            StringValue = strVal;
        }
    }
}
