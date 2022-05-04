using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D.FileTranslators.CoDXAsset
{
    public sealed class ExportTokenReader : TokenReader
    {
        /// <summary>
        /// Gets the Reader
        /// </summary>
        internal StreamReader Reader { get; set; }

        /// <summary>
        /// Gets or Sets the token builder
        /// </summary>
        internal StringBuilder TokenBuilder { get; set; }

        /// <summary>
        /// Gets or Sets the current token list
        /// </summary>
        internal List<string> TokenList { get; set; }

        public ExportTokenReader(string fileName)
        {
            TokenBuilder = new(256);
            TokenList = new(8);
            Reader = new(fileName);
        }

        public void ConsumeTokens()
        {
            TokenList.Clear();

            int c;

            while (true)
            {
                //if (TokenList.FindIndex(x => x == "BONE") != -1)
                //    Debugger.Break();
                c = Reader.Read();

                // First pass for 
                while (true)
                {
                    if (c == -1 || c != ',' && !char.IsWhiteSpace((char)c) && c != '\n' && c != '\r')
                        break;

                    c = Reader.Read();
                }

                TokenBuilder.Clear();

                if(c == '"')
                {
                    while (true)
                    {
                        c = Reader.Read();

                        if (c == '"')
                            break;
                        if (c == -1)
                            throw new IOException("Unexpected EOF while parsing string literal in export file.");
                        if(c == '\n' || c == '\r')
                            throw new IOException("Unexpected EOL while parsing string literal in export file.");

                        TokenBuilder.Append((char)c);
                    }

                    c = Reader.Peek();
                }
                else
                {
                    while (true)
                    {
                        if (c == -1 || c == ',' || char.IsWhiteSpace((char)c) || c == '\n' || c == '\r')
                            break;

                        TokenBuilder.Append((char)c);
                        c = Reader.Read();
                    }
                }



                TokenList.Add(TokenBuilder.ToString());

                if (c == '\n' || c == '\r')
                    break;
            }
        }

        /// <summary>
        /// Requests the next token from the stream
        /// </summary>
        /// <returns></returns>
        public override TokenData RequestNextToken()
        {
            while (true)
            {
                // No tokens left
                if (Reader.BaseStream.Position >= Reader.BaseStream.Length)
                    return null;

                ConsumeTokens();

                if (TokenList.Count == 0)
                    break;

                if (!TryGetToken(TokenList[0], TokenList, out var token))
                    throw new IOException("Unrecognized Token");

                switch (token.DataType)
                {
                    case TokenDataType.Section:
                        {
                            return new TokenData(token);
                        }
                    case TokenDataType.BoneInfo:
                        {
                            return new TokenDataBoneInfo(
                                int.Parse(TokenList[1]),
                                int.Parse(TokenList[2]),
                                TokenList[3], token);
                        }
                    //case TokenDataType.Short:
                    //    {
                    //    }
                    case TokenDataType.UShortString:
                        {
                            return new TokenDataUIntString(
                                ushort.Parse(TokenList[1]),
                                TokenList[2], token);
                        }
                    case TokenDataType.UShortStringX3:
                        {
                            return new TokenDataUIntStringX3(
                                ushort.Parse(TokenList[1]),
                                TokenList[2],
                                TokenList[3],
                                TokenList[4], token);
                        }
                    case TokenDataType.UShort:
                        {
                            return new TokenDataUInt(
                                ushort.Parse(TokenList[1]), token);
                        }
                    case TokenDataType.Int:
                        {
                            return new TokenDataInt(
                                int.Parse(TokenList[1]), token);
                        }
                    case TokenDataType.UInt:
                        {
                            return new TokenDataUInt(
                                uint.Parse(TokenList[1]), token);
                        }
                    case TokenDataType.Float:
                        {
                            return new TokenDataFloat(
                                float.Parse(TokenList[1]), token);
                        }
                    case TokenDataType.Vector2:
                        {
                            return new TokenDataVector2(new(
                                float.Parse(TokenList[1]),
                                float.Parse(TokenList[2])), token);
                        }
                    case TokenDataType.Vector3:
                    case TokenDataType.Vector316Bit:
                        {
                            return new TokenDataVector3(new(
                                float.Parse(TokenList[1]),
                                float.Parse(TokenList[2]),
                                float.Parse(TokenList[3])), token);
                        }
                    case TokenDataType.Vector4:
                    case TokenDataType.Vector48Bit:
                        {
                            return new TokenDataVector4(new(
                                float.Parse(TokenList[1]),
                                float.Parse(TokenList[2]),
                                float.Parse(TokenList[3]),
                                float.Parse(TokenList[4])), token);
                        }
                    case TokenDataType.BoneWeight:
                        {
                            return new TokenDataBoneWeight(
                                ushort.Parse(TokenList[1]),
                                float.Parse(TokenList[1]), token);
                        }
                    case TokenDataType.Tri:
                    case TokenDataType.Tri16:
                        {
                            return new TokenDataTri(
                                int.Parse(TokenList[1]),
                                int.Parse(TokenList[2]), token);
                        }
                    case TokenDataType.UVSet:
                        {
                            var result = new TokenDataUVSet(token);
                            var uvSets = ushort.Parse(TokenList[1]);
                            for (int i = 0; i < uvSets; i++)
                                result.UVs.Add(new(
                                    float.Parse(TokenList[2 + (i * 2) + 0]),
                                    float.Parse(TokenList[2 + (i * 2) + 1])));
                            return result;
                        }
                }

                throw new IOException($"Token Data Type: {token.DataType} for Token: {token.Name} @ {Reader.BaseStream.Position}");
            }

            return null;
        }

        internal static bool TryGetToken(string name, List<string> tokens, out Token result)
        {
            foreach (var token in Token.Tokens)
            {
                if (token.Name == name)
                {
                    // This is sort of how export2bin handles it from what I can see
                    // only I think it uses index of token/type but this does the job too
                    switch(token.Hash)
                    {
                        // Bone Info
                        case 0xF099:
                            if (tokens.Count != 4)
                                continue;
                            break;
                        // Bone Weights
                        case 0xF1AB:
                            if (tokens.Count != 3)
                                continue;
                            break;
                        // Bone Index
                        case 0xDD9A:
                            if (tokens.Count != 2)
                                continue;
                            break;
                    }

                    result = token;
                    return true;
                }
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Finalizes the write, performing any necessary compression, flushing, etc.
        /// </summary>
        public override void FinalizeRead()
        {

        }

        /// <summary>
        /// Disposes of the data
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Reader != null)
                {
                    Reader.Dispose();
                    Reader = null;
                }
            }
        }

        public override void Reset()
        {
            Reader.BaseStream.Position = 0;
        }
    }
}
