using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    public struct AnimationFrame<T>
    {
        /// <summary>
        /// Gets 
        /// </summary>
        public float Time { get; set; }

        public T Data { get; set; }

        public AnimationFrame(float time, T data)
        {
            Time = time;
            Data = data;
        }
    }
}
