using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold an animation notification
    /// </summary>
    public class AnimationNotification
    {
        /// <summary>
        /// Gets or Sets the Name of the Notetrack
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the Notetrack Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets the notification frames
        /// </summary>
        public List<AnimationFrame<Action>> Frames { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationNotification"/> class
        /// </summary>
        /// <param name="name">Name of the notification</param>
        public AnimationNotification(string name) : this(name, "Basic") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationNotification"/> class
        /// </summary>
        /// <param name="name">Name of the notification</param>
        /// <param name="type">Notification type</param>
        public AnimationNotification(string name, string type)
        {
            Name = name;
            Type = type;

            Frames = new List<AnimationFrame<Action>>();
        }
    }
}
