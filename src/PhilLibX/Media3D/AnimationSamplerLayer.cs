using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// 
    /// </summary>
    public class AnimationSamplerLayer
    {
        /// <summary>
        /// Gets or Sets the name of the layer
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the underlying animation
        /// </summary>
        public Animation Animation { get; set; }

        /// <summary>
        /// Gets or Sets the weights
        /// </summary>
        public List<AnimationFrame<float>> Weights { get; set; }

        /// <summary>
        /// Gets or Sets the start time
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        /// Gets or Sets the max time
        /// </summary>
        public float MaxTime { get; set; }

        /// <summary>
        /// Gets the length of the animation
        /// </summary>
        public float Framerate { get; private set; }

        /// <summary>
        /// Gets the length of the animation
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// Gets or Sets the current time
        /// </summary>
        public float CurrentTime { get; set; }

        /// <summary>
        /// Gets or Sets the Weights Cursor
        /// </summary>
        public int CurrentWeightsCursor { get; set; }

        /// <summary>
        /// Gets or Sets the current weight
        /// </summary>
        public float CurrentWeight { get; set; }

        /// <summary>
        /// Gets or Sets the default weight
        /// </summary>
        public float DefaultWeight { get; set; }

        public AnimationSamplerLayer(string name, Animation animation, float startTime)
        {
            Name = name;
            Animation = animation;
            Weights = new List<AnimationFrame<float>>();
            Framerate = animation.Framerate == 0 ? 30 : animation.Framerate;
            Length = animation.GetAnimationFrameCount();
            StartTime = startTime;
            CurrentTime = 0;
            CurrentWeightsCursor = 0;
            CurrentWeight = 1;
            DefaultWeight = 1;
        }

        public void Update(float time, AnimationSampler.SampleType sampleType, bool loop, float frameRate)
        {
            if (sampleType == AnimationSampler.SampleType.DeltaTime)
            {
                // Delta time from prev sample
                CurrentTime += time * frameRate;
            }
            else if (sampleType == AnimationSampler.SampleType.AbsoluteTime)
            {
                CurrentTime = time * frameRate;
            }
            else
            {
                // Absolute
                CurrentTime = time;
            }

            if (loop)
            {
                CurrentTime %= Length;
            }

            CurrentWeight = GetWeight(CurrentTime, StartTime);
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetWeight(float time, float startTime)
        {
            if (time < StartTime)
                return 0;

            var (firstIndex, secondIndex) = AnimationBoneSampler.GetFramePairIndex(Weights, time, startTime, cursor: CurrentWeightsCursor);
            var result = DefaultWeight;

            if (firstIndex != -1)
            {
                if (firstIndex == secondIndex)
                {
                    result = Weights[firstIndex].Data;
                }
                else
                {
                    var firstFrame = Weights[firstIndex];
                    var secondFrame = Weights[secondIndex];

                    firstFrame.Time += startTime;
                    secondFrame.Time += startTime;

                    var lerpAmount = (time - firstFrame.Time) / (secondFrame.Time - firstFrame.Time);

                    result = (firstFrame.Data * (1 - lerpAmount)) + (secondFrame.Data * lerpAmount);
                }

                CurrentWeightsCursor = firstIndex;
            }

            return result;
        }
    }
}
