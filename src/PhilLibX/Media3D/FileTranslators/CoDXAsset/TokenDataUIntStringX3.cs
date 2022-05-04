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
    public class TokenDataUIntStringX3 : TokenData
    {
        /// <summary>
        /// Gets or Sets the value
        /// </summary>
        public int IntegerValue { get; set; }

        /// <summary>
        /// Gets or Sets the value
        /// </summary>
        public string StringValue1 { get; set; }

        /// <summary>
        /// Gets or Sets the value
        /// </summary>
        public string StringValue2 { get; set; }

        /// <summary>
        /// Gets or Sets the value
        /// </summary>
        public string StringValue3 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataUIntStringX3"/> class
        /// </summary>
        /// <param name="intVal">Integer Value</param>
        /// <param name="strVal1">String value</param>
        /// <param name="strVal2">String value</param>
        /// <param name="strVal3">String value</param>
        /// <param name="token">Token</param>
        public TokenDataUIntStringX3(int intVal, string strVal1, string strVal2, string strVal3, Token token) : base(token)
        {
            IntegerValue = intVal;
            StringValue1 = strVal1;
            StringValue2 = strVal2;
            StringValue3 = strVal3;
        }
    }
}
