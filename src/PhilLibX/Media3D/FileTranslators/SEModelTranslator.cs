using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;

namespace PhilLibX.Media3D.FileTranslators
{
    /// <summary>
    /// A class to translate a Model to SEModel
    /// </summary>
    public sealed class SEModelTranslator : IMedia3DTranslator
    {
        /// <summary>
        /// SEModel Magic
        /// </summary>
        public static readonly byte[] Magic = { 0x53, 0x45, 0x4D, 0x6F, 0x64, 0x65, 0x6C };

        /// <summary>
        /// Gets the Name of this File Translator
        /// </summary>
        public string Name => "SEModelTranslator";

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
            ".semodel"
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
            using var reader = new BinaryReader(stream, Encoding.Default, true);

            if (!Magic.SequenceEqual(reader.ReadBytes(Magic.Length)))
                throw new IOException("Invalid SEModel Magic");
            if (reader.ReadUInt16() != 0x1)
                throw new IOException("Invalid SEModel Version");
            if (reader.ReadUInt16() != 0x14)
                throw new IOException("Invalid SEModel Header Size");

            var dataPresence = reader.ReadByte();
            var boneDataPresence = reader.ReadByte();
            var meshDataPresence = reader.ReadByte();

            var boneCount = reader.ReadInt32();
            var meshCount = reader.ReadInt32();
            var matCount = reader.ReadInt32();

            var reserved0 = reader.ReadByte();
            var reserved1 = reader.ReadByte();
            var reserved2 = reader.ReadByte();

            var boneNames = new string[boneCount];
            var boneParents = new int[boneCount];

            for (int i = 0; i < boneNames.Length; i++)
            {
                boneNames[i] = ReadUTF8String(reader);
            }

            var hasWorldTransforms = (boneDataPresence & (1 << 0)) != 0;
            var hasLocalTransforms = (boneDataPresence & (1 << 1)) != 0;
            var hasScaleTransforms = (boneDataPresence & (1 << 2)) != 0;

            for (int i = 0; i < boneCount; i++)
            {
                // For now, this flag is unused, and so a non-zero indicates
                // something is wrong in our book
                if (reader.ReadByte() != 0)
                    throw new IOException("Invalid SEModel Bone Flag");

                boneParents[i] = reader.ReadInt32();

                var bone = new ModelBone(boneNames[i]);

                if (hasWorldTransforms)
                {
                    bone.WorldTranslation = new Vector3(
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale);
                    bone.WorldRotation = new Quaternion(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle());
                }
                else
                {
                    bone.WorldTranslation = Vector3.Zero;
                    bone.WorldRotation = Quaternion.Identity;
                }

                if (hasLocalTransforms)
                {
                    bone.LocalTranslation = new Vector3(
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale);
                    bone.LocalRotation = new Quaternion(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle());
                }
                else
                {
                    bone.LocalTranslation = Vector3.Zero;
                    bone.LocalRotation = Quaternion.Identity;
                }

                if (hasScaleTransforms)
                {
                    bone.LocalScale = new Vector3(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle());
                    bone.WorldScale = bone.LocalScale;
                }

                bone.Index = i;

                data.Bones.Add(bone);
            }

            for (int i = 0; i < data.Bones.Count; i++)
            {
                if(boneParents[i] != -1)
                {
                    data.Bones[i].Parent = data.Bones[boneParents[i]];
                }
            }

            var hasUVs      = (meshDataPresence & (1 << 0)) != 0;
            var hasNormals  = (meshDataPresence & (1 << 1)) != 0;
            var hasColours  = (meshDataPresence & (1 << 2)) != 0;
            var hasWeights  = (meshDataPresence & (1 << 3)) != 0;

            var materialIndices = new List<int>[meshCount];

            data.Meshes = new List<Mesh>();

