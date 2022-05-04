using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold a Model Mesh
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// Gets or Sets the Positions
        /// </summary>
        public VertexAttributeCollection<Vector3> Positions { get; set; }

        /// <summary>
        /// Gets or Sets the Normals
        /// </summary>
        public VertexAttributeCollection<Vector3> Normals { get; set; }

        /// <summary>
        /// Gets or Sets the BiTangents
        /// </summary>
        public VertexAttributeCollection<Vector3> BiTangents { get; set; }

        /// <summary>
        /// Gets or Sets the Tangents
        /// </summary>
        public VertexAttributeCollection<Vector3> Tangents { get; set; }

        /// <summary>
        /// Gets or Sets the Colours
        /// </summary>
        public VertexAttributeCollection<Vector4> Colours { get; set; }

        /// <summary>
        /// Gets or Sets the UV Layers
        /// </summary>
        public VertexAttributeCollection<Vector2> UVs { get; set; }

        /// <summary>
        /// Gets or Sets the Bone Weights
        /// </summary>
        public VertexAttributeCollection<(int, float)> BoneWeights { get; set; }

        /// <summary>
        /// Gets or Sets the Materials assigned to this mesh
        /// </summary>
        public List<Material> Materials { get; set; }

        /// <summary>
        /// Gets or Sets the Polygon Face Indices
        /// </summary>
        public List<(int, int, int)> Faces { get; set; }

        /// <summary>
        /// Initializes a new instance of a <see cref="Mesh"/>
        /// </summary>
        public Mesh() { }

        /// <summary>
        /// Initializes a new instance of a <see cref="Mesh"/> with the provided data
        /// </summary>
        /// <param name="vertexCount">Initial vertex count</param>
        /// <param name="faceCount">Initial face count</param>
        public Mesh(int vertexCount, int faceCount)
        {
            Positions   = new(vertexCount);
            Normals     = new(vertexCount);
            Tangents    = new(vertexCount);
            BiTangents  = new(vertexCount);
            Colours     = new(vertexCount);
            UVs         = new(vertexCount, 1);
            BoneWeights = new(vertexCount, 8);

            Faces       = new(faceCount);
            Materials = new List<Material>();
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="Mesh"/> with the provided data
        /// </summary>
        /// <param name="vertexCount">Initial vertex count</param>
        /// <param name="faceCount">Initial face count</param>
        public Mesh(int vertexCount, int faceCount, int uvLayers)
        {
            Positions  = new(vertexCount);
            Normals    = new(vertexCount);
            Tangents   = new(vertexCount);
            BiTangents = new(vertexCount);
            Colours    = new(vertexCount);

            if (uvLayers > 0)
                UVs = new(vertexCount, uvLayers);

            BoneWeights = new(vertexCount, 8);

            Faces = new(faceCount);
            Materials = new List<Material>();
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="Mesh"/> with the provided data
        /// </summary>
        /// <param name="vertexCount">Initial vertex count</param>
        /// <param name="faceCount">Initial face count</param>
        public Mesh(int vertexCount, int faceCount, int uvLayers, int influences)
        {
            Positions  = new(vertexCount);
            Normals    = new(vertexCount);
            Tangents   = new(vertexCount);
            BiTangents = new(vertexCount);
            Colours    = new(vertexCount);

            if (uvLayers > 0)
                UVs = new(vertexCount, uvLayers);
            if (influences > 0)
                BoneWeights = new(vertexCount, influences);

            Faces = new(faceCount);
            Materials = new List<Material>();
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="Mesh"/> with the provided data
        /// </summary>
        /// <param name="vertexCount">Initial vertex count</param>
        /// <param name="faceCount">Initial face count</param>
        public Mesh(int vertexCount, int faceCount, int uvLayers, int influences, bool hasNormals, bool hasTangents, bool hasBiTangents, bool hasColours)
        {
            Positions   = new(vertexCount);

            if(hasNormals)
                Normals     = new(vertexCount);
            if(hasTangents)
                Tangents    = new(vertexCount);
            if(hasBiTangents)
                BiTangents  = new(vertexCount);
            if(hasColours)
                Colours     = new(vertexCount);
            if(uvLayers > 0)
                UVs         = new(vertexCount, uvLayers);
            if(influences > 0)
                BoneWeights = new(vertexCount, influences);

            Faces = new(faceCount);
            Materials = new List<Material>();
        }

        /// <summary>
        /// Normalizes the weights and removes any duplicates
        /// </summary>
        public void NormalizeWeights()
        {
            var nWeights = new VertexAttributeCollection<(int, float)>(BoneWeights.VertexCount, BoneWeights.Dimension);

            for (int v = 0; v < nWeights.VertexCount; v++)
            {

            }
        }

        public (Vector3, Vector3) GetMinMax()
        {
            Vector3 min = new(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new(float.MinValue, float.MinValue, float.MinValue);

            foreach (var position in Positions)
            {
                if (position.X > max.X)
                    max.X = position.X;
                if (position.Y > max.Y)
                    max.Y = position.Y;
                if (position.Z > max.Z)
                    max.Z = position.Z;

                if (position.X < min.X)
                    min.X = position.X;
                if (position.Y < min.Y)
                    min.Y = position.Y;
                if (position.Z < min.Z)
                    min.Z = position.Z;
            }

            return (min, max);
        }
    }
}
