using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold an animated bone and its frames
    /// </summary>
    public class AnimationBone
    {
        /// <summary>
        /// Gets or Sets the name of the bone
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the transform type for this bone
        /// </summary>
        public AnimationTransformType TransformType { get; set; }

        /// <summary>
        /// Gets or Sets the transform type for bones that are children of bone
        /// </summary>
        public AnimationTransformType ChildTransformType { get; set; }

        /// <summary>
        /// Gets or Sets the translation frames
        /// </summary>
        public List<AnimationFrame<Vector3>> TranslationFrames { get; set; }

        /// <summary>
        /// Gets or Sets the rotation frames
        /// </summary>
        public List<AnimationFrame<Quaternion>> RotationFrames { get; set; }

        /// <summary>
        /// Gets or Sets the translation frames
        /// </summary>
        public List<AnimationFrame<Vector3>> ScaleFrames { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationBone"/> class
        /// </summary>
        /// <param name="name">Name of the bone</param>
        public AnimationBone(string name)
        {
            Name               = name;
            ChildTransformType = AnimationTransformType.Parent;
            TransformType      = AnimationTransformType.Parent;

            TranslationFrames = new List<AnimationFrame<Vector3>>();
            RotationFrames    = new List<AnimationFrame<Quaternion>>();
            ScaleFrames       = new List<AnimationFrame<Vector3>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationBone"/> class
        /// </summary>
        /// <param name="name">Name of the bone</param>
        /// <param name="transformType">Transform type for this bone</param>
        public AnimationBone(string name, AnimationTransformType transformType)
        {
            Name               = name;
            TransformType      = transformType;
            ChildTransformType = transformType;

            TranslationFrames = new List<AnimationFrame<Vector3>>();
            RotationFrames    = new List<AnimationFrame<Quaternion>>();
            ScaleFrames       = new List<AnimationFrame<Vector3>>();
        }

        /// <summary>
        /// Sets the key frame at the given time
        /// </summary>
        /// <param name="time">Frame time</param>
        /// <param name="x">X data</param>
        /// <param name="y">Y data</param>
        /// <param name="z">Z data</param>
        public void AddTranslationKeyFrame(float time, float x, float y, float z) => AddTranslationKeyFrame((float)time, new(x, y, z));

        /// <summary>
        /// Sets the key frame at the given time
        /// </summary>
        /// <param name="time">Frame time</param>
        /// <param name="x">X data</param>
        /// <param name="y">Y data</param>
        /// <param name="z">Z data</param>
        public void AddTranslationKeyFrame(float time, double x, double y, double z) => AddTranslationKeyFrame((float)time, new((float)x, (float)y, (float)z));

        /// <summary>
        /// Sets the key frame at the given time
        /// </summary>
        /// <param name="time">Frame time</param>
        /// <param name="data">Data</param>
        public void AddTranslationKeyFrame(float time, Vector3 data)
        {
            TranslationFrames.Add(new AnimationFrame<Vector3>((float)time, data));
        }

        /// <summary>
        /// Adds a rotation frame to this bone
        /// </summary>
        /// <param name="time">Frame time</param>
        /// <param name="x">X data</param>
        /// <param name="y">Y data</param>
        /// <param name="z">Z data</param>
        /// <param name="w">W data</param>
        public void AddRotationKeyFrame(float time, float x, float y, float z, float w) => AddRotationKeyFrame((float)time, new(x, y, z, w));

        /// <summary>
        /// Adds a rotation frame to this bone
        /// </summary>
        /// <param name="time">Frame time</param>
        /// <param name="x">X data</param>
        /// <param name="y">Y data</param>
        /// <param name="z">Z data</param>
        /// <param name="w">W data</param>
        public void AddRotationKeyFrame(float time, double x, double y, double z, double w) => AddRotationKeyFrame((float)time, new((float)x, (float)y, (float)z, (float)w));

        /// <summary>
        /// Adds a scale frame to this bone
        /// </summary>
        /// <param name="time">Frame time</param>
        /// <param name="data">Data</param>
        public void AddRotationKeyFrame(float time, Quaternion data)
        {
            RotationFrames.Add(new AnimationFrame<Quaternion>((float)time, data));
        }

        /// <summary>
        /// Adds a scale frame to this bone
        /// </summary>
        /// <param name="time">Frame time</param>
        /// <param name="x">X data</param>
        /// <param name="y">Y data</param>
        /// <param name="z">Z data</param>
        public void AddScaleKeyFrame(float time, float x, float y, float z) => AddScaleKeyFrame((float)time, new(x, y, z));

        /// <summary>
        /// Adds a scale frame to this bone
        /// </summary>
        /// <param name="time">Frame time</param>
        /// <param name="x">X data</param>
        /// <param name="y">Y data</param>
        /// <param name="z">Z data</param>
        public void AddScaleKeyFrame(float time, double x, double y, double z) => AddScaleKeyFrame((float)time, new((float)x, (float)y, (float)z));

        /// <summary>
        /// Adds a scale frame to this bone
        /// </summary>
        /// <param name="time">Frame time</param>
        /// <param name="data">Data</param>
        public void AddScaleKeyFrame(float time, Vector3 data)
        {
            ScaleFrames.Add(new AnimationFrame<Vector3>((float)time, data));
        }

        public void ScaleTranslationFrames(float scale)
        {
            var firstFrame = TranslationFrames[0];

            for (int i = 0; i < TranslationFrames.Count; i++)
            {
                TranslationFrames[i] = new(TranslationFrames[i].Time, firstFrame.Data + (TranslationFrames[i].Data - firstFrame.Data) * scale);
            }
        }

        public void ScaleRotationFrames(float scale)
        {
            var firstFrame = RotationFrames[0];

            for (int i = 0; i < RotationFrames.Count; i++)
            {
                RotationFrames[i] = new(RotationFrames[i].Time, firstFrame.Data * (firstFrame.Data * Quaternion.Conjugate(RotationFrames[i].Data) * scale));
            }
        }
    }
}
