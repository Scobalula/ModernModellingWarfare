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
    public class ExportTokenWriter : TokenWriter
    {
        /// <summary>
        /// Gets or Sets the Writer
        /// </summary>
        public StreamWriter Writer { get; set; }

        public ExportTokenWriter(string fileName)
        {
            Writer = new(fileName);
        }

        public ExportTokenWriter(Stream stream)
        {
            Writer = new(stream);
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        public override void WriteSection(string name, uint hash)
        {
            Writer.WriteLine($"{name}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteComment(string name, uint hash, string value)
        {
            Writer.WriteLine($"{name} {value}");
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
            Writer.WriteLine($"{name} {boneIndex} {parentIndex} \"{boneName}\"");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteFloat(string name, uint hash, float value)
        {
            Writer.WriteLine($"{name} {value}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteInt(string name, uint hash, int value)
        {
            Writer.WriteLine($"{name} {value}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteUInt(string name, uint hash, uint value)
        {
            Writer.WriteLine($"{name} {value}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteShort(string name, uint hash, short value)
        {
            Writer.WriteLine($"{name} {value}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteUShort(string name, uint hash, ushort value)
        {
            Writer.WriteLine($"{name} {value}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector2(string name, uint hash, Vector2 value)
        {
            Writer.WriteLine($"{name} {value.X} {value.Y}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector3(string name, uint hash, Vector3 value)
        {
            Writer.WriteLine($"{name} {value.X} {value.Y} {value.Z}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector316Bit(string name, uint hash, Vector3 value)
        {
            Writer.WriteLine($"{name} {value.X} {value.Y} {value.Z}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector4(string name, uint hash, Vector4 value)
        {
            Writer.WriteLine($"{name} {value.X} {value.Y} {value.Z} {value.W}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteVector48Bit(string name, uint hash, Vector4 value)
        {
            Writer.WriteLine($"{name} {value.X} {value.Y} {value.Z} {value.W}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="boneIndex">Bone index</param>
        /// <param name="boneWeight">Bone weight</param>
        public override void WriteBoneWeight(string name, uint hash, int boneIndex, float boneWeight)
        {
            Writer.WriteLine($"{name} {boneIndex} {boneWeight}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="objectIndex">Object index</param>
        /// <param name="materialIndex">Material index</param>
        public override void WriteTri(string name, uint hash, int objectIndex, int materialIndex)
        {
            Writer.WriteLine($"{name} {objectIndex} {materialIndex} 0 0");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="objectIndex">Object index</param>
        /// <param name="materialIndex">Material index</param>
        public override void WriteTri16(string name, uint hash, int objectIndex, int materialIndex)
        {
            Writer.WriteLine($"{name} {objectIndex} {materialIndex} 0 0");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="value">Token value</param>
        public override void WriteUVSet(string name, uint hash, Vector2 value)
        {
            Writer.WriteLine($"{name} 1 {value.X} {value.Y}");
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="count">UV Count</param>
        /// <param name="values">Token values</param>
        public override void WriteUVSet(string name, uint hash, int count, IEnumerable<Vector2> values)
        {
            Writer.Write($"{name} {count}");

            foreach (var value in values)
            {
                Writer.Write($" {value.X} {value.Y}");
            }

            Writer.WriteLine();
        }

        /// <summary>
        /// Writes the token
        /// </summary>
        /// <param name="name">Name of the token</param>
        /// <param name="intVal">Token value</param>
        /// <param name="strVal">Token value</param>
        public override void WriteUShortString(string name, uint hash, ushort intVal, string strVal)
        {
            Writer.WriteLine($"{name} {intVal} \"{strVal}\"");
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
            Writer.WriteLine($"{name} {intVal} \"{strVal1}\" \"{strVal2}\" \"{strVal3}\"");
        }

        /// <summary>
        /// Finalizes the write, performing any necessary compression, flushing, etc.
        /// </summary>
        public override void FinalizeWrite()
        {
            Writer?.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(Writer != null)
                {
                    Writer.Dispose();
                    Writer = null;
                }
            }
        }
    }
}
