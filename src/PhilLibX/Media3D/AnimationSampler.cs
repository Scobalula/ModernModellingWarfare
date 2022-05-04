using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    public class AnimationSampler
    {
        /// <summary>
        /// Gets or Sets the sampler layers
        /// </summary>
        public List<AnimationSamplerLayer> Layers { get; set; }

        /// <summary>
        /// Gets the samplers
        /// </summary>
        public List<AnimationBoneSampler> BoneSamplers { get; private set; }

        /// <summary>
        /// Gets or Sets the solvers
        /// </summary>
        public List<IAnimationSamplerSolver> Solvers { get; set; }

        /// <summary>
        /// Gets or Sets the current time
        /// </summary>
        public float CurrentTime { get; set; }

        public AnimationSampler(Model model, Animation anim) : this(model, new[] { ("Main", anim, 0.0f) }) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="animation"></param>
        public AnimationSampler(Model model, IEnumerable<(string, Animation, float)> anims)
        {
            // TODO: Optimize lookups, etc. but since this isn't a path often taken (only for sampler init)
            // it should be fine, as the important work is done during update each frame, this is purely
            // resolving model to anim and these classes are intended for offline work/non-performance critical
            // applications
            Layers = new();
            BoneSamplers = new(model.Bones.Count);
            Solvers = new List<IAnimationSamplerSolver>();

            foreach (var bone in model.EnumerateBones())
            {
                BoneSamplers.Add(new AnimationBoneSampler(BoneSamplers.Find(x => x.ModelBone == bone.Parent), bone));
            }

            foreach(var (layerName, anim, startFrame) in anims)
            {
                // No anim
                if (anim == null)
                    continue;

                var layer = new AnimationSamplerLayer(layerName, anim, startFrame);

                foreach (var bone in model.Bones)
                {
                    var animBone = anim.GetBone(bone.Name);

                    if (animBone != null)
                    {
                        var transformType = animBone.TransformType;

                        // Check if we need to resolve transform
                        if (transformType == AnimationTransformType.Parent)
                        {
                            foreach (var parent in bone.EnumerateParents())
                            {
                                var parentAnimBone = anim.GetBone(parent.Name);

                                if (parentAnimBone != null && parentAnimBone.ChildTransformType != AnimationTransformType.Parent)
                                {
                                    transformType = parentAnimBone.ChildTransformType;
                                    break;
                                }
                            }
                        }

                        // Last check, inherit from animation
                        if (transformType == AnimationTransformType.Parent)
                            transformType = anim.TransformType;

                        TryGetBoneSampler(bone.Name).Layers.Add(new(layer, animBone, transformType));
                    }
                }

                Layers.Add(layer);
            }

            // Init first frame
            Update(0, SampleType.AbsoluteFrameTime, false);
        }

        /// <summary>
        /// Gets the bone by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>Bone if found, otherwise null</returns>
        public AnimationBoneSampler TryGetBoneSampler(string boneName) => BoneSamplers.Find(x => x.ModelBone.Name == boneName);

        /// <summary>
        /// Gets the bone index by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>Index of the bone if found, otherwise -1</returns>
        public int GetBoneSamplerIndex(string boneName) => BoneSamplers.FindIndex(x => x.ModelBone.Name == boneName);

        /// <summary>
        /// Attempts to get the bone by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>True of the bone if found, otherwise false</returns>
        public bool TryGetBoneSampler(string boneName, out AnimationBoneSampler sampler) => (sampler = BoneSamplers.Find(x => x.ModelBone.Name == boneName)) != null;

        /// <summary>
        /// Attempts to get the bone index by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>True of the bone if found, otherwise false</returns>
        public bool TryGetBoneSamplerIndex(string boneName, out int boneIndex) => (boneIndex = BoneSamplers.FindIndex(x => x.ModelBone.Name == boneName)) != -1;

        public float GetAverageFrameRate()
        {
            return 30;
        }

        public enum SampleType
        {
            AbsoluteFrameTime,
            AbsoluteTime,
            DeltaTime,
        }

        public List<AnimationBoneSampler> Update(float time, SampleType sampleType, bool loop)
        {
            if (sampleType == SampleType.DeltaTime)
            {
                // Delta time from prev sample
                CurrentTime += time * GetAverageFrameRate();
            }
            else if(sampleType == SampleType.AbsoluteTime)
            {
                CurrentTime = time * GetAverageFrameRate();
            }
            else
            {
                // Absolute
                CurrentTime = time;
            }

            foreach (var layer in Layers)
            {
                layer.Update(time, sampleType, loop, layer.Animation.Framerate);
            }

            foreach (var sampler in BoneSamplers)
            {
                sampler.Update();
            }

            foreach (var solver in Solvers)
            {
                solver.Update(CurrentTime);
            }

            return BoneSamplers;
        }
    } 
}
