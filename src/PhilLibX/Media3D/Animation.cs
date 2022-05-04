using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold 3-D Animation Data
    /// </summary>
    public class Animation
    {
        /// <summary>
        /// Gets or Sets the name of the animation
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the transform space
        /// </summary>
        public AnimationTransformSpace TransformSpace { get; set; }

        /// <summary>
        /// Gets or Sets the transform type
        /// </summary>
        public AnimationTransformType TransformType { get; set; }

        /// <summary>
        /// Gets or Sets the framerate
        /// </summary>
        public float Framerate { get; set; }

        /// <summary>
        /// Gets or Sets the bones
        /// </summary>
        public List<AnimationBone> Bones { get; set; }

        /// <summary>
        /// Gets or Sets the notifications
        /// </summary>
        public List<AnimationNotification> Notifications { get; set; }

        /// <summary>
        /// Gets whether the animation contains translation keys in any of the bones
        /// </summary>
        public bool ContainsTranslationKeys
        {
            get
            {
                foreach (var bone in Bones)
                    if (bone.TranslationFrames.Count > 0)
                        return true;

                return false;
            }
        }

        /// <summary>
        /// Gets whether the animation contains rotation keys in any of the bones
        /// </summary>
        public bool ContainsRotationKeys
        {
            get
            {
                foreach (var bone in Bones)
                    if (bone.RotationFrames.Count > 0)
                        return true;

                return false;
            }
        }

        /// <summary>
        /// Gets whether the animation contains scale keys in any of the bones
        /// </summary>
        public bool ContainsScaleKeys
        {
            get
            {
                foreach (var bone in Bones)
                    if (bone.ScaleFrames.Count > 0)
                        return true;

                return false;
            }
        }

        public Animation()
        {
            Bones = new List<AnimationBone>();
            Notifications = new List<AnimationNotification>();
        }

        public Animation(string name)
        {
            Name = name;
            Framerate = 30;
            TransformType = AnimationTransformType.Relative;
            Bones = new List<AnimationBone>();
            Notifications = new List<AnimationNotification>();
        }

        public AnimationBone GetBone(string name) => Bones.Find(x => x.Name == name);

        public AnimationBone CreateBone(string name) => CreateBone(name, AnimationTransformType.Parent);

        public AnimationBone CreateBone(string name, AnimationTransformType type)
        {
            var bone = Bones.Find(x => x.Name == name);

            if (bone == null)
            {
                bone = new AnimationBone(name, type);
                Bones.Add(bone);
            }

            return bone;
        }

        public AnimationNotification CreateNotification(string name)
        {
            var notification = Notifications.Find(x => x.Name == name);

            if(notification == null)
            {
                notification = new AnimationNotification(name);
                Notifications.Add(notification);
            }

            return notification;
        }

        public float GetAnimationFrameCount()
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (var bone in Bones)
            {
                foreach (var frame in bone.TranslationFrames)
                {
                    min = Math.Min(min, frame.Time);
                    max = Math.Max(max, frame.Time);
                }
                foreach (var frame in bone.RotationFrames)
                {
                    min = Math.Min(min, frame.Time);
                    max = Math.Max(max, frame.Time);
                }
                foreach (var frame in bone.ScaleFrames)
                {
                    min = Math.Min(min, frame.Time);
                    max = Math.Max(max, frame.Time);
                }
            }

            return max + 1;
        }

        public float GetAnimationLength()
        {
            return GetAnimationFrameCount() / Framerate;
        }

        public void ScaleTranslations(float scale)
        {
            foreach (var bone in Bones)
            {
                for (int i = 0; i < bone.TranslationFrames.Count; i++)
                {
                    bone.TranslationFrames[i] = new(bone.TranslationFrames[i].Time, bone.TranslationFrames[i].Data * scale);
                }
            }
        }

        public int GetNotificationFrameCount()
        {
            int result = 0;

            foreach (var notification in Notifications)
                result += notification.Frames.Count;

            return result;
        }
    }
}