            for (int i = 0; i < meshCount; i++)
            {
                // For now, this flag is unused, and so a non-zero indicates
                // something is wrong in our book
                if (reader.ReadByte() != 0)
                    throw new IOException("Invalid SEModel Mesh Flag");

                var layerCount = reader.ReadByte();
                var influences = reader.ReadByte();
                var vertexCount = reader.ReadInt32();
                var faceCount = reader.ReadInt32();

                var mesh = new Mesh(vertexCount, faceCount, layerCount, influences);

                // Positions
                for (int v = 0; v < vertexCount; v++)
                {
                    mesh.Positions.Add(new(
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale,
                        reader.ReadSingle() * scale));
                }
                // UVs
                if (hasUVs)
                {
                    for (int v = 0; v < vertexCount; v++)
                    {
                        for (int l = 0; l < layerCount; l++)
                        {
                            mesh.UVs.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()), v, l);
                        }
                    }
                }
                // Normals
                if (hasNormals)
                {
                    for (int v = 0; v < vertexCount; v++)
                    {
                        mesh.Normals.Add(new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                    }
                }
                // Colours
                if (hasColours)
                {
                    for (int v = 0; v < vertexCount; v++)
                    {
                        mesh.Colours.Add(new(
                            reader.ReadByte() / 255.0f,
                            reader.ReadByte() / 255.0f,
                            reader.ReadByte() / 255.0f,
                            reader.ReadByte() / 255.0f));
                    }
                }
                // Weights
                if (hasWeights)
                {
                    for (int v = 0; v < vertexCount; v++)
                    {
                        for (int w = 0; w < influences; w++)
                        {
                            mesh.BoneWeights.Add(new(
                                boneCount <= 0xFF ? reader.ReadByte() :
                                boneCount <= 0xFFFF ? reader.ReadUInt16() :
                                reader.ReadInt32(),
                                reader.ReadSingle()), v, w);
                        }
                    }
                }
                // Faces
                for (int f = 0; f < faceCount; f++)
                {
                    if (vertexCount <= 0xFF)
                    {
                        mesh.Faces.Add(new(
                            reader.ReadByte(),
                            reader.ReadByte(),
                            reader.ReadByte()));
                    }
                    else if (vertexCount <= 0xFFFF)
                    {
                        mesh.Faces.Add(new(
                            reader.ReadUInt16(),
                            reader.ReadUInt16(),
                            reader.ReadUInt16()));
                    }
                    else
                    {
                        mesh.Faces.Add(new(
                            reader.ReadInt32(),
                            reader.ReadInt32(),
                            reader.ReadInt32()));
                    }
                }

                materialIndices[i] = new List<int>(layerCount);

                for (int m = 0; m < layerCount; m++)
                {
                    materialIndices[i].Add(reader.ReadInt32());
                }

                data.Meshes.Add(mesh);
            }

            for (int i = 0; i < matCount; i++)
            {
                var mtl = new Material(ReadUTF8String(reader));

                if (reader.ReadBoolean())
                {
                    mtl.Images["DiffuseMap"]  = ReadUTF8String(reader);
                    mtl.Images["NormalMap"]   = ReadUTF8String(reader);
                    mtl.Images["SpecularMap"] = ReadUTF8String(reader);
                }

                data.Materials.Add(mtl);
            }

