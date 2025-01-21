using UnityEngine;

namespace Assembly_CSharp
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class TerrainMeshSimulator : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int x = 120; // Number of cells horizontally
        public int z = 60; // Number of cells vertically
        public float yVariability = 2;
        public int seed = 1;
        public float cellSize = 1.0f; // Size of each cell
        public float yOffset = 4f;
        public bool gizmos = true;

        private Mesh mesh;
        private MeshCollider meshCollider;

        private void OnEnable()
        {
            GenerateMesh();
        }

        private void OnValidate()
        {
            // Regenerate the mesh when parameters change
            GenerateMesh();
        }

        private void GenerateMesh()
        {
            var rand = new System.Random(seed);

            if (meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();
            }

            // Initialize the mesh if it doesn't exist
            if (mesh == null)
            {
                mesh = new Mesh();
                GetComponent<MeshFilter>().mesh = mesh;
            }

            // Generate vertices
            int numVertices = (x + 1) * (z + 1);
            Vector3[] vertices = new Vector3[numVertices];
            int[] triangles = new int[x * z * 6]; // 2 triangles per cell, 3 indices per triangle

            for (int y = 0; y <= this.z; y++)
            {
                for (int x = 0; x <= this.x; x++)
                {
                    int index = y * (this.x + 1) + x;
                    vertices[index] = new Vector3(x * cellSize, (yVariability * (float)rand.NextDouble()) - yOffset, y * cellSize);
                }
            }

            // Generate triangles
            int t = 0;

            for (int y = 0; y < this.z; y++)
            {
                for (int x = 0; x < this.x; x++)
                {
                    int vertexIndex = y * (this.x + 1) + x;

                    // First triangle (bottom-left to top-left to bottom-right)
                    triangles[t++] = vertexIndex;
                    triangles[t++] = vertexIndex + this.x + 1;
                    triangles[t++] = vertexIndex + 1;

                    // Second triangle (bottom-right to top-left to top-right)
                    triangles[t++] = vertexIndex + 1;
                    triangles[t++] = vertexIndex + this.x + 1;
                    triangles[t++] = vertexIndex + this.x + 2;
                }
            }

            // Assign to mesh
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals(); 
            mesh.RecalculateBounds();

            // meshCollider updaten
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        private void OnDrawGizmos()
        {
            if (!gizmos)
            {
                return;
            }

            Gizmos.color = Color.green;

            // Draw all vertices
            foreach (Vector3 vertex in mesh.vertices)
            {
                //Gizmos.DrawSphere(transform.TransformPoint(vertex), 0.1f);
            }
        }     
    }
}