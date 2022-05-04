using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using PhilLibX.Media3D.FileTranslators.CoDXAsset;

namespace PhilLibX.Media3D.FileTranslators
{
    /// <summary>
    /// A class to translate a Model to a CoD XModel
    /// </summary>
    public sealed class CoDXModelTranslator : IMedia3DTranslator
    {
        /// <summary>
        /// Gets the Name of this File Translator
        /// </summary>
        public string Name => "CoDXModelTranslator";

        /// <summary>
        /// Gets the Filter for Open/Save File Dialogs
        /// </summary>
        public string Filter => "";

        /// <summary>
        /// Gets if this Translator supports Reading
        /// </summary>
        public bool SupportsModelReading => true;

        /// <summary>
        /// Gets if this Translator supports Writing
        /// </summary>
        public bool SupportsModelWriting => true;

        /// <summary>
        /// Gets if this Translator supports Reading
        /// </summary>
        public bool SupportsAnimationReading => false;

        /// <summary>
        /// Gets if this Translator supports Writing
        /// </summary>
        public bool SupportsAnimationWriting => false;

        /// <summary>
        /// Gets the Extensions associated with this Translator
        /// </summary>
        public string[] Extensions { get; } =
        {
            ".xmodel_bin",
            ".xmodel_export"
        };

        /// <summary>
        /// Translates the data to the stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Write(Stream stream, string name, Animation data, float scale, Dictionary<string, string> config) => throw new NotSupportedException("SEModel format does not support Animations");

        /// <summary>
        /// Translates the data from the file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Read(Stream stream, string name, Animation data, float scale, Dictionary<string, string> config) => throw new NotSupportedException("SEModel format does not support Animations");

        /// <summary>
        /// Translates the data from the file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Read(Stream stream, string name, Model data, float scale, Dictionary<string, string> config)
        {
            var useBin = name.EndsWith(".xmodel_bin");

            //using TokenReader reader = useBin ? new BinaryTokenReader(stream) : new ExportTokenReader(stream);

            // TODO: Read support for CoD XModels
            throw new NotSupportedException("Reading CoD XModels is currently not supported");
        }

