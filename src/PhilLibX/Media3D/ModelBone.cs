using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold a 3-D Bone
    /// </summary>
    [DebuggerDisplay("Name = {Name}")]
    public class ModelBone
    {
        /// <summary>
        /// Bone Parent
        /// </summary>
        public ModelBone InternalParent;

        /// <summary>
        /// Gets or Sets the name of the Bone
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the Parent Bone
        /// </summary>
        public ModelBone Parent
        {
            get
            {
                return InternalParent;
            }
            set
            {
                // Remove from previous bone
                InternalParent?.Children.Remove(this);
                InternalParent = value;
                InternalParent?.Children.Add(this);
            }
        }

        /// <summary>
        /// Gets or Sets the Child Bones
        /// </summary>
        public List<ModelBone> Children { get; set; }

        /// <summary>
        /// Gets or Sets the local bone position
        /// </summary>
        public Vector3 LocalTranslation { get; set; }

        /// <summary>
        /// Gets or Sets the local bone rotation
        /// </summary>
        public Quaternion LocalRotation { get; set; }

        /// <summary>
        /// Gets or Sets the world bone scale
        /// </summary>
        public Vector3 LocalScale { get; set; }

        /// <summary>
        /// Gets or Sets the world bone position
        /// </summary>
        public Vector3 WorldTranslation { get; set; }

        /// <summary>
        /// Gets or Sets the world bone rotation
        /// </summary>
        public Quaternion WorldRotation { get; set; }

        /// <summary>
        /// Gets or Sets the world bone scale
        /// </summary>
        public Vector3 WorldScale { get; set; }

        /// <summary>
        /// Gets or Sets the Bone Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets whether this bone is a root bone or not (has no Parent)
        /// </summary>
        public bool IsRoot => Parent == null;

        /// <summary>
        /// Initializes a new instance of a <see cref="ModelBone"/>
        /// </summary>
        public ModelBone()
        {
            Children = new List<ModelBone>();
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="ModelBone"/> with the provided data
        /// </summary>
        /// <param name="name">Bone Name</param>
        /// <param name="parent">Parent Bone</param>
        /// <param name="local">Transform relative to parent</param>
        /// <param name="world">Absolute Transform</param>
        public ModelBone(string name, ModelBone parent = null)
        {
            Children = new List<ModelBone>();
            Name = name;
            Parent = parent;
            //LocalTransform = local ?? Matrix4x4.Identity;
            //WorldTransform = world ?? Matrix4x4.Identity;

            //// Compute if one or other was provided from parent
            //if (local == null)
            //{
            //    GenerateLocalTransform();
            //}
            //else if (world == null)
            //{

            //}
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="ModelBone"/> with the provided data
        /// </summary>
        /// <param name="name">Bone Name</param>
        /// <param name="localPos">Position</param>
        /// <param name="localQuat">Rotation</param>
        /// <param name="parent">Parent Bone</param>
        public ModelBone(string name, Vector3 pos, Quaternion rot, bool worldSpace, ModelBone parent = null)
        {
            Children = new List<ModelBone>();
            Name = name;
            Parent = parent;

            if(!worldSpace)
            {
                LocalTranslation = pos;
                LocalRotation = rot;

                GenerateWorldTransform();
            }
            else
            {
                WorldTranslation = pos;
                WorldRotation = rot;

                GenerateLocalTransform();
            }
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="ModelBone"/> with the provided data
        /// </summary>
        /// <param name="name">Bone Name</param>
        /// <param name="localPos">Position relative to parent</param>
        /// <param name="localQuat">Rotation relative to parent</param>
        /// <param name="worldPos">Absolute position</param>
        /// <param name="worldQuat">Absolute rotation</param>
        /// <param name="parent">Parent Bone</param>
        public ModelBone(string name, Vector3 localPos, Quaternion localQuat, Vector3 worldPos, Quaternion worldQuat, ModelBone parent = null)
        {
            Children = new List<ModelBone>();
            Name     = name;
            Parent   = parent;

            LocalTranslation = localPos;
            LocalRotation    = localQuat;
            WorldTranslation = worldPos;
            WorldRotation    = worldQuat;
        }

        /// <summary>
        /// Creates a copy of this bone
        /// </summary>
        /// <returns>New copy of this bone</returns>
        // public ModelBone CreateCopy() => new(Name, null, LocalTransform, WorldTransform);

        /// <summary>
        /// Checks if this bone is a descendant of the given bone by name
        /// </summary>
        /// <param name="boneName">Parent to check for</param>
        /// <returns>True if it is, otherwise false</returns>
        public bool IsDescendantOf(ModelBone parent)
        {
            var current = Parent;

            while (current is not null)
            {
                if (current == parent)
                    return true;

                current = current.Parent;
            }

            return false;
        }

        /// <summary>
        /// Checks if this bone is a descendant of the given bone by name
        /// </summary>
        /// <param name="parentName">Parent name to check for</param>
        /// <returns>True if it is, otherwise false</returns>
        public bool IsDescendantOf(string parentName)
        {
            var current = Parent;

            while(current is not null)
            {
                if (current.Name.Equals(parentName))
                    return true;

                current = current.Parent;
            }

            return false;
        }

        /// <summary>
        /// Checks if this bone is a descendant of the given bone by regex
        /// </summary>
        /// <param name="parentRegex">Parent regex to attempt to match</param>
        /// <returns>True if it is, otherwise false</returns>
        public bool IsDescendantOf(Regex parentRegex)
        {
            var current = Parent;

            while (current != null)
            {
                if (parentRegex.IsMatch(current.Name))
                    return true;

                current = current.Parent;
            }

            return false;
        }

        public void SetWorldRotation(Quaternion rotation)
        {
            WorldRotation = rotation;

            GenerateLocalTransform();

            foreach (var child in Children)
            {
                child.GenerateWorldTransforms();
            }
        }

        public void GenerateLocalTransform()
        {
            if (Parent != null)
            {
                LocalRotation = Quaternion.Conjugate(Parent.WorldRotation) * WorldRotation;
                LocalTranslation = Vector3.Transform(WorldTranslation - Parent.WorldTranslation, Quaternion.Conjugate(Parent.WorldRotation));
            }
            else
            {
                LocalTranslation = WorldTranslation;
                LocalRotation = WorldRotation;
            }
        }

        public void GenerateWorldTransform()
        {
            if (Parent != null)
            {
                WorldRotation = Parent.WorldRotation * LocalRotation;
                WorldTranslation = Vector3.Transform(LocalTranslation, Parent.WorldRotation) + Parent.WorldTranslation;
            }
            else
            {
                WorldTranslation = LocalTranslation;
                WorldRotation = LocalRotation;
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

        public IEnumerable<ModelBone> EnumerateParents()
        {
            var parent = Parent;

            while(parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }
    }
}
