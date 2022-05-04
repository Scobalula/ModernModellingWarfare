using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// 
    /// </summary>
    public class AnimationBoneSampler
    {
        /// <summary>
        /// Bone Parent
        /// </summary>
        public AnimationBoneSampler InternalParent;

        /// <summary>
        /// Gets or Sets the name of the Bone
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the Child Bones
        /// </summary>
        public List<AnimationBoneSampler> Children { get; set; }

        /// <summary>
        /// Gets or Sets the Parent Bone
        /// </summary>
        public AnimationBoneSampler Parent
        {
            get
            {
                return InternalParent;
            }
            internal set
            {
                // Remove from previous bone
                InternalParent?.Children.Remove(this);
                InternalParent = value;
                InternalParent?.Children.Add(this);
            }
        }

        /// <summary>
        /// Gets the sampler
        /// </summary>
        public AnimationSampler Sampler { get; internal set; }

        /// <summary>
        /// Gets the underlying Model Bone this animation is applied to
        /// </summary>
        public ModelBone ModelBone { get; internal set; }

        /// <summary>
        /// Gets the layers
        /// </summary>
        public List<AnimationBoneSamplerLayer> Layers { get; internal set; }

        /// <summary>
        /// Gets or Sets the local bone position
        /// </summary>
        public Vector3 CurrentLocalTranslation { get; set; }

        /// <summary>
        /// Gets or Sets the local bone rotation
        /// </summary>
        public Quaternion CurrentLocalRotation { get; set; }

        /// <summary>
        /// Gets or Sets the world bone scale
        /// </summary>
        public Vector3 CurrentLocalScale { get; set; }

        /// <summary>
        /// Gets or Sets the world bone position
        /// </summary>
        public Vector3 CurrentWorldTranslation { get; set; }

        /// <summary>
        /// Gets or Sets the world bone rotation
        /// </summary>
        public Quaternion CurrentWorldRotation { get; set; }

        /// <summary>
        /// Gets or Sets the world bone scale
        /// </summary>
        public Vector3 CurrentWorldScale { get; set; }

        public Quaternion CurrentInverseRoation => Quaternion.Conjugate(ModelBone.WorldRotation) * CurrentWorldRotation;

        public Vector3 CurrentInverseTranslation => CurrentWorldTranslation - ModelBone.WorldTranslation;

        public AnimationBoneSampler(AnimationBoneSampler parent, ModelBone modelBone)
        {
            Parent                    = parent;
            ModelBone                 = modelBone;

            Children = new List<AnimationBoneSampler>();
            Layers = new List<AnimationBoneSamplerLayer>();
        }

        /// <summary>
        /// Updates the current animation bone sampler
        /// </summary>
        public void Update()
        {
            // TODO: World translation support possibly, but not a priority as I personally never deal 
            // with animations with world translations
            var translationAtFrame = ModelBone.LocalTranslation;
            var rotationAtFrame = ModelBone.LocalRotation;

            foreach (var layer in Layers)
            {
                var bone = layer.AnimationBone;
                var time = layer.Layer.CurrentTime;

                var (firstRIndex, secondRIndex) = GetFramePairIndex(bone.RotationFrames, time, layer.Layer.StartTime, cursor: layer.CurrentRotationsCursor);
                var (firstTIndex, secondTIndex) = GetFramePairIndex(bone.TranslationFrames, time, layer.Layer.StartTime, cursor: layer.CurrentTranslationsCursor);

                // We have a rotation
                if (firstRIndex != -1)
                {
                    var firstFrame = bone.RotationFrames[firstRIndex];
                    var secondFrame = bone.RotationFrames[secondRIndex];

                    firstFrame.Time += layer.Layer.StartTime;
                    secondFrame.Time += layer.Layer.StartTime;

                    Quaternion rot;

                    // Identical Frames, no interpolating
                    if (firstRIndex == secondRIndex)
                        rot = bone.RotationFrames[firstRIndex].Data;
                    else
                        rot = Quaternion.Slerp(firstFrame.Data, secondFrame.Data, (time - firstFrame.Time) / (secondFrame.Time - firstFrame.Time));

                    Quaternion result = rotationAtFrame;

                    switch(layer.TransformType)
                    {
                        case AnimationTransformType.Additive:
                            // Add to current frame
                            result *= rot;
                            break;
                        default:
                            // Take literal value
                            result = rot;
                            break;
                    }

                    // Blend between
                    rotationAtFrame = Quaternion.Slerp(rotationAtFrame, result, layer.Layer.CurrentWeight);
                    // Update cursor (to speed up linear sampling if we're going forward)
                    layer.CurrentRotationsCursor = firstRIndex;
                }

                if (firstTIndex != -1)
                {
                    var firstFrame = bone.TranslationFrames[firstTIndex];
                    var secondFrame = bone.TranslationFrames[secondTIndex];

                    firstFrame.Time += layer.Layer.StartTime;
                    secondFrame.Time += layer.Layer.StartTime;

                    Vector3 translation;

                    Vector3 result = translationAtFrame;

                    // Identical Frames, no interpolating
                    if (firstTIndex == secondTIndex)
                        translation = bone.TranslationFrames[firstTIndex].Data;
                    else
                        translation = Vector3.Lerp(firstFrame.Data, secondFrame.Data, (time - firstFrame.Time) / (secondFrame.Time - firstFrame.Time));

                    switch (layer.TransformType)
                    {
                        case AnimationTransformType.Additive:
                            // Add onto current value
                            result += translation;
                            break;
                        case AnimationTransformType.Relative:
                            // Add onto base (override)
                            result = ModelBone.LocalTranslation + translation;
                            break;
                        default:
                            // Take literal
                            result = translation;
                            break;
                    }

                    // Blend between
                    translationAtFrame = Vector3.Lerp(translationAtFrame, result, layer.Layer.CurrentWeight);
                    // Update cursor (to speed up linear sampling if we're going forward)
                    layer.CurrentTranslationsCursor = firstTIndex;
                }
            }

            CurrentLocalTranslation = translationAtFrame;
            CurrentLocalRotation = rotationAtFrame;

            GenerateWorldTransform();
        }

        /// <summary>
        /// Gets the frame pair indices for the given animation frame list
        /// </summary>
        public static (int, int) GetFramePairIndex<T>(List<AnimationFrame<T>> list, float time, float startTime, float minTime = float.MinValue, float maxTime = float.MaxValue, int cursor = 0)
        {
            // Early quit for lists that we can't "pair"
            if (list == null)
                return (-1, -1);
            if (list.Count == 0)
                return (-1, -1);
            if (list.Count == 1)
                return (0, 0);
            if (time > (startTime + list.Last().Time))
                return (list.Count - 1, list.Count - 1);
            if (time < (startTime + list.First().Time))
                return (0, 0);

            int i;

            // First pass from cursor
            for (i = 0; i < list.Count - 1; i++)
            {
                if (time < (startTime + list[i + 1].Time))
                    return (i, i + 1);
            }

            // Second pass up to cursor
            for (i = 0; i < list.Count - 1 && i < cursor; i++)
            {
                if (time < (startTime + list[i + 1].Time))
                    return (i, i + 1);
            }

            return (list.Count - 1, list.Count - 1);
        }

        public Matrix4x4 GetCurrentLocalMatrix()
        {
            var result = Matrix4x4.CreateFromQuaternion(CurrentLocalRotation);
            result.Translation = CurrentLocalTranslation;
            return result;
        }

        public Matrix4x4 GetCurrentWorldMatrix()
        {
            var result = Matrix4x4.CreateFromQuaternion(CurrentWorldRotation);
            result.Translation = CurrentWorldTranslation;
            return result;
        }

        public void GenerateLocalTransform()
        {
            if (Parent != null)
            {
                CurrentLocalRotation = Quaternion.Conjugate(Parent.CurrentWorldRotation) * CurrentWorldRotation;
                CurrentLocalTranslation = Vector3.Transform(CurrentWorldTranslation - Parent.CurrentWorldTranslation, Quaternion.Conjugate(Parent.CurrentWorldRotation));
            }
            else
            {
                CurrentLocalTranslation = CurrentWorldTranslation;
                CurrentLocalRotation = CurrentWorldRotation;
            }
        }

        public void GenerateLocalTransforms()
        {
            GenerateLocalTransform();

            foreach (var child in Children)
            {
                child.GenerateLocalTransforms();
            }
        }

        public void GenerateWorldTransform()
        {
            if (Parent != null)
            {
                CurrentWorldRotation = Parent.CurrentWorldRotation * CurrentLocalRotation;
                CurrentWorldTranslation = Vector3.Transform(CurrentLocalTranslation, Parent.CurrentWorldRotation) + Parent.CurrentWorldTranslation;
            }
            else
            {
                CurrentWorldTranslation = CurrentLocalTranslation;
                CurrentWorldRotation = CurrentLocalRotation;
            }
        }

        public void GenerateWorldTransforms()
        {
            GenerateWorldTransform();

            foreach (var child in Children)
            {
                child.GenerateWorldTransforms();
            }
        }
    }
}
