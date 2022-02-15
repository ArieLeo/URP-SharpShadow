using System;
using Object = UnityEngine.Object;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace MalyaWka.SharpShadow.Utils
{
    public class MeshCreatorStatic
    {
        protected static long TLMS = 1000000L;
        protected static long TLMS2 = 1000000L * 1000000L;
        
        protected struct Triangle
        {
            public Vector3 p0;
            public Vector3 p1;
            public Vector3 p2;
            public Vector3 n;
            public bool valid;
        }
        
        protected struct TriangleAndLine
        {
            public int id;

            public int i0;
            public int i1;
            public int i2;

            public Vector3 v0;
            public Vector3 v1;
        }

        protected struct Line
        {
            public Vector3 p0;
            public Vector3 p1;
        }
        
        protected static void Swap(int[] list, int i, int j)
        {
            (list[i], list[j]) = (list[j], list[i]);
        }
        
        protected static Vector3 Vector3Normalize(Vector3 n)
        {
            float m = n.magnitude;
            n /= m;
            return n;
        }
        
        protected static bool LineEqual(Line a, Line b)
        {
            return (a.p0 == b.p0 && a.p1 == b.p1) || (a.p0 == b.p1 && a.p1 == b.p0);
        }
        
        protected static bool TrianglesCanBeSimplified(List<TriangleAndLine> trisLines, Dictionary<long, int> trisLinesMap, int vi0, int vi1, int vi2, int vi3, int vi4, int vi5)
        {
            TriangleAndLine me = trisLines[trisLinesMap[vi0 * TLMS2 + vi1 * TLMS + vi2]];
            int meIndex = me.id; //1;
            TriangleAndLine other = trisLines[trisLinesMap[vi3 * TLMS2 + vi4 * TLMS + vi5]];
            int otherIndex = other.id; //1;
            int length = trisLines.Count;

            if (meIndex != -1 && otherIndex != -1)
            {
                if (me.id != other.id)
                {
                    Line lineMe = new Line() { p0 = me.v0, p1 = me.v1 };
                    Line lineOther = new Line() { p0 = other.v0, p1 = other.v1 };
                    if (LineEqual(lineMe, lineOther))
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            Debug.LogError("Error...");
            return false;
        }
        
        protected static Vector3 PointHitOnGround(Vector3 p, Transform transform, Vector3 invOLightDir, float ground, float offset)
        {
            Matrix4x4 l2w = transform.localToWorldMatrix;
            Vector3 pWorld = l2w.MultiplyPoint(transform.position + p);
            if (pWorld.y <= ground)
            {
                ground += pWorld.y;
            }
            float opposite = pWorld.y - ground;
            float cosTheta = -invOLightDir.y;
            float hypotenuse = opposite / cosTheta;
            Vector3 endPoint = p + invOLightDir * hypotenuse - invOLightDir * offset;
            return endPoint;
        }
        
        public static Mesh CreateMesh(Mesh reference, Transform transform, Light light, float ground, float offset, bool reverse)
        {
            bool isReadable = reference.isReadable;

            if (!isReadable)
            {
                Debug.Log($"'{reference.name}' is not readable, and will be instantiated...");
                reference = Object.Instantiate(reference);
            }

            Matrix4x4 w2l = transform.worldToLocalMatrix;
            Vector3 wLightDir = light.transform.forward;
            Vector3 oLightDir = w2l.MultiplyVector(wLightDir).normalized;
            Vector3 invOLightDir = -oLightDir;

            Vector3[] meshVertices = reference.vertices;
            int[] meshTriangles = reverse ? reference.triangles.Reverse().ToArray() : reference.triangles;

            Vector3[] newVertices = null;
            int[] newTriangles = null;
            int[] newTrianglesCaps = null;

            Triangle[] trianglesData = null;
            Triangle[] trianglesGroundData = null;
            List<TriangleAndLine> trisLines = new List<TriangleAndLine>();
            Dictionary<long, int> trisLinesMap = new Dictionary<long, int>();
            
            trianglesGroundData = new Triangle[meshTriangles.Length / 3];
            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                Vector3 p0 = meshVertices[meshTriangles[i + 0]];
                Vector3 p1 = meshVertices[meshTriangles[i + 1]];
                Vector3 p2 = meshVertices[meshTriangles[i + 2]];

                Vector3 n = Vector3.Cross(p1 - p0, p2 - p0);
                n = Vector3Normalize(n);
                bool valid = Vector3.Dot(n, invOLightDir) < 0; //TODO will > 0

                trianglesGroundData[i / 3] = new Triangle
                {
                    p0 = PointHitOnGround(p0, transform, invOLightDir, ground, offset), 
                    p1 = PointHitOnGround(p1, transform, invOLightDir, ground, offset), 
                    p2 = PointHitOnGround(p2, transform, invOLightDir, ground, offset),
                    n = n, 
                    valid = valid
                };
                if (Mathf.Approximately(n.magnitude, 0.0f))
                {
                    Debug.LogError("Error: Normal is Zero.");
                }
            }
            
            trianglesData = new Triangle[meshTriangles.Length / 3];

            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                Vector3 p0 = meshVertices[meshTriangles[i + 0]];
                Vector3 p1 = meshVertices[meshTriangles[i + 1]];
                Vector3 p2 = meshVertices[meshTriangles[i + 2]];

                Vector3 n = Vector3.Cross(p1 - p0, p2 - p0);
                n = Vector3Normalize(n);
                bool valid = Vector3.Dot(n, invOLightDir) < 0; //TODO will > 0

                trianglesData[i / 3] = new Triangle() { p0 = p0, p1 = p1, p2 = p2, n = n, valid = valid };
                if (Mathf.Approximately(n.magnitude, 0.0f))
                {
                    Debug.LogError("Error: Normal is Zero.");
                }
            }

            List<Triangle> validTriangles = new List<Triangle>();
            List<Triangle> validGroundTriangles = new List<Triangle>();
            for (int i = 0; i < trianglesData.Length; ++i)
            {

                Triangle triangle = trianglesData[i];
                Triangle groundTriangle = trianglesGroundData[i];
                if (!triangle.valid)
                {
                    validTriangles.Add(triangle);
                    validGroundTriangles.Add(groundTriangle);
                }
            }

            newVertices = new Vector3[validTriangles.Count * 3 * 2];
            newTriangles = new int[validTriangles.Count * 3 * 6];
            newTrianglesCaps = new int[validTriangles.Count * 3 * 2];
            int triLineId = 0;
            for (int i = 0; i < validTriangles.Count; ++i)
            {
                int vi0 = i * 6 + 0;
                int vi1 = i * 6 + 1;
                int vi2 = i * 6 + 2;
                int vi3 = i * 6 + 3;
                int vi4 = i * 6 + 4;
                int vi5 = i * 6 + 5;

                int tiIndex = 0;
                int[] tiList = null;
                {
                    int tstep = 18;
                    tiList = new int[tstep];
                    tiIndex = 0;
                    for (tiIndex = 0; tiIndex < tstep; ++tiIndex)
                    {
                        tiList[tiIndex] = i * tstep + tiIndex;
                    }
                }
                int[] triList_caps = null;
                {
                    int tstep = 6;
                    triList_caps = new int[tstep];
                    tiIndex = 0;
                    for (tiIndex = 0; tiIndex < tstep; ++tiIndex)
                    {
                        triList_caps[tiIndex] = i * tstep + tiIndex;
                    }
                }

                Triangle groundTriangle = validGroundTriangles[i];
                Triangle triangle = validTriangles[i];
                
                //TODO Control light direction!
                newVertices[vi0] = triangle.p0 + oLightDir * offset;
                newVertices[vi1] = triangle.p1 + oLightDir * offset;
                newVertices[vi2] = triangle.p2 + oLightDir * offset;

                newVertices[vi3] = groundTriangle.p0;
                newVertices[vi4] = groundTriangle.p1;
                newVertices[vi5] = groundTriangle.p2;

                tiIndex = 0;

                newTrianglesCaps[triList_caps[tiIndex++]] = vi0;
                newTrianglesCaps[triList_caps[tiIndex++]] = vi1;
                newTrianglesCaps[triList_caps[tiIndex++]] = vi2;

                newTrianglesCaps[triList_caps[tiIndex++]] = vi3;
                newTrianglesCaps[triList_caps[tiIndex++]] = vi4;
                newTrianglesCaps[triList_caps[tiIndex++]] = vi5;
                Swap(newTrianglesCaps, triList_caps[tiIndex - 1], triList_caps[tiIndex - 2]);

                tiIndex = 0;

                newTriangles[tiList[tiIndex++]] = vi0;
                newTriangles[tiList[tiIndex++]] = vi3;
                newTriangles[tiList[tiIndex++]] = vi4;
                newTriangles[tiList[tiIndex++]] = vi0;
                newTriangles[tiList[tiIndex++]] = vi4;
                newTriangles[tiList[tiIndex++]] = vi1;

                trisLines.Add(new TriangleAndLine() { i0 = vi0, i1 = vi3, i2 = vi4, v0 = newVertices[vi0], v1 = newVertices[vi1], id = ++triLineId });
                trisLinesMap.Add(vi0 * TLMS2 + vi3 * TLMS + vi4, trisLines.Count - 1);
                trisLines.Add(new TriangleAndLine() { i0 = vi0, i1 = vi4, i2 = vi1, v0 = newVertices[vi0], v1 = newVertices[vi1], id = triLineId });
                trisLinesMap.Add(vi0 * TLMS2 + vi4 * TLMS + vi1, trisLines.Count - 1);

                newTriangles[tiList[tiIndex++]] = vi1;
                newTriangles[tiList[tiIndex++]] = vi4;
                newTriangles[tiList[tiIndex++]] = vi5;
                newTriangles[tiList[tiIndex++]] = vi1;
                newTriangles[tiList[tiIndex++]] = vi5;
                newTriangles[tiList[tiIndex++]] = vi2;

                trisLines.Add(new TriangleAndLine() { i0 = vi1, i1 = vi4, i2 = vi5, v0 = newVertices[vi1], v1 = newVertices[vi2], id = ++triLineId });
                trisLinesMap.Add(vi1 * TLMS2 + vi4 * TLMS + vi5, trisLines.Count - 1);
                trisLines.Add(new TriangleAndLine() { i0 = vi1, i1 = vi5, i2 = vi2, v0 = newVertices[vi1], v1 = newVertices[vi2], id = triLineId });
                trisLinesMap.Add(vi1 * TLMS2 + vi5 * TLMS + vi2, trisLines.Count - 1);

                newTriangles[tiList[tiIndex++]] = vi2;
                newTriangles[tiList[tiIndex++]] = vi5;
                newTriangles[tiList[tiIndex++]] = vi3;
                newTriangles[tiList[tiIndex++]] = vi2;
                newTriangles[tiList[tiIndex++]] = vi3;
                newTriangles[tiList[tiIndex++]] = vi0;

                trisLines.Add(new TriangleAndLine() { i0 = vi2, i1 = vi5, i2 = vi3, v0 = newVertices[vi2], v1 = newVertices[vi0], id = ++triLineId });
                trisLinesMap.Add(vi2 * TLMS2 + vi5 * TLMS + vi3, trisLines.Count - 1);
                trisLines.Add(new TriangleAndLine() { i0 = vi2, i1 = vi3, i2 = vi0, v0 = newVertices[vi2], v1 = newVertices[vi0], id = triLineId });
                trisLinesMap.Add(vi2 * TLMS2 + vi3 * TLMS + vi0, trisLines.Count - 1);
            }
            
            List<int> trianglesList = new List<int>();
            int numTriangles = newTriangles.Length / 3;
            for (int i = 0; i < numTriangles; ++i)
            {
                int vi0 = newTriangles[i * 3 + 0];
                int vi1 = newTriangles[i * 3 + 1];
                int vi2 = newTriangles[i * 3 + 2];

                bool isDuplicated = false;

                for (int j = 0; j < numTriangles; ++j)
                {
                    if (i != j)
                    {
                        int vi3 = newTriangles[j * 3 + 0];
                        int vi4 = newTriangles[j * 3 + 1];
                        int vi5 = newTriangles[j * 3 + 2];
                        if (TrianglesCanBeSimplified(trisLines, trisLinesMap, vi0, vi1, vi2, vi3, vi4, vi5))
                        {
                            isDuplicated = true;
                            break;
                        }
                    }
                }
                if (!isDuplicated)
                {
                    trianglesList.Add(newTriangles[i * 3 + 0]);
                    trianglesList.Add(newTriangles[i * 3 + 1]);
                    trianglesList.Add(newTriangles[i * 3 + 2]);
                }
            }

            newTriangles = trianglesList.ToArray();
            
            int trianglesLength0 = 0;
            int[] triangles0 = null;
            {
                triangles0 = newTriangles;
                trianglesLength0 = triangles0.Length;
                int numVertices = newVertices.Length;
                for (int i = 0; i < trianglesLength0; ++i)
                {
                    Vector3 vi = newVertices[triangles0[i]];
                    for (int j = 0; j < numVertices; ++j)
                    {
                        Vector3 vj = newVertices[j];
                        if (vi == vj)
                        {
                            triangles0[i] = j;
                            break;
                        }
                    }
                }
            }

            int trianglesLength1 = 0;
            int[] triangles1 = null;
            {
                triangles1 = newTrianglesCaps;
                trianglesLength1 = triangles1.Length;
                int numVertices = newVertices.Length;
                for (int i = 0; i < trianglesLength1; ++i)
                {
                    Vector3 vi = newVertices[triangles1[i]];
                    for (int j = 0; j < numVertices; ++j)
                    {
                        Vector3 vj = newVertices[j];
                        if (vi == vj)
                        {
                            triangles1[i] = j;
                            break;
                        }
                    }
                }
            }

            List<Vector3> verticesList = new List<Vector3>(newVertices);
            for (int i = 0; i < verticesList.Count; ++i)
            {
                Vector3 vi = verticesList[i];
                for (int j = i; j < verticesList.Count; ++j)
                {
                    if (i != j)
                    {
                        Vector3 vj = verticesList[j];
                        if (vi == vj)
                        {
                            verticesList.RemoveAt(j);
                            for (int k = 0; k < trianglesLength0; ++k)
                            {
                                if (triangles0[k] >= j)
                                {
                                    --triangles0[k];
                                }
                            }
                            for (int k = 0; k < trianglesLength1; ++k)
                            {
                                if (triangles1[k] >= j)
                                {
                                    --triangles1[k];
                                }
                            }
                            --j;
                        }
                    }
                }
            }

            newVertices = verticesList.ToArray();
            newTriangles = triangles0;
            newTrianglesCaps = triangles1;

            Mesh result = new Mesh();
            
            bool thirtyTwoBit = newVertices.Length >= 65536;

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
            
            result.name = $"{reference.name.ToLower().Replace("(clone)", String.Empty)}" +
                          $"_static_mesh_h{transform.position.y}" +
                          $"_r{transform.rotation.eulerAngles.y}";

            int[] combinedTriangles = new int[newTriangles.Length + newTrianglesCaps.Length];
            Array.Copy(newTriangles, 0, combinedTriangles, 0, newTriangles.Length);
            Array.Copy(newTrianglesCaps, 0, combinedTriangles, newTriangles.Length, newTrianglesCaps.Length);
            
            result.indexFormat = thirtyTwoBit ? IndexFormat.UInt32 : IndexFormat.UInt16;
            result.subMeshCount = 1;
            result.vertices = newVertices;
            result.triangles = combinedTriangles;
            result.RecalculateBounds();

            if (!isReadable)
            {
                Debug.Log($"'{reference.name}' is instantiated, and will be destroyed...");
                Object.DestroyImmediate(reference);
            }

            return result;
        }
    }
}