        /// <summary>
        /// Translates the data to the stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Write(Stream stream, string name, Model data, float scale, Dictionary<string, string> config)
        {
            var useBin = name.EndsWith(".xmodel_bin");

            using TokenWriter writer = useBin ? new BinaryTokenWriter(stream) : new ExportTokenWriter(stream);

            writer.WriteSection("MODEL", 0x46C8);
            writer.WriteUShort("VERSION", 0x24D1, 6);


            writer.WriteUShort("NUMBONES", 0x76BA, (ushort)data.Bones.Count);

            for (int i = 0; i < data.Bones.Count; i++)
            {
                writer.WriteBoneInfo(
                    "BONE",
                    0xF099,
                    i,
                    data.Bones.IndexOf(data.Bones[i].Parent),
                    data.Bones[i].Name);
            }

            for (int i = 0; i < data.Bones.Count; i++)
            {
                var worldMatrix = Matrix4x4.CreateFromQuaternion(data.Bones[i].WorldRotation);

                writer.WriteUShort("BONE", 0xDD9A, (ushort)i);
                writer.WriteVector3("OFFSET", 0x9383, data.Bones[i].WorldTranslation * scale);
                writer.WriteVector3("SCALE", 0x1C56, data.Bones[i].WorldScale);
                writer.WriteVector316Bit("X", 0xDCFD, new(worldMatrix.M11, worldMatrix.M12, worldMatrix.M13));
                writer.WriteVector316Bit("Y", 0xCCDC, new(worldMatrix.M21, worldMatrix.M22, worldMatrix.M23));
                writer.WriteVector316Bit("Z", 0xFCBF, new(worldMatrix.M31, worldMatrix.M32, worldMatrix.M33));
            }

            writer.WriteInt("NUMVERTS32", 0x2AEC, data.Meshes.Sum(x => x.Positions.Count));

            int globalVertexIndex = 0;

            foreach (var mesh in data.Meshes)
            {
                var weightsPerVtx = mesh.BoneWeights == null ? (ushort)0 : (ushort)mesh.BoneWeights.Dimension;

                for (int i = 0; i < mesh.Positions.Count; i++)
                {
                    writer.WriteInt("VERT32", 0xB097, globalVertexIndex + i);
                    writer.WriteVector3("OFFSET", 0x9383, mesh.Positions[i] * scale);

                    ushort weightCount;

                    // Build count based off non-zero weights
                    for (weightCount = 0; weightCount < weightsPerVtx; weightCount++)
                        if (mesh.BoneWeights[i, weightCount].Item2 <= 0.0001)
                            break;

                    writer.WriteUShort("BONES", 0xEA46, weightCount);

                    for (int w = 0; w < weightCount; w++)
                    {
                        var (index, weight) = mesh.BoneWeights[i, w];
                        writer.WriteBoneWeight("BONE", 0xF1AB, index, weight);
                    }
                }

                globalVertexIndex += mesh.Positions.Count;
            }

            globalVertexIndex = 0;

            writer.WriteInt("NUMFACES", 0xBE92, data.Meshes.Sum(x => x.Faces.Count));

            foreach (var mesh in data.Meshes)
            {
                var materialIndex = data.Materials.IndexOf(mesh.Materials[0]);

                foreach (var (i0, i1, i2) in mesh.Faces)
                {
                    writer.WriteTri("TRI", 0x562F, 0, materialIndex);

                    writer.WriteInt("VERT32", 0xB097, i0 + globalVertexIndex);
                    writer.WriteVector316Bit("NORMAL", 0x89EC, mesh.Normals[i0]);
                    writer.WriteVector48Bit("COLOR", 0x6DD8, mesh.Colours != null && mesh.Colours.Count != 0 ? mesh.Colours[i0] : Vector4.One);
                    writer.WriteUVSet("UV", 0x1AD4, mesh.UVs[i0, 0]);

                    writer.WriteInt("VERT32", 0xB097, i1 + globalVertexIndex);
                    writer.WriteVector316Bit("NORMAL", 0x89EC, mesh.Normals[i1]);
                    writer.WriteVector48Bit("COLOR", 0x6DD8, mesh.Colours != null && mesh.Colours.Count != 0 ? mesh.Colours[i0] : Vector4.One);
                    writer.WriteUVSet("UV", 0x1AD4, mesh.UVs[i1, 0]);

                    writer.WriteInt("VERT32", 0xB097, i2 + globalVertexIndex);
                    writer.WriteVector316Bit("NORMAL", 0x89EC, mesh.Normals[i2]);
                    writer.WriteVector48Bit("COLOR", 0x6DD8, mesh.Colours != null && mesh.Colours.Count != 0 ? mesh.Colours[i0] : Vector4.One);
                    writer.WriteUVSet("UV", 0x1AD4, mesh.UVs[i2, 0]);
                }

                globalVertexIndex += mesh.Positions.Count;
            }

            writer.WriteUShort("NUMOBJECTS", 0x62AF, (ushort)data.Meshes.Count);
            ushort idx = 0;

            foreach (var mesh in data.Meshes)
            {
                writer.WriteUShortString("OBJECT", 0x87D4, idx, $"Mesh_{idx++}");
            }

            writer.WriteUShort("NUMMATERIALS", 0xA1B2, (ushort)data.Materials.Count);
            idx = 0;

            foreach (var material in data.Materials)
            {
                writer.WriteUShortStringX3("MATERIAL", 0xA700, idx++, material.Name, "lambert", "");
                writer.WriteVector48Bit("COLOR", 0x6DD8, Vector4.One);
                writer.WriteVector4("TRANSPARENCY", 0x6DAB, Vector4.One);
                writer.WriteVector4("AMBIENTCOLOR", 0x37FF, Vector4.One);
                writer.WriteVector4("INCANDESCENCE", 0x4265, Vector4.One);
                writer.WriteVector2("COEFFS", 0xC835, Vector2.One);
                writer.WriteVector2("GLOW", 0xFE0C, Vector2.One);
                writer.WriteVector2("REFRACTIVE", 0x7E24, Vector2.One);
                writer.WriteVector4("SPECULARCOLOR", 0x317C, Vector4.One);
                writer.WriteVector4("REFLECTIVECOLOR", 0xE593, Vector4.One);
                writer.WriteVector2("REFLECTIVE", 0x7D76, Vector2.One);
                writer.WriteVector2("BLINN", 0x83C7, Vector2.One);
                writer.WriteFloat("PHONG", 0x5CD2, -1);
            }

            writer.FinalizeWrite();
        }

        /// <summary>
        /// Checks if the File is Valid
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <param name="buffer">Initial 128 byte buffer from start of file</param>
        /// <returns>True if the file is valid, otherwise false</returns>
        /// <param name="config">Misc config information</param>
        public bool IsValid(string extension, byte[] buffer)
        {
            return Extensions.Contains(extension);
        }

        /// <summary>
        /// Reads a UTF8 string from the file
        /// </summary>
        internal static string ReadUTF8String(BinaryReader reader)
        {
            var output = new StringBuilder(32);

            while (true)
            {
                var c = reader.ReadByte();
                if (c == 0)
                    break;
                output.Append(Convert.ToChar(c));
            }

            return output.ToString();
        }
    }
}
