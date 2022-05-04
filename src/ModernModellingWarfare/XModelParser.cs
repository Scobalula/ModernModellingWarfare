using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ModernModellingWarfare.Assets;
using ModernModellingWarfare.Shared;
using PhilLibX;
using PhilLibX.Media3D;
using PhilLibX.Media3D.FileTranslators;

namespace ModernModellingWarfare
{
    internal unsafe static class XModelParser
    {

        /// <summary>
        /// The translator 
        /// </summary>
        public static SEModelTranslator Translator => new();

        internal static void ConvertLOD(Zone zone, XModelHandler.XModel* xmodel, XModelHandler.XModelLodInfo lod, int lodIndex = 0)
        {
            if (lod.Surfaces == null)
                return;

            var xmodelName = new string(xmodel->Name);

            //if (xmodelName != "mp_western_vm_arms_fireteam_west_1_1")
            //    return;

            var dir = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "exported_files", "xmodels", xmodelName);
            Directory.CreateDirectory(dir);

            Printer.WriteLine("XMODEL", $"Exporting LOD: {lodIndex} for XModel: {xmodelName}");

            var model = new Model();
            var uniqueMaterials = new HashSet<string>();
            Span<byte> boneCounts = stackalloc byte[8]
            {
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7
            };

            for (int i = 0, p = 0; i < (xmodel->NumBones + xmodel->UnkBoneCount); i++)
            {
                if (i < xmodel->NumRootBones)
                {
                    model.Bones.Add(new(
                        zone.AssetList->StringList.GetByIndex(xmodel->BoneNames[i]),
                        new Vector3(0, 0, 0),
                        new Quaternion(0, 0, 0, 1),
                        new Vector3(0, 0, 0),
                        new Quaternion(0, 0, 0, 1)));
                }
                else
                {
                    var r = xmodel->Rotations[p].Unpacked;
                    var t = xmodel->Translations[p] * 2.54f;
                    var m = xmodel->BaseMatrices[i];

                    model.Bones.Add(new(
                        zone.AssetList->StringList.GetByIndex(xmodel->BoneNames[i]),
                        new Vector3(m.Trans.X * 2.54f, m.Trans.Y * 2.54f, m.Trans.Z * 2.54f),
                        new Quaternion(m.Quat.X, m.Quat.Y, m.Quat.Z, m.Quat.W),
                        new Vector3(t.X, t.Y, t.Z),
                        new Quaternion(r.X, r.Y, r.Z, r.W),
                        model.Bones[i - xmodel->ParentList[p]]));

                    p++;
                }
            }

