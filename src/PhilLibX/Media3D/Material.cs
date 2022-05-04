using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D
{
    public class Material
    {
        /// <summary>
        /// Gets or Sets the name of the material
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the material type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets the images assigned to this material by name
        /// </summary>
        public Dictionary<string, string> Images { get; set; }

        /// <summary>
        /// Gets or Sets the material settings
        /// </summary>
        public Dictionary<string, object> Settings { get; set; }

        public Material()
        {
            Images = new Dictionary<string, string>();
        }

        public Material(string name)
        {
            Name = name;
            Images = new Dictionary<string, string>();
        }

        public Material(string name, string type)
        {
            Name = name;
            Images = new Dictionary<string, string>();
        }
    }
}
