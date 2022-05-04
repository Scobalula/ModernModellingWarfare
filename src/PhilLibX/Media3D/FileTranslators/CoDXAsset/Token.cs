using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D.FileTranslators.CoDXAsset
{
    /// <summary>
    /// A class to hold a token
    /// </summary>
    public record Token
    {
        /// <summary>
        /// Gets the name of the block
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the data type
        /// </summary>
        public TokenDataType DataType { get; private set; }

        /// <summary>
        /// Gets the block hash (CRC16 with data type as init)
        /// </summary>
        public uint Hash { get; private set; }

        public Token(string name, TokenDataType dataType, uint hash)
        {
            Name     = name;
            DataType = dataType;
            Hash     = hash;
        }

        #region Tokens
        /// <summary>
        /// Gets supported tokens
        /// </summary>
        public static readonly Token[] Tokens =
        {
            new(";",                TokenDataType.Comment,          0x8738),
            new("//",               TokenDataType.Comment,          0xC355),
            new("AMBIENTCOLOR",     TokenDataType.Vector4,          0x37FF),
            new("ANIMATION",        TokenDataType.Section,          0x7AAC),
            new("BLINN",            TokenDataType.Vector2,          0x83C7),
            new("BONE",             TokenDataType.UShort,           0xDD9A),
            new("BONE",             TokenDataType.BoneWeight,       0xF1AB),
            new("BONE",             TokenDataType.BoneInfo,         0xF099),
            new("BONES",            TokenDataType.UShort,           0xEA46),
            new("COEFFS",           TokenDataType.Vector2,          0xC835),
            new("COLOR",            TokenDataType.Vector48Bit,      0x6DD8),
            new("FIRSTFRAME",       TokenDataType.UShort,           0xBCD4),
            new("FRAME",            TokenDataType.UInt,             0xC723),
            new("FRAME",            TokenDataType.Unk4,             0x1675),
            new("FRAMERATE",        TokenDataType.UShort,           0x92D3),
            new("GLOW",             TokenDataType.Vector2,          0xFE0C),
            new("INCANDESCENCE",    TokenDataType.Vector4,          0x4265),
            new("MATERIAL",         TokenDataType.UShortStringX3,   0xA700),
            new("MODEL",            TokenDataType.Section,          0x46C8),
            new("NORMAL",           TokenDataType.Vector316Bit,     0x89EC),
            new("NOTETRACK",        TokenDataType.UShort,           0x4643),
            new("NOTETRACKS",       TokenDataType.Section,          0xC7F3),
            new("NUMBONES",         TokenDataType.UShort,           0x76BA),
            new("NUMFACES",         TokenDataType.Int,              0xBE92),
            new("NUMFRAMES",        TokenDataType.Int,              0xB917),
            new("NUMKEYS",          TokenDataType.UShort,           0x7A6C),
            new("NUMMATERIALS",     TokenDataType.UShort,           0xA1B2),
            new("NUMOBJECTS",       TokenDataType.UShort,           0x62AF),
            new("NUMPARTS",         TokenDataType.UShort,           0x9279),
            new("NUMTRACKS",        TokenDataType.UShort,           0x9016),
            new("NUMVERTS",         TokenDataType.UShort,           0x950D),
            new("NUMVERTS32",       TokenDataType.Int,              0x2AEC),
            new("OBJECT",           TokenDataType.UShortString,     0x87D4),
            new("OFFSET",           TokenDataType.Vector3,          0x9383),
            new("PART",             TokenDataType.UShort,           0x745A),
            new("PART",             TokenDataType.UShortString,     0x360B),
            new("PHONG",            TokenDataType.Float,            0x5CD2),
            new("REFLECTIVE",       TokenDataType.Vector2,          0x7D76),
            new("REFLECTIVECOLOR",  TokenDataType.Vector4,          0xE593),
            new("REFRACTIVE",       TokenDataType.Vector2,          0x7E24),
            new("SCALE",            TokenDataType.Vector3,          0x1C56),
            new("SPECULARCOLOR",    TokenDataType.Vector4,          0x317C),
            new("TRANSPARENCY",     TokenDataType.Vector4,          0x6DAB),
            new("TRI",              TokenDataType.Tri,              0x562F),
            new("TRI16",            TokenDataType.Tri16,            0x6711),
            new("UV",               TokenDataType.UVSet,            0x1AD4),
            new("VERSION",          TokenDataType.UShort,           0x24D1),
            new("VERT",             TokenDataType.UShort,           0x8F03),
            new("VERT32",           TokenDataType.Int,              0xB097),
            new("X",                TokenDataType.Vector316Bit,     0xDCFD),
            new("Y",                TokenDataType.Vector316Bit,     0xCCDC),
            new("Z",                TokenDataType.Vector316Bit,     0xFCBF),
            new("NUMSBONES",        TokenDataType.Int,              0x1FC2),
            new("NUMSWEIGHTS",      TokenDataType.Int,              0xB35E),
            new("QUATERNION",       TokenDataType.Vector4,          0xEF69),
            new("NUMIKPITCHLAYERS", TokenDataType.Int,              0xA65B),
            new("IKPITCHLAYER",     TokenDataType.UInt,             0x1D7D),
            new("ROTATION",         TokenDataType.Vector3,          0xA58B),
            new("NUMCOSMETICBONES", TokenDataType.Int,              0x7836),
            new("EXTRA",            TokenDataType.Vector4,          0x6EEE),
        };
        #endregion

        internal static bool TryGetToken(uint hash, out Token result)
        {
            foreach (var token in Tokens)
            {
                if (token.Hash == hash)
                {
                    result = token;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}
