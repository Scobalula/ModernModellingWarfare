using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// An interface that describes a Media3D Translator
    /// </summary>
    public interface IMedia3DTranslator
    {
        /// <summary>
        /// Gets the Name of this File Translator
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the Filter for Open/Save File Dialogs
        /// </summary>
        string Filter { get; }

        /// <summary>
        /// Gets if this Translator supports Reading
        /// </summary>
        bool SupportsModelReading { get; }

        /// <summary>
        /// Gets if this Translator supports Writing
        /// </summary>
        bool SupportsModelWriting { get; }

        /// <summary>
        /// Gets if this Translator supports Reading
        /// </summary>
        bool SupportsAnimationReading { get; }

        /// <summary>
        /// Gets if this Translator supports Writing
        /// </summary>
        bool SupportsAnimationWriting { get; }

        /// <summary>
        /// Gets the Extensions associated with this Translator
        /// </summary>
        string[] Extensions { get; }

        /// <summary>
        /// Translates the data to the stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Write(Stream stream, string name, Model data, float scale, Dictionary<string, string> config);

        /// <summary>
        /// Translates the data from the file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Read(Stream stream, string name, Model data, float scale, Dictionary<string, string> config);

        /// <summary>
        /// Translates the data to the stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Write(Stream stream, string name, Animation data, float scale, Dictionary<string, string> config);

        /// <summary>
        /// Translates the data from the file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="data">Data</param>
        /// <param name="config">Misc config information</param>
        public void Read(Stream stream, string name, Animation data, float scale, Dictionary<string, string> config);

        /// <summary>
        /// Checks if the File is Valid
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <returns>True if the file is valid, otherwise false</returns>
        public bool IsValid(string extension) => Extensions?.Contains(extension) == true;

        /// <summary>
        /// Checks if the File is Valid
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <param name="buffer">Initial 128 byte buffer from start of file</param>
        /// <returns>True if the file is valid, otherwise false</returns>
        public bool IsValid(string extension, byte[] buffer);
    }
}
