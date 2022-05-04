using PhilLibX.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D.FileTranslators.CoDXAsset
{
    /// <summary>
    /// A class to handle writing to an ASCII CoD Export File
    /// </summary>
    public sealed class BinaryTokenWriter : TokenWriter
    {
        private static byte[] LZ4Magic = { 0x2A, 0x4C, 0x5A, 0x34, 0x2A };
        /// <summary>
        /// Gets or Sets the Writer
        /// </summary>
        private BinaryWriter Writer { get; set; }

        private Stream Output { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public BinaryTokenWriter(string fileName)
        {
            Writer = new(new MemoryStream());
            Output = File.Create(fileName);
        }

        public BinaryTokenWriter(Stream stream)
        {
            Writer = new(new MemoryStream());
            Output = stream;
        }

        private void AlignWriter(int alignment)
        {
            alignment -= 1;
            var padSize = (int)((~alignment & Writer.BaseStream.Position + alignment) - Writer.BaseStream.Position);

            if (padSize <= 0)
                return;

            Span<byte> padBuffer = stackalloc byte[padSize];
            Writer.Write(padBuffer);
        }

        ///// <summary>
        ///// Writes the provided hash for the given value and data type
        ///// </summary>
        ///// <param name="tokenID">Token ID</param>
        ///// <param name="dataType">Data type</param>
        //private void WriteHash(string tokenID, TokenDataType dataType)
        //{
        //    AlignWriter(4);
        //    Writer.Write(CRC16(tokenID, (int)dataType));
        //}

        /// <summary>
        /// Writes the provided hash for the given value and data type
        /// </summary>
        /// <param name="tokenID">Token ID</param>
        /// <param name="dataType">Data type</param>
        private void WriteHash(ushort hash)
        {
            AlignWriter(4);
            Writer.Write(hash);
        }

        /// <summary>
        /// Writes the provided hash for the given value and data type
        /// </summary>
        /// <param name="tokenID">Token ID</param>
        /// <param name="dataType">Data type</param>
        private void WriteString(string value)
        {
            AlignWriter(4);
            for (int i = 0; i < value.Length && i < 127; i++)
                Writer.Write((byte)value[i]);
            Writer.Write((byte)0);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        public override void WriteSection(string name, uint hash)
        {
            WriteHash((ushort)hash);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteComment(string name, uint hash, string value)
        {
            WriteHash((ushort)hash);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="boneIndex">Bone index</param>
        /// <param name="parentIndex">Bone parent</param>
        /// <param name="boneName">Bone name</param>
        public override void WriteBoneInfo(string name, uint hash, int boneIndex, int parentIndex, string boneName)
        {
            WriteHash((ushort)hash);
            AlignWriter(4);
            Writer.Write(boneIndex);
            Writer.Write(parentIndex);
            WriteString(boneName);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteFloat(string name, uint hash, float value)
        {
            WriteHash((ushort)hash);
            AlignWriter(4);
            Writer.Write(value);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteInt(string name, uint hash, int value)
        {
            WriteHash((ushort)hash);
            AlignWriter(4);
            Writer.Write(value);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteUInt(string name, uint hash, uint value)
        {
            WriteHash((ushort)hash);
            AlignWriter(4);
            Writer.Write(value);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteShort(string name, uint hash, short value)
        {
            WriteHash((ushort)hash);
            AlignWriter(2);
            Writer.Write(value);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteUShort(string name, uint hash, ushort value)
        {
            WriteHash((ushort)hash);
            AlignWriter(2);
            Writer.Write(value);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector2(string name, uint hash, Vector2 value)
        {
            WriteHash((ushort)hash);
            AlignWriter(4);
            Writer.Write(value.X);
            Writer.Write(value.Y);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector3(string name, uint hash, Vector3 value)
        {
            WriteHash((ushort)hash);
            AlignWriter(4);
            Writer.Write(value.X);
            Writer.Write(value.Y);
            Writer.Write(value.Z);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector316Bit(string name, uint hash, Vector3 value)
        {
            WriteHash((ushort)hash);
            AlignWriter(2);
            Writer.Write((ushort)(value.X * 32767.0f));
            Writer.Write((ushort)(value.Y * 32767.0f));
            Writer.Write((ushort)(value.Z * 32767.0f));
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector4(string name, uint hash, Vector4 value)
        {
            WriteHash((ushort)hash);
            AlignWriter(4);
            Writer.Write(value.X);
            Writer.Write(value.Y);
            Writer.Write(value.Z);
            Writer.Write(value.W);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector48Bit(string name, uint hash, Vector4 value)
        {
            WriteHash((ushort)hash);
            AlignWriter(4);
            Writer.Write((byte)(value.X * 255.0f));
            Writer.Write((byte)(value.Y * 255.0f));
            Writer.Write((byte)(value.Z * 255.0f));
            Writer.Write((byte)(value.W * 255.0f));
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="boneIndex">Bone index</param>
        /// <param name="boneWeight">Bone weight</param>
        public override void WriteBoneWeight(string name, uint hash, int boneIndex, float boneWeight)
        {
            WriteHash((ushort)hash);
            Writer.Write((ushort)boneIndex);
            Writer.Write(boneWeight);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="objectIndex">Object index</param>
        /// <param name="materialIndex">Material index</param>
        public override void WriteTri(string name, uint hash, int objectIndex, int materialIndex)
        {
            WriteHash((ushort)hash);
            Writer.Write((byte)objectIndex);
            Writer.Write((byte)materialIndex);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="objectIndex">Object index</param>
        /// <param name="materialIndex">Material index</param>
        public override void WriteTri16(string name, uint hash, int objectIndex, int materialIndex)
        {
            WriteHash((ushort)hash);
            Writer.Write((ushort)objectIndex);
            Writer.Write((ushort)materialIndex);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteUVSet(string name, uint hash, Vector2 value)
        {
            WriteHash((ushort)hash);
            Writer.Write((ushort)1);
            Writer.Write(value.X);
            Writer.Write(value.Y);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="count">UV Count</param>
        /// <param name="values">Token values</param>
        public override void WriteUVSet(string name, uint hash, int count, IEnumerable<Vector2> values)
        {
            WriteHash((ushort)hash);
            Writer.Write((ushort)count);


            foreach (var value in values)
            {
                Writer.Write((float)value.X);
                Writer.Write((float)value.Y);
            }
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="intVal">Token value</param>
        /// <param name="strVal">Token value</param>
        public override void WriteUShortString(string name, uint hash, ushort intVal, string strVal)
        {
            WriteHash((ushort)hash);
            Writer.Write(intVal);
            WriteString(strVal);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="intVal">Token value</param>
        /// <param name="strVal1">Token value</param>
        /// <param name="strVal2">Token value</param>
        /// <param name="strVal3">Token value</param>
        public override void WriteUShortStringX3(string name, uint hash, ushort intVal, string strVal1, string strVal2, string strVal3)
        {
            WriteHash((ushort)hash);
            Writer.Write(intVal);
            WriteString(strVal1);
            WriteString(strVal2);
            WriteString(strVal3);
        }

        /// <summary>
        /// Finalizes the write, performing any necessary compression, flushing, etc.
        /// </summary>
        public override void FinalizeWrite()
        {
            var stream = Writer.BaseStream as MemoryStream;
            var result = new byte[LZ4.CompressBound((int)stream.Length)];

            var resultSize = LZ4.Compress(stream.GetBuffer(), 0, (int)stream.Length, result, 0, result.Length);

            Output.Write(LZ4Magic);
            Output.Write(BitConverter.GetBytes((int)stream.Length));
            Output.Write(result, 0, resultSize);

            Writer?.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Writer != null)
                {
                    Writer.Dispose();
                    Writer = null;
                }

                if(Output != null)
                {
                    Output.Dispose();
                    Output = null;
                }
            }
        }

        public static ushort CRC16(string value, int init)
        {
            var result = init;

            foreach (var c in value)
            {
                result = (c << 8) ^ result;

                for (int i = 0; i < 8; i++)
                {
                    if ((result & 0x8000) != 0)
                        result = result << 1 ^ 0x1021;
                    else
                        result <<= 1;
                }
            }

            return (ushort)result;
        }
    }
}
