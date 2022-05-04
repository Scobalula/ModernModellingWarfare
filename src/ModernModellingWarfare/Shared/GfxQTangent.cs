using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ModernModellingWarfare.Shared
{
    /// <summary>
    /// A struct to hold a Modern Warfare Packed Tangent (QTangent/Quaternion)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    internal struct GfxQTangent
    {
        /// <summary>
        /// The packed quaternion value
        /// </summary>
        public uint PackedQuaternion;

        /// <summary>
        /// Gets the tangent frame from the quaternion
        /// </summary>
        /// <returns>Tangent, bitangent, and normal results</returns>
        public (Vector3, Vector3, Vector3) Unpack()
        {
            // https://dev.theomader.com/qtangents/
            var idx = PackedQuaternion >> 30;

            var tx = ((PackedQuaternion >> 00 & 0x3FF) / 511.5f - 1.0f) / 1.4142135f;
            var ty = ((PackedQuaternion >> 10 & 0x3FF) / 511.5f - 1.0f) / 1.4142135f;
            var tz = ((PackedQuaternion >> 20 & 0x1FF) / 255.5f - 1.0f) / 1.4142135f;
            var tw = 0.0f;

            var sum = tx * tx + ty * ty + tz * tz;

            if (sum <= 1.0f)
                tw = (float)Math.Sqrt(1.0f - sum);

            var q = Quaternion.Identity;

            switch (idx)
            {
                case 0:
                    q.X = tw;
                    q.Y = tx;
                    q.Z = ty;
                    q.W = tz;
                    break;
                case 1:
                    q.X = tx;
                    q.Y = tw;
                    q.Z = ty;
                    q.W = tz;
                    break;
                case 2:
                    q.X = tx;
                    q.Y = ty;
                    q.Z = tw;
                    q.W = tz;
                    break;
                case 3:
                    q.X = tx;
                    q.Y = ty;
                    q.Z = tz;
                    q.W = tw;
                    break;
            }

            var tangent = new Vector3(
                1 - 2 * (q.Y * q.Y + q.Z * q.Z),
                2 * (q.X * q.Y + q.W * q.Z),
                2 * (q.X * q.Z - q.W * q.Y));
            var bitangent = new Vector3(
                2 * (q.X * q.Y - q.W * q.Z),
                1 - 2 * (q.X * q.X + q.Z * q.Z),
                2 * (q.Y * q.Z + q.W * q.X));
            var normal = Vector3.Cross(tangent, bitangent);

            return (tangent, bitangent, normal);
        }
    }
}
