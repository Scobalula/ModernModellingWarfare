using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D.FileTranslators.CoDXAsset
{
    public abstract class TokenWriter : IDisposable
    {
        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        public abstract void WriteSection(string name, uint hash);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteComment(string name, uint hash, string value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="boneIndex">Bone index</param>
        /// <param name="parentIndex">Bone parent</param>
        /// <param name="boneName">Bone name</param>
        public abstract void WriteBoneInfo(string name, uint hash, int boneIndex, int parentIndex, string boneName);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteFloat(string name, uint hash, float value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteInt(string name, uint hash, int value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteUInt(string name, uint hash, uint value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteShort(string name, uint hash, short value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteUShort(string name, uint hash, ushort value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteVector2(string name, uint hash, Vector2 value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteVector3(string name, uint hash, Vector3 value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteVector316Bit(string name, uint hash, Vector3 value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteVector4(string name, uint hash, Vector4 value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteVector48Bit(string name, uint hash, Vector4 value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="boneIndex">Bone index</param>
        /// <param name="boneWeight">Bone weight</param>
        public abstract void WriteBoneWeight(string name, uint hash, int boneIndex, float boneWeight);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="objectIndex">Object index</param>
        /// <param name="materialIndex">Material index</param>
        public abstract void WriteTri(string name, uint hash, int objectIndex, int materialIndex);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="objectIndex">Object index</param>
        /// <param name="materialIndex">Material index</param>
        public abstract void WriteTri16(string name, uint hash, int objectIndex, int materialIndex);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public abstract void WriteUVSet(string name, uint hash, Vector2 value);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="count">UV Count</param>
        /// <param name="values">Token values</param>
        public abstract void WriteUVSet(string name, uint hash, int count, IEnumerable<Vector2> values);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="intVal">Token value</param>
        /// <param name="strVal">Token value</param>
        public abstract void WriteUShortString(string name, uint hash, ushort intVal, string strVal);

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="intVal">Token value</param>
        /// <param name="strVal1">Token value</param>
        /// <param name="strVal2">Token value</param>
        /// <param name="strVal3">Token value</param>
        public abstract void WriteUShortStringX3(string name, uint hash, ushort intVal, string strVal1, string strVal2, string strVal3);

        /// <summary>
        /// Requests the next token from the stream
        /// </summary>
        /// <returns></returns>
        public virtual void WriteToken(TokenData data)
        {
            switch (data.Token.DataType)
            {
                //case TokenDataType.Comment:
                //    {
                //        WriteComment(
                //            data.Token.Name,
                //            data.Token.Hash);
                //        break;
                //    }
                case TokenDataType.Section:
                    {
                        WriteSection(data.Token.Name, data.Token.Hash);
                        break;
                    }
                case TokenDataType.BoneInfo:
                    {
                        var castedData = (TokenDataBoneInfo)data;
                        WriteBoneInfo(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.BoneIndex,
                            castedData.BoneParentIndex,
                            castedData.Name);
                        break;
                    }
                case TokenDataType.Short:
                    {
                        var castedData = (TokenDataInt)data;
                        WriteShort(
                            data.Token.Name,
                            data.Token.Hash,
                            (short)castedData.Data);
                        break;
                    }
                case TokenDataType.UShortString:
                    {
                        var castedData = (TokenDataUIntString)data;
                        WriteUShortString(
                            data.Token.Name,
                            data.Token.Hash,
                            (ushort)castedData.IntegerValue,
                            castedData.StringValue);
                        break;
                    }
                case TokenDataType.UShortStringX3:
                    {
                        var castedData = (TokenDataUIntStringX3)data;
                        WriteUShortStringX3(
                            data.Token.Name,
                            data.Token.Hash,
                            (ushort)castedData.IntegerValue,
                            castedData.StringValue1,
                            castedData.StringValue2,
                            castedData.StringValue3);
                        break;
                    }
                case TokenDataType.UShort:
                    {
                        var castedData = (TokenDataUInt)data;
                        WriteUShort(
                            data.Token.Name,
                            data.Token.Hash,
                            (ushort)castedData.Data);
                        break;
                    }
                case TokenDataType.Int:
                    {
                        var castedData = (TokenDataInt)data;
                        WriteInt(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.Data);
                        break;
                    }
                case TokenDataType.UInt:
                    {
                        var castedData = (TokenDataUInt)data;
                        WriteUInt(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.Data);
                        break;
                    }
                case TokenDataType.Float:
                    {
                        var castedData = (TokenDataFloat)data;
                        WriteFloat(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.Data);
                        break;
                    }
                case TokenDataType.Vector2:
                    {
                        var castedData = (TokenDataVector2)data;
                        WriteVector2(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.Data);
                        break;
                    }
                case TokenDataType.Vector3:
                    {
                        var castedData = (TokenDataVector3)data;
                        WriteVector3(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.Data);
                        break;
                    }
                case TokenDataType.Vector316Bit:
                    {
                        var castedData = (TokenDataVector3)data;
                        WriteVector316Bit(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.Data);
                        break;
                    }
                case TokenDataType.Vector4:
                    {
                        var castedData = (TokenDataVector4)data;
                        WriteVector4(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.Data);
                        break;
                    }
                case TokenDataType.Vector48Bit:
                    {
                        var castedData = (TokenDataVector4)data;
                        WriteVector48Bit(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.Data);
                        break;
                    }
                case TokenDataType.BoneWeight:
                    {
                        var castedData = (TokenDataBoneWeight)data;
                        WriteBoneWeight(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.BoneIndex,
                            castedData.BoneWeight);
                        break;
                    }
                case TokenDataType.Tri:
                    {
                        var castedData = (TokenDataTri)data;
                        WriteTri(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.A,
                            castedData.B);
                        break;
                    }
                case TokenDataType.Tri16:
                    {
                        var castedData = (TokenDataTri)data;
                        WriteTri16(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.A,
                            castedData.B);
                        break;
                    }
                case TokenDataType.UVSet:
                    {
                        var castedData = (TokenDataUVSet)data;
                        WriteUVSet(
                            data.Token.Name,
                            data.Token.Hash,
                            castedData.UVs.Count,
                            castedData.UVs);
                        break;
                    }
            }
        }

        /// <summary>
        /// Requests the next token from the stream
        /// </summary>
        /// <returns></returns>
        public virtual void WriteTokens(IEnumerable<TokenData> tokenDatas)
        {
            foreach (var tokenData in tokenDatas)
            {
                WriteToken(tokenData);
            }
        }

        /// <summary>
        /// Finalizes the write, performing any necessary compression, flushing, etc.
        /// </summary>
        public abstract void FinalizeWrite();

        /// <summary>
        /// Disposes of the data
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the data
        /// </summary>
        protected abstract void Dispose(bool disposing);
    }
}