            // Last pass for materials
            for (int i = 0; i < data.Meshes.Count; i++)
            {
                foreach (var index in materialIndices[i])
                    data.Meshes[i].Materials.Add(data.Materials[index]);
            }
        }

        /// <summary>
        /// Translates the data to the stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Write(Stream stream, string name, Model data, float scale, Dictionary<string, string> config)
        {
            using var writer = new BinaryWriter(stream, Encoding.Default, true);

            writer.Write(Magic);
            writer.Write((ushort)0x1);
            writer.Write((ushort)0x14);
            writer.Write((byte)0x7); // Data Presence
            writer.Write((byte)0x7); // Bone Data Presence
            writer.Write((byte)0xF); // Mesh Data Presence
            writer.Write(data.Bones.Count);
            writer.Write(data.Meshes.Count);
            writer.Write(data.Materials.Count);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);

            foreach (var bone in data.Bones)
            {
                writer.Write(Encoding.ASCII.GetBytes(bone.Name));
                writer.Write((byte)0);
            }

            foreach (var bone in data.Bones)
            {
                writer.Write((byte)0); // Unused flags

                writer.Write(data.Bones.IndexOf(bone.Parent));

                var wt = bone.WorldTranslation;
                var wr = bone.WorldRotation;
                var lt = bone.LocalTranslation;
                var lr = bone.LocalRotation;
                var s  = Vector3.One;

                writer.Write(wt.X * scale);
                writer.Write(wt.Y * scale);
                writer.Write(wt.Z * scale);
                writer.Write(wr.X);
                writer.Write(wr.Y);
                writer.Write(wr.Z);
                writer.Write(wr.W);

                writer.Write(lt.X * scale);
                writer.Write(lt.Y * scale);
                writer.Write(lt.Z * scale);
                writer.Write(lr.X);
                writer.Write(lr.Y);
                writer.Write(lr.Z);
                writer.Write(lr.W);

                writer.Write(s.X);
                writer.Write(s.Y);
                writer.Write(s.Z);
            }

            foreach (var mesh in data.Meshes)
            {
                var vertCount  = mesh.Positions.Count;
                var faceCount  = mesh.Faces.Count;
                var layerCount = mesh.UVs.Dimension;
                var influences = mesh.BoneWeights.Count == 0 ? 0 : mesh.BoneWeights.Dimension;

                writer.Write((byte)0); // Unused flags

                writer.Write((byte)layerCount);
                writer.Write((byte)influences);
                writer.Write(vertCount);
                writer.Write(faceCount);

                // Positions
                for (int i = 0; i < mesh.Positions.Count; i++)
                {
                    writer.Write(mesh.Positions[i].X * scale);
                    writer.Write(mesh.Positions[i].Y * scale);
                    writer.Write(mesh.Positions[i].Z * scale);
                }
                // UVs
                for (int i = 0; i < mesh.Positions.Count; i++)
                {
                    for (int l = 0; l < layerCount; l++)
                    {
                        writer.Write(mesh.UVs[i, l].X);
                        writer.Write(mesh.UVs[i, l].Y);
                    }
                }
                // Normals
                for (int i = 0; i < mesh.Positions.Count; i++)
                {
                    writer.Write(mesh.Normals[i].X);
                    writer.Write(mesh.Normals[i].Y);
                    writer.Write(mesh.Normals[i].Z);
                }
                // Colours
                if(mesh.Colours != null && mesh.Colours.Count != 0)
                {
                    for (int i = 0; i < mesh.Positions.Count; i++)
                    {
                        writer.Write((byte)(mesh.Colours[i].X * 255.0f));
                        writer.Write((byte)(mesh.Colours[i].Y * 255.0f));
                        writer.Write((byte)(mesh.Colours[i].Z * 255.0f));
                        writer.Write((byte)(mesh.Colours[i].W * 255.0f));
                    }
                }
                else
                {
                    writer.Write(new byte[4 * mesh.Positions.Count]);
                }
                // Weights
                if(influences != 0)
                {
                    for (int i = 0; i < mesh.Positions.Count; i++)
                    {
                        for (int w = 0; w < influences; w++)
                        {
                            var (index, value) = mesh.BoneWeights[i, w];

                            if (data.Bones.Count <= 0xFF)
                                writer.Write((byte)index);
                            else if (data.Bones.Count <= 0xFFFF)
                                writer.Write((ushort)index);
                            else
                                writer.Write(index);

                            writer.Write(value);
                        }
                    }
                }

                foreach (var (firstIndex, secondIndex, thirdIndex) in mesh.Faces)
                {
                    if (vertCount <= 0xFF)
                    {
                        writer.Write((byte)firstIndex);
                        writer.Write((byte)secondIndex);
                        writer.Write((byte)thirdIndex);
                    }
                    else if (vertCount <= 0xFFFF)
                    {
                        writer.Write((ushort)firstIndex);
                        writer.Write((ushort)secondIndex);
                        writer.Write((ushort)thirdIndex);
                    }
                    else
                    {
                        writer.Write(firstIndex);
                        writer.Write(secondIndex);
                        writer.Write(thirdIndex);
                    }
                }

                foreach (var material in mesh.Materials)
                    writer.Write(data.Materials.IndexOf(material));
            }

            foreach (var material in data.Materials)
            {
                writer.Write(Encoding.ASCII.GetBytes(material.Name));
                writer.Write((byte)0);
                writer.Write(true);
                writer.Write(Encoding.ASCII.GetBytes(material.Images.TryGetValue("DiffuseMap", out var img) ? img : string.Empty));
                writer.Write((byte)0);
                writer.Write(Encoding.ASCII.GetBytes(material.Images.TryGetValue("NormalMap", out img) ? img : string.Empty));
                writer.Write((byte)0);
                writer.Write(Encoding.ASCII.GetBytes(material.Images.TryGetValue("SpecularMap", out img) ? img : string.Empty));
                writer.Write((byte)0);
            }
        }

        /// <summary>
        /// Checks if the File is Valid
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <param name="buffer">Initial 128 byte buffer from start of file</param>
        /// <returns>True if the file is valid, otherwise false</returns>
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
