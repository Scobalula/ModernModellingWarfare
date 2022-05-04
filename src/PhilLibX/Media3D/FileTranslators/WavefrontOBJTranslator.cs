using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D.FileTranslators
{
    public class WavefrontOBJTranslator : IMedia3DTranslator
    {
        public string Name => throw new NotImplementedException();

        public string Filter => throw new NotImplementedException();

        public bool SupportsModelReading => throw new NotImplementedException();

        public bool SupportsModelWriting => throw new NotImplementedException();

        public bool SupportsAnimationReading => throw new NotImplementedException();

        public bool SupportsAnimationWriting => throw new NotImplementedException();

        public string[] Extensions => throw new NotImplementedException();

        public bool IsValid(string extension, byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, string name, Model data, float scale, Dictionary<string, string> config)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, string name, Animation data, float scale, Dictionary<string, string> config)
        {
            throw new NotImplementedException();
        }

        public void Write(Stream stream, string name, Model data, float scale, Dictionary<string, string> config)
        {
            throw new NotImplementedException();
        }

        public void Write(Stream stream, string name, Animation data, float scale, Dictionary<string, string> config)
        {
            throw new NotImplementedException();
        }
    }
}
