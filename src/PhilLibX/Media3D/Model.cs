using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold a 3-D Model
    /// </summary>
    public class Model
    {
        /// <summary>
        /// Gets or Sets the name of the Model
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the path of the Model if applicable
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or Sets the Model Meshes
        /// </summary>
        public List<Mesh> Meshes { get; set; }

        /// <summary>
        /// Gets or Sets the Model Bones
        /// </summary>
        public List<ModelBone> Bones { get; set; }

        /// <summary>
        /// Gets or Sets the Model Materials
        /// </summary>
        public List<Material> Materials { get; set; }

        public Model()
        {
            Bones = new List<ModelBone>();
            Meshes = new List<Mesh>();
            Materials = new List<Material>();
        }

        public Model(string name) : this()
        {
            Name = name;
        }

        public Model(string name, string filePath) : this()
        {
            Name = name;
            FilePath = filePath;
        }

        /// <summary>
        /// Gets the bone by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>Bone if found, otherwise null</returns>
        public ModelBone GetBone(string boneName) => Bones.Find(x => x.Name == boneName);

        /// <summary>
        /// Gets the bone index by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>Index of the bone if found, otherwise -1</returns>
        public int GetBoneIndex(string boneName) => Bones.FindIndex(x => x.Name == boneName);

        /// <summary>
        /// Attempts to get the bone by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>True of the bone if found, otherwise false</returns>
        public bool TryGetBone(string boneName, out ModelBone bone) => (bone = Bones.Find(x => x.Name == boneName)) != null;

        /// <summary>
        /// Attempts to get the bone index by name
        /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>True of the bone if found, otherwise false</returns>
        public bool TryGetBoneIndex(string boneName, out int boneIndex) => (boneIndex = Bones.FindIndex(x => x.Name == boneName)) != -1;

        /// <summary>
        /// Determines whether the model contains the bone
        /// /// </summary>
        /// <param name="boneName">Name of the bone</param>
        /// <returns>True of the bone if found, otherwise false</returns>
        public bool ContainsBone(string boneName) => Bones.FindIndex(x => x.Name == boneName) != -1;

        public enum InfluenceDiscard
        {
            SmallestInflunce,
            FurthestInfluence,
        }

        public void SetMaxInfluences(int maxInfluences, InfluenceDiscard discardMode)
        {
        }

        public void Scale(float scale)
        {
            //if(scale != 1 && scale != 0)
            //{
            //    foreach (var bone in Bones)
            //    {
            //        var locMat = bone.LocalTransform;
            //        locMat.Translation *= scale;
            //        bone.LocalTransform = locMat;
            //        var worldMat = bone.WorldTransform;
            //        worldMat.Translation *= scale;
            //        bone.WorldTransform = worldMat;
            //    }

            //    foreach (var mesh in Meshes)
            //    {
            //        for (int i = 0; i < mesh.Positions.Count; i++)
            //        {
            //            mesh.Positions[i] *= scale;
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Enumerates the bones depth first from the root nodes
        /// </summary>
        /// <returns>Current bone</returns>
        public IEnumerable<ModelBone> EnumerateBones()
        {
            var boneStack = new Stack<ModelBone>();

            // Push roots first
            foreach (var bone in Bones)
                if (bone.Parent == null)
                    boneStack.Push(bone);

            while(boneStack.Count > 0)
            {
                var currentBone = boneStack.Pop();

                yield return currentBone;

                foreach (var bone in currentBone.Children)
                    boneStack.Push(bone);
            }
        }

        public void GenerateWorldTransforms()
        {
            foreach (var bone in EnumerateBones())
            {
                bone.GenerateWorldTransform();
            }
        }

        public (Vector3, Vector3) GetMinMax()
        {
            Vector3 min = new(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new(float.MinValue, float.MinValue, float.MinValue);

            foreach (var mesh in Meshes)
            {
                var (meshMin, meshMax) = mesh.GetMinMax();

                if (meshMax.X > max.X)
                    max.X = meshMax.X;
                if (meshMax.Y > max.Y)
                    max.Y = meshMax.Y;
                if (meshMax.Z > max.Z)
                    max.Z = meshMax.Z;

                if (meshMin.X < min.X)
                    min.X = meshMin.X;
                if (meshMin.Y < min.Y)
                    min.Y = meshMin.Y;
                if (meshMin.Z < min.Z)
                    min.Z = meshMin.Z;
            }

            return (min, max);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Name;
    }
}
