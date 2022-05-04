using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PhilLibX.Media3D.FileTranslators
{
    /// <summary>
    /// A class to translate a Model to SEModel
    /// </summary>
    public sealed class SEAnimTranslator : IMedia3DTranslator
    {
        /// <summary>
        /// Gets the SEAnim Magic
        /// </summary>
        private static char[] SEAnimMagic => new char[] { 'S', 'E', 'A', 'n', 'i', 'm' };

        /// <summary>
        /// Gets the Name of this File Translator
        /// </summary>
        public string Name => "SEAnimTranslator";

        /// <summary>
        /// Gets the Filter for Open/Save File Dialogs
        /// </summary>
        public string Filter => "";

        /// <summary>
        /// Gets if this Translator supports Reading
        /// </summary>
        public bool SupportsModelReading => false;

        /// <summary>
        /// Gets if this Translator supports Writing
        /// </summary>
        public bool SupportsModelWriting => false;

        /// <summary>
        /// Gets if this Translator supports Reading
        /// </summary>
        public bool SupportsAnimationReading => true;

        /// <summary>
        /// Gets if this Translator supports Writing
        /// </summary>
        public bool SupportsAnimationWriting => true;

        /// <summary>
        /// Gets the Extensions associated with this Translator
        /// </summary>
        public string[] Extensions { get; } =
        {
            ".seanim"
        };

        /// <summary>
        /// Translates the data to the stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Write(Stream stream, string name, Animation data, float scale, Dictionary<string, string> config)
        {
            // Determine bones with different types
            var boneModifiers = new Dictionary<int, byte>();

            var frameCount = data.GetAnimationFrameCount();
            int index = 0;

            foreach (var bone in data.Bones)
            {
                if (bone.TransformType != AnimationTransformType.Parent && bone.TransformType != data.TransformType)
                {
                    // Convert to SEAnim Type
                    switch (bone.TransformType)
                    {
                        case AnimationTransformType.Absolute: boneModifiers[index] = 0; break;
                        case AnimationTransformType.Additive: boneModifiers[index] = 1; break;
                        case AnimationTransformType.Relative: boneModifiers[index] = 2; break;
                    }
                }

                index++;
            }

            using var writer = new BinaryWriter(stream);

            writer.Write(SEAnimMagic);
            writer.Write((ushort)0x1);
            writer.Write((ushort)0x1C);

            // Convert to SEAnim Type
            switch (data.TransformType)
            {
                case AnimationTransformType.Absolute: writer.Write((byte)0); break;
                case AnimationTransformType.Additive: writer.Write((byte)1); break;
                case AnimationTransformType.Relative: writer.Write((byte)2); break;
            }

            writer.Write((byte)0);

            byte flags = 0;

            if (data.ContainsTranslationKeys)
                flags |= 1;
            if (data.ContainsRotationKeys)
                flags |= 2;
            if (data.ContainsScaleKeys)
                flags |= 4;
            if (data.Notifications.Count > 0)
                flags |= 64;

            writer.Write(flags);
            writer.Write((byte)0);
            writer.Write((ushort)0);
            writer.Write(data.Framerate);
            writer.Write((int)frameCount);
            writer.Write(data.Bones.Count);
            writer.Write((byte)boneModifiers.Count);
            writer.Write((byte)0);
            writer.Write((ushort)0);
            writer.Write(data.GetNotificationFrameCount());

            foreach (var bone in data.Bones)
            {
                writer.Write(Encoding.UTF8.GetBytes(bone.Name));
                writer.Write((byte)0);
            }

            foreach (var modifier in boneModifiers)
            {
                if (data.Bones.Count <= 0xFF)
                    writer.Write((byte)modifier.Key);
                else if (data.Bones.Count <= 0xFFFF)
                    writer.Write((ushort)modifier.Key);
                else
                    throw new NotSupportedException();

                writer.Write(modifier.Value);
            }

            foreach (var bone in data.Bones)
            {
                writer.Write((byte)0);

                // TranslationFrames
                if ((flags & 1) != 0)
                {
                    if (frameCount <= 0xFF)
                        writer.Write((byte)bone.TranslationFrames.Count);
                    else if (frameCount <= 0xFFFF)
                        writer.Write((ushort)bone.TranslationFrames.Count);
                    else
                        writer.Write(bone.TranslationFrames.Count);

                    foreach (var frame in bone.TranslationFrames)
                    {
                        if (frameCount <= 0xFF)
                            writer.Write((byte)frame.Time);
                        else if (frameCount <= 0xFFFF)
                            writer.Write((ushort)frame.Time);
                        else
                            writer.Write((int)frame.Time);

                        writer.Write(frame.Data.X * scale);
                        writer.Write(frame.Data.Y * scale);
                        writer.Write(frame.Data.Z * scale);
                    }
                }

                // RotationFrames
                if ((flags & 2) != 0)
                {
                    if (frameCount <= 0xFF)
                        writer.Write((byte)bone.RotationFrames.Count);
                    else if (frameCount <= 0xFFFF)
                        writer.Write((ushort)bone.RotationFrames.Count);
                    else
                        writer.Write(bone.RotationFrames.Count);

                    foreach (var frame in bone.RotationFrames)
                    {
                        if (frameCount <= 0xFF)
                            writer.Write((byte)frame.Time);
                        else if (frameCount <= 0xFFFF)
                            writer.Write((ushort)frame.Time);
                        else
                            writer.Write((int)frame.Time);

                        writer.Write(frame.Data.X);
                        writer.Write(frame.Data.Y);
                        writer.Write(frame.Data.Z);
                        writer.Write(frame.Data.W);
                    }
                }

                // ScaleFrames
                if ((flags & 4) != 0)
                {
                    if (frameCount <= 0xFF)
                        writer.Write((byte)bone.ScaleFrames.Count);
                    else if (frameCount <= 0xFFFF)
                        writer.Write((ushort)bone.ScaleFrames.Count);
                    else
                        writer.Write(bone.ScaleFrames.Count);

                    foreach (var frame in bone.ScaleFrames)
                    {
                        if (frameCount <= 0xFF)
                            writer.Write((byte)frame.Time);
                        else if (frameCount <= 0xFFFF)
                            writer.Write((ushort)frame.Time);
                        else
                            writer.Write((int)frame.Time);

                        writer.Write(frame.Data.X);
                        writer.Write(frame.Data.Y);
                        writer.Write(frame.Data.Z);
                    }
                }
            }

            foreach (var note in data.Notifications)
            {
                foreach (var frame in note.Frames)
                {
                    if (frameCount <= 0xFF)
                        writer.Write((byte)frame.Time);
                    else if (frameCount <= 0xFFFF)
                        writer.Write((ushort)frame.Time);
                    else
                        writer.Write((int)frame.Time);

                    writer.Write(Encoding.UTF8.GetBytes(note.Name));
                    writer.Write((byte)0);
                }
            }
        }

        /// <summary>
        /// Translates the data from the file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Read(Stream stream, string name, Animation data, float scale, Dictionary<string, string> config)
        {
            using var reader = new BinaryReader(stream);

            var magic = reader.ReadChars(6);
            var version = reader.ReadInt16();
            var sizeofHeader = reader.ReadInt16();

            switch (reader.ReadByte())
            {
                case 0: data.TransformType = AnimationTransformType.Absolute; break;
                case 1: data.TransformType = AnimationTransformType.Additive; break;
                case 2: data.TransformType = AnimationTransformType.Relative; break;
            }

            var flags         = reader.ReadByte();
            var dataFlags     = reader.ReadByte();
            var dataPropFlags = reader.ReadByte();
            var reserved      = reader.ReadUInt16();
            var frameRate     = reader.ReadSingle();
            var frameCount    = reader.ReadInt32();
            var boneCount     = reader.ReadInt32();
            var modCount      = reader.ReadByte();
            var reserved0     = reader.ReadByte();
            var reserved1     = reader.ReadByte();
            var reserved2     = reader.ReadByte();
            var noteCount     = reader.ReadInt32();

            data.Framerate = frameRate;

            var bones = new AnimationBone[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
                bones[i] = data.CreateBone(ReadUTF8String(reader));
            }

            for (int i = 0; i < modCount; i++)
            {
                var boneIndex = boneCount <= 0xFF ? reader.ReadByte() : reader.ReadUInt16();

                switch (reader.ReadByte())
                {
                    case 0: bones[boneIndex].ChildTransformType = AnimationTransformType.Absolute; break;
                    case 1: bones[boneIndex].ChildTransformType = AnimationTransformType.Additive; break;
                    case 2: bones[boneIndex].ChildTransformType = AnimationTransformType.Relative; break;
                }
            }

            foreach (var bone in bones)
            {
                reader.ReadByte();

                if ((dataFlags & 1) != 0)
                {
                    int keyCount;

                    if (frameCount <= 0xFF)
                        keyCount = reader.ReadByte();
                    else if (frameCount <= 0xFFFF)
                        keyCount = reader.ReadUInt16();
                    else
                        keyCount = reader.ReadInt32();

                    for (int f = 0; f < keyCount; f++)
                    {
                        int frame;

                        if (frameCount <= 0xFF)
                            frame = reader.ReadByte();
                        else if (frameCount <= 0xFFFF)
                            frame = reader.ReadUInt16();
                        else
                            frame = reader.ReadInt32();

                        if ((dataPropFlags & (1 << 0)) == 0)
                            bone.AddTranslationKeyFrame(
                                frame,
                                reader.ReadSingle() * scale,
                                reader.ReadSingle() * scale,
                                reader.ReadSingle() * scale);
                        else
                            bone.AddTranslationKeyFrame(
                                frame,
                                reader.ReadDouble() * scale,
                                reader.ReadDouble() * scale,
                                reader.ReadDouble() * scale);
                    }
                }

                if ((dataFlags & 2) != 0)
                {
                    int keyCount;

                    if (frameCount <= 0xFF)
                        keyCount = reader.ReadByte();
                    else if (frameCount <= 0xFFFF)
                        keyCount = reader.ReadUInt16();
                    else
                        keyCount = reader.ReadInt32();

                    for (int f = 0; f < keyCount; f++)
                    {
                        int frame;

                        if (frameCount <= 0xFF)
                            frame = reader.ReadByte();
                        else if (frameCount <= 0xFFFF)
                            frame = reader.ReadUInt16();
                        else
                            frame = reader.ReadInt32();

                        if ((dataPropFlags & (1 << 0)) == 0)
                            bone.AddRotationKeyFrame(frame, reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        else
                            bone.AddRotationKeyFrame(frame, reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
                    }
                }

                if ((dataFlags & 4) != 0)
                {
                    int keyCount;

                    if (frameCount <= 0xFF)
                        keyCount = reader.ReadByte();
                    else if (frameCount <= 0xFFFF)
                        keyCount = reader.ReadUInt16();
                    else
                        keyCount = reader.ReadInt32();

                    for (int f = 0; f < keyCount; f++)
                    {
                        int frame;

                        if (frameCount <= 0xFF)
                            frame = reader.ReadByte();
                        else if (frameCount <= 0xFFFF)
                            frame = reader.ReadUInt16();
                        else
                            frame = reader.ReadInt32();

                        if ((dataPropFlags & (1 << 0)) == 0)
                            bone.AddScaleKeyFrame(frame, reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        else
                            bone.AddScaleKeyFrame(frame, reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
                    }
                }
            }

            for (int i = 0; i < noteCount; i++)
            {
                int frame;

                if (frameCount <= 0xFF)
                    frame = reader.ReadByte();
                else if (frameCount <= 0xFFFF)
                    frame = reader.ReadUInt16();
                else
                    frame = reader.ReadInt32();

                data.CreateNotification(ReadUTF8String(reader)).Frames.Add(new AnimationFrame<Action>(frame, null));
            }
        }

        /// <summary>
        /// Translates the data from the file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Read(Stream stream, string name, Model data, float scale, Dictionary<string, string> config) => throw new NotSupportedException("SEAnim format does not support Models");

        /// <summary>
        /// Translates the data to the stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Write(Stream stream, string name, Model data, float scale, Dictionary<string, string> config) => throw new NotSupportedException("SEAnim format does not support Models");

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
