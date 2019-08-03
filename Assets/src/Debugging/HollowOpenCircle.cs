using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Debugging
{
    public class HollowOpenCircle : IDebugShape
    {
        public Vector2 Center { get; private set; }
        public float InnerRadius { get; private set; }
        public float OuterRadius { get; private set; }
        public float Fill { get; private set; }
        public Color Color { get; private set; }

        public HollowOpenCircle(Vector2 center, float innerRadius, float outerRadius, float fill, Color color)
        {
            Center = center;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            Fill = fill;
            Color = color;
        }

        public void Render(Camera camera, CommandBuffer buffer, Material material)
        {
            var mesh = GetMesh(InnerRadius, OuterRadius, Fill);
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_Color", Color);

            var mat = Matrix4x4.Translate(Center)
                * Matrix4x4.Scale(new Vector3(InnerRadius, InnerRadius, InnerRadius));

            buffer.DrawMesh(mesh, mat, material, 0, 0, properties);
        }

        private static Mesh GetMesh(float innerRadius, float outerRadius, float fill)
        {
            var resolution = 32;
            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            var indices = new List<int>();

            var angleStep = (360.0f * Mathf.Clamp01(fill)) / (float)resolution;
            var quaternion = Quaternion.Euler(0.0f, 0.0f, angleStep);

            var inner = new Vector3(0f, innerRadius, 0f);
            var outer = new Vector3(0f, outerRadius, 0f);

            vertices.Add(inner);
            vertices.Add(outer);

            int index = 0;
            for (int i = 0; i < resolution - 1; i++)
            {
                index = vertices.Count - 2;

                inner = quaternion * inner;
                outer = quaternion * outer;

                vertices.Add(inner);
                vertices.Add(outer);

                indices.Add(index);
                indices.Add(index + 2);
                indices.Add(index + 1);

                indices.Add(index + 2);
                indices.Add(index + 3);
                indices.Add(index + 1);
            }

            index = vertices.Count - 2;

            if (fill >= 1f - Mathf.Epsilon)
            {
                indices.Add(0);
                indices.Add(1);
                indices.Add(index);

                indices.Add(1);
                indices.Add(index + 1);
                indices.Add(index);
            }

            mesh.vertices = vertices.ToArray();
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0, true);
            mesh.SetTriangles(indices.ToArray(), 0, true);

            return mesh;
        }
    }
}
