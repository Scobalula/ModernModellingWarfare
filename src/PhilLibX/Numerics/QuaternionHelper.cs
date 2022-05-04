using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Numerics
{
    /// <summary>
    /// Class to provide helper methods for working with <see cref="Quaternion"/>s
    /// </summary>
    public static class QuaternionHelper
    {
        /// <summary>
        /// Converts the provided <see cref="Quaternion"/> to Degrees
        /// </summary>
        /// <param name="q">Quaternion to convert</param>
        /// <returns>Result as Degrees</returns>
        public static Vector3 ToDegrees(Quaternion q) =>
            ToEulerAngles(q) * (180 / MathF.PI);

        /// <summary>
        /// Converts the provided <see cref="Quaternion"/> to Euler
        /// </summary>
        /// <param name="q">Quaternion to convert</param>
        /// <returns>Result as Euler</returns>
        public static Vector3 ToEulerAngles(Quaternion q)
        {
            // https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            Vector3 result = Vector3.Zero;

            // Roll
            result.X = MathF.Atan2(
                2.0f * (q.W * q.X + q.Y * q.Z),
                1.0f - 2.0f * (q.X * q.X + q.Y * q.Y));

            // Pitch
            var sinp = 2.0f * (q.W * q.Y - q.Z * q.X);
            if (MathF.Abs(sinp) >= 1)
                result.Y = MathF.CopySign(MathF.PI / 2, sinp);
            else
                result.Y = MathF.Asin(sinp);

            // Yaw
            result.Z = MathF.Atan2(
                2.0f * (q.W * q.Z + q.X * q.Y),
                1.0f - 2.0f * (q.Y * q.Y + q.Z * q.Z));

            return result;
        }

        /// <summary>
        /// Creates a <see cref="Quaternion"/> from the provided degrees
        /// </summary>
        /// <param name="degrees">Degrees to convert</param>
        /// <returns>Resulting Quaternion</returns>
        public static Quaternion CreateFromDegrees(Vector3 degrees) =>
            CreateFromEuler(degrees / (180 / MathF.PI));

        /// <summary>
        /// Creates a <see cref="Quaternion"/> from the provided euler angles
        /// </summary>
        /// <param name="degrees">Euler value to convert</param>
        /// <returns>Resulting Quaternion</returns>
        public static Quaternion CreateFromEuler(Vector3 vec)
        {
            // Abbreviations for the various angular functions
            var cy = MathF.Cos(vec.Z * 0.5f);
            var sy = MathF.Sin(vec.Z * 0.5f);
            var cp = MathF.Cos(vec.Y * 0.5f);
            var sp = MathF.Sin(vec.Y * 0.5f);
            var cr = MathF.Cos(vec.X * 0.5f);
            var sr = MathF.Sin(vec.X * 0.5f);

            Quaternion q;
            q.W = cr * cp * cy + sr * sp * sy;
            q.X = sr * cp * cy - cr * sp * sy;
            q.Y = cr * sp * cy + sr * cp * sy;
            q.Z = cr * cp * sy - sr * sp * cy;

            return q;
        }
    }
}
