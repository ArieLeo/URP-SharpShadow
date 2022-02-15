using System;
using Object = UnityEngine.Object;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace MalyaWka.SharpShadow.Utils
{
    public static class VectorExtensions
    {
        public static bool MatchesExact(this Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
        }

        public static Vector3 NormalizeExact(this Vector3 v)
        {
            float length = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            if (length == 0.0f)
            {
                return Vector3.zero;
            }
            return v * (1.0f / length);
        }
    }
    
    public class MeshCreatorActive
    {
        protected static int[] a = new int[3];
        protected static int[] b = new int[3];
        
        protected struct Edge
        {
            public Vector3 a;
            public Vector3 b;
            public float cellSize;

            public Edge(Vector3 a, Vector3 b, float cellSize)
            {
                this.a = a;
                this.b = b;
                this.cellSize = cellSize;
            }

            public bool Same(Edge other)
            {
                return a.MatchesExact(other.a) && b.MatchesExact(other.b) || a.MatchesExact(other.b) && b.MatchesExact(other.a);
            }

            public int CalculateHashCode()
            {
                int hashA = (int)(a.x / cellSize) * 73856093 ^ (int)(a.y / cellSize) * 19349663 ^ (int)(a.z / cellSize) * 83492791;
                int hashB = (int)(b.x / cellSize) * 73856093 ^ (int)(b.y / cellSize) * 19349663 ^ (int)(b.z / cellSize) * 83492791;
                
                int min, max;
                if (hashA < hashB)
                {
                    min = hashA;
                    max = hashB;
                }
                else
                {
                    min = hashB;
                    max = hashA;
                }
                return min ^ max;
            }
        }

        protected struct EdgeEqualityComparer : IEqualityComparer<Edge>
        {
            public bool Equals(Edge x, Edge y)
            {
                return x.Same(y);
            }

            public int GetHashCode(Edge obj)
            {
                return obj.CalculateHashCode();
            }
        }
        
        protected static void AddEdge(IDictionary<Edge, List<int>> edges, Edge edge, int triangleIndex)
        {
            if (!edges.ContainsKey(edge))
            {
                var triangles = new List<int>();
                triangles.Add(triangleIndex);
                edges.Add(edge, triangles);
            }
            else
            {
                var triangles = edges[edge];
                triangles.Add(triangleIndex);
            }
        }
        
        protected static bool NeighborSameWindingOrder(Vector3[] vertices, int[] indices, int triangleA, int triangleB)
        {
            a[0] = indices[triangleA * 3 + 0];
            a[1] = indices[triangleA * 3 + 1];
            a[2] = indices[triangleA * 3 + 2];

            b[0] = indices[triangleB * 3 + 0];
            b[1] = indices[triangleB * 3 + 1];
            b[2] = indices[triangleB * 3 + 2];

            for (int m = 0; m < 3; m++)
            {
                int a0 = a[m];
                int a1 = a[(m + 1) % 3];

                for (int n = 0; n < 3; n++)
                {
                    int b0 = b[n];
                    int b1 = b[(n + 1) % 3];
                    
                    if (vertices[a0].MatchesExact(vertices[b1]) && vertices[a1].MatchesExact(vertices[b0]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected static void CreateDegenerateQuad(Vector3[] vertices, int[] indices, Vector3 vertexA, Vector3 vertexB, int triangleA, int triangleB, ICollection<int> outIndices)
        {
            a[0] = indices[triangleA * 3 + 0];
            a[1] = indices[triangleA * 3 + 1];
            a[2] = indices[triangleA * 3 + 2];

            b[0] = indices[triangleB * 3 + 0];
            b[1] = indices[triangleB * 3 + 1];
            b[2] = indices[triangleB * 3 + 2];

            for (int m = 0; m < 3; m++)
            {
                int a0 = a[m];
                int a1 = a[(m + 1) % 3];

                for (int n = 0; n < 3; n++)
                {
                    int b0 = b[n];
                    int b1 = b[(n + 1) % 3];
                    
                    if (vertices[a0].MatchesExact(vertices[b1]) && vertices[a1].MatchesExact(vertices[b0]))
                    {
                        if (vertices[a0].MatchesExact(vertexA) && vertices[a1].MatchesExact(vertexB) ||
                            vertices[a0].MatchesExact(vertexB) && vertices[a1].MatchesExact(vertexA))
                        {
                            outIndices.Add(a0);
                            outIndices.Add(b1);
                            outIndices.Add(a1);

                            outIndices.Add(a1);
                            outIndices.Add(b1);
                            outIndices.Add(b0);

                            return;
                        }
                    }
                }
            }

            Debug.LogError("Could not create degenerate quad!");
        }
        
        public static Mesh CreateMesh(Mesh reference, float boundsPadFactor = 0f)
        {
            bool isReadable = reference.isReadable;

            if (!isReadable)
            {
                Debug.Log($"'{reference.name}' is not readable, and will be instantiated...");
                reference = Object.Instantiate(reference);
            }
            
            Vector3 refBoundsSize = reference.bounds.size;
            Vector3[] refVertices = reference.vertices;
            BoneWeight[] refBoneWeights = reference.boneWeights;
            int[] refIndices = reference.triangles;
            
            int refTriangleCount = refIndices.Length / 3;
            
            Vector3[] vertices = new Vector3[refIndices.Length];
            Vector3[] normals = new Vector3[refIndices.Length];
            BoneWeight[] boneWeights = refBoneWeights.Length > 0 ? new BoneWeight[refIndices.Length] : null;
            int[] indices = new int[refIndices.Length];
            
            for (int i = 0; i < refIndices.Length; i++)
            {
                vertices[i] = refVertices[refIndices[i]];
                indices[i] = i;
            }
            
            for (int i = 0; i < refTriangleCount; i++)
            {
                int index0 = i * 3 + 0;
                int index1 = i * 3 + 1;
                int index2 = i * 3 + 2;

                var normal = Vector3.Cross(vertices[index1] - vertices[index0], vertices[index2] - vertices[index0]);

                normal.NormalizeExact();

                normals[index0] = normal;
                normals[index1] = normal;
                normals[index2] = normal;
            }
            
            if (boneWeights != null)
            {
                for (int i = 0; i < refIndices.Length; i++)
                {
                    boneWeights[i] = refBoneWeights[refIndices[i]];
                }
            }
            
            var cellSize = Mathf.Max(refBoundsSize.x, refBoundsSize.y, refBoundsSize.z) * 0.001f;
            var edges = new Dictionary<Edge, List<int>>(new EdgeEqualityComparer());

            for (int i = 0; i < refTriangleCount; i++)
            {
                var t0 = vertices[i * 3 + 0];
                var t1 = vertices[i * 3 + 1];
                var t2 = vertices[i * 3 + 2];

                AddEdge(edges, new Edge(t0, t1, cellSize), i);
                AddEdge(edges, new Edge(t1, t2, cellSize), i);
                AddEdge(edges, new Edge(t2, t0, cellSize), i);
            }
            
            bool validTwoManifold = true;

            foreach (var edge in edges.Keys)
            {
                var triangles = edges[edge];

                if (triangles.Count != 2 || !NeighborSameWindingOrder(vertices, indices, triangles[0], triangles[1]))
                {
                    validTwoManifold = false;
                    break;
                }
            }

            if (!validTwoManifold)
            {
                int vertexOffset = vertices.Length;
                int triangleOffset = refTriangleCount;

                var newVertices = new Vector3[vertices.Length * 2];
                vertices.CopyTo(newVertices, 0);
                vertices.CopyTo(newVertices, vertexOffset);
                
                var newNormals = new Vector3[vertices.Length * 2];
                normals.CopyTo(newNormals, 0);

                for (int i = 0; i < normals.Length; i++)
                {
                    newNormals[vertexOffset + i] = -normals[i];
                }
                
                var newBoneWeights = boneWeights != null ? new BoneWeight[vertices.Length * 2] : null;
                if (boneWeights != null)
                {
                    boneWeights.CopyTo(newBoneWeights, 0);
                    boneWeights.CopyTo(newBoneWeights, vertexOffset);
                }
                
                var newIndices = new int[vertices.Length * 2];
                indices.CopyTo(newIndices, 0);

                for (int i = 0; i < refTriangleCount; i++)
                {
                    int index0 = i * 3 + 0;
                    int index1 = i * 3 + 1;
                    int index2 = i * 3 + 2;

                    newIndices[vertexOffset + index0] = vertexOffset + index0;
                    newIndices[vertexOffset + index1] = vertexOffset + index2;
                    newIndices[vertexOffset + index2] = vertexOffset + index1;
                }
                
                var finalIndices = new List<int>(newIndices);

                foreach (var edge in edges.Keys)
                {
                    var triangles = edges[edge];
                    
                    if (triangles.Count == 2 && NeighborSameWindingOrder(newVertices, newIndices, triangles[0], triangles[1]))
                    {
                        CreateDegenerateQuad(newVertices, newIndices, edge.a, edge.b, triangles[0], triangles[1], finalIndices);
                        CreateDegenerateQuad(newVertices, newIndices, edge.a, edge.b, triangleOffset + triangles[0], triangleOffset + triangles[1], finalIndices);
                    }
                    else
                    {
                        for (int i = 0; i < triangles.Count; i++)
                        {
                            CreateDegenerateQuad(newVertices, newIndices, edge.a, edge.b, triangles[i], triangleOffset + triangles[i], finalIndices);
                        }
                    }
                }

                vertices = newVertices;
                normals = newNormals;
                boneWeights = newBoneWeights;
                indices = finalIndices.ToArray();
            }
            else
            {
                var finalIndices = new List<int>(indices);

                foreach (var edge in edges.Keys)
                {
                    var triangles = edges[edge];
                    if (triangles.Count == 2)
                    {
                        CreateDegenerateQuad(vertices, indices, edge.a, edge.b, triangles[0], triangles[1], finalIndices);
                    }
                }

                indices = finalIndices.ToArray();
            }
            
            Mesh result = new Mesh();
            if (boneWeights != null)
            {
                result.bindposes = reference.bindposes;
            }

            bool thirtyTwoBit = vertices.Length >= 65536;

            result.Clear();
            result.colors32 = null;
            result.tangents = null;
            result.uv = null;
            result.uv2 = null;
            result.uv3 = null;
            result.uv4 = null;
            result.uv5 = null;
            result.uv6 = null;
            result.uv7 = null;
            result.uv8 = null;
            
            result.name = $"{reference.name.ToLower().Replace("(clone)", String.Empty)}_active_mesh";
            
            result.indexFormat = thirtyTwoBit ? IndexFormat.UInt32 : IndexFormat.UInt16;
            result.vertices = vertices;
            result.normals = normals;

            if (boneWeights != null)
            {
                result.boneWeights = boneWeights;
            }

            result.triangles = indices;
            result.RecalculateBounds();

            if (!Mathf.Approximately(boundsPadFactor, 0f))
            {
                var bounds = result.bounds;
                bounds.Expand(bounds.size.magnitude * boundsPadFactor);
                result.bounds = bounds;
            }

            if (!isReadable)
            {
                Debug.Log($"'{reference.name}' is instantiated, and will be destroyed...");
                Object.DestroyImmediate(reference);
            }
            return result;
        }
    }
}