            for (int i = 0; i < lod.SurfaceCount; i++)
            {
                var xsurface = lod.Surfaces->Surfaces[lod.SurfIndex + i];

                // Compute Bounds
                var bounds = new Vector4(
                    xsurface.XOffset,
                    xsurface.YOffset,
                    xsurface.ZOffset,
                    Math.Max(Math.Max(xsurface.Min, xsurface.Scale), xsurface.Max));


                var positions = (ulong*)(xsurface.MeshBuffer->Buffer + xsurface.VerticesOffset);
                var uvs       = (GfxPackedUV*)(xsurface.MeshBuffer->Buffer + xsurface.UVsOffset);
                var normals   = (GfxQTangent*)(xsurface.MeshBuffer->Buffer + xsurface.NormalsOffset);
                var faces     = (GfxStreamFace*)(xsurface.MeshBuffer->Buffer + xsurface.TriOffset);

                var mesh = new Mesh(xsurface.VertexCount, xsurface.FaceCount);

                var mtl = new Material()
                {
                    Name = Path.GetFileNameWithoutExtension(new string(xmodel->MaterialHandles[lod.SurfIndex + i]->Name))
                };

                Global.VerbosePrint($"Exporting Mesh: {i}");
                Global.VerbosePrint($"Material: {mtl.Name}");
                Global.VerbosePrint($"Vertices: {xsurface.VertexCount}");
                Global.VerbosePrint($"Faces: {xsurface.FaceCount}");
                Global.VerbosePrint($"Skinned: {xsurface.BlendWeightsSize != 0}");


                MaterialCacheHandler.TryExportMaterialImages(mtl, $"{dir}\\_images");

                model.Materials.Add(mtl);
                mesh.Materials.Add(mtl);

                uniqueMaterials.Add(mtl.Name);

                for (int v = 0; v < xsurface.VertexCount; v++)
                {
                    var (t, b, n) = normals[v].Unpack();

                    mesh.Positions.Add(new Vector3(
                            ((((((positions[v] >> 00) & 0x1FFFFF) * 0.00000047683739f * 2.0f) - 1.0f) * bounds.W) + bounds.X) * 2.54f,
                            ((((((positions[v] >> 21) & 0x1FFFFF) * 0.00000047683739f * 2.0f) - 1.0f) * bounds.W) + bounds.Y) * 2.54f,
                            ((((((positions[v] >> 42) & 0x1FFFFF) * 0.00000047683739f * 2.0f) - 1.0f) * bounds.W) + bounds.Z) * 2.54f));
                    mesh.Tangents.Add(t);
                    mesh.BiTangents.Add(b);
                    mesh.Normals.Add(n);
                    mesh.UVs.Add(new Vector2((float)uvs[v].UVX, (float)uvs[v].UVY));
                    mesh.Colours.Add(Vector4.One);
                }

                for (int v = 0; v < xsurface.FaceCount; v++)
                {
                    var packedIndex = faces[v];
                    mesh.Faces.Add((packedIndex.Index0, packedIndex.Index1, packedIndex.Index2));
                }


                // Check for rigid weights
                if (xsurface.RigidWeights != null)
                {
                    var vertexIndex = 0;

                    for (int rigidIndex = 0; rigidIndex < xsurface.RigidWeightCount; rigidIndex++)
                    {
                        var xrigidVertList = xsurface.RigidWeights[rigidIndex];

                        for (int v = 0; v < xrigidVertList.VertexCount; v++)
                        {
                            mesh.BoneWeights.Add(new(xrigidVertList.BoneIndex, 1.0f), vertexIndex++);
                        }
                    }
                }

                ushort* blendWeights = xsurface.BlendWeights;

                var vertexOffset = 0;

                if (xsurface.BlendWeights != null)
                {
                    for (int blendIndex = 0; blendIndex < 8; blendIndex++)
                    {
                        for (int boneIndex = 0; boneIndex < blendIndex + 1; ++boneIndex)
                        {
                            for (int vertexIndex = vertexOffset; vertexIndex < (vertexOffset + xsurface.BlendVertCounts[blendIndex]); vertexIndex++)
                            {
                                int index = blendWeights[0];
                                float weight = 1.0f;

                                if (boneIndex > 0)
                                {
                                    weight = blendWeights[1] / 65536.0f;
                                    mesh.BoneWeights[vertexIndex, 0] = new(mesh.BoneWeights[vertexIndex, 0].Item1, mesh.BoneWeights[vertexIndex, 0].Item2 - weight);
                                }

                                mesh.BoneWeights.Add((index, weight), vertexIndex);
                                blendWeights += 2;
                            }
                        }

                        vertexOffset += xsurface.BlendVertCounts[blendIndex];
                    }
                }

                model.Meshes.Add(mesh);
            }

            using var stream = File.Create($"{dir}\\{xmodelName}_lod{lodIndex}.semodel");

            Translator.Write(stream, xmodelName, model, 1.0f, new());

            File.WriteAllLines($"{dir}\\{xmodelName}_lod{lodIndex}_materials.txt", uniqueMaterials);
            File.WriteAllText($"{dir}\\{xmodelName}_lod{lodIndex}_materials_search.txt", string.Join(",", uniqueMaterials));
        }
    }
}
