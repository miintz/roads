using System;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Helper
{
    public static class GraphHelper
    {
        public static List<Edge> KruskalMST(List<Vector3> nodes)
        {
            int n = nodes.Count;
            List<Edge> edges = new List<Edge>();

            // Step 1: Create edges with their weights (distances)
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    float distance = EuclideanDistance(nodes[i], nodes[j]);
                    edges.Add(new Edge(i, j, distance));
                }
            }

            // Step 2: Sort edges by weight (distance)
            edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

            // Step 3: Initialize the disjoint set (Union-Find)
            DisjointSet ds = new DisjointSet(n);
            List<Edge> mst = new List<Edge>();

            // Step 4: Process edges in order, adding them to the MST if they don't form a cycle
            foreach (var edge in edges)
            {
                int rootStart = ds.Find(edge.StartNode);
                int rootEnd = ds.Find(edge.EndNode);

                if (rootStart != rootEnd)
                {
                    mst.Add(edge);
                    ds.Union(rootStart, rootEnd);
                }
            }

            return mst;
        }

        // Class to represent an edge in the graph
        public class Edge
        {
            public int StartNode { get; set; }
            public int EndNode { get; set; }
            public float Weight { get; set; }

            public Edge(int startNode, int endNode, float weight)
            {
                StartNode = startNode;
                EndNode = endNode;
                Weight = weight;
            }
        }

        // Class to represent the Union-Find structure
        public class DisjointSet
        {
            private int[] parent;
            private int[] rank;

            public DisjointSet(int size)
            {
                parent = new int[size];
                rank = new int[size];
                for (int i = 0; i < size; i++)
                    parent[i] = i;
            }

            // Find the root of the set containing 'x' (with path compression)
            public int Find(int x)
            {
                if (parent[x] != x)
                    parent[x] = Find(parent[x]);
                return parent[x];
            }

            // Union by rank
            public void Union(int x, int y)
            {
                int rootX = Find(x);
                int rootY = Find(y);

                if (rootX != rootY)
                {
                    if (rank[rootX] > rank[rootY])
                        parent[rootY] = rootX;
                    else if (rank[rootX] < rank[rootY])
                        parent[rootX] = rootY;
                    else
                    {
                        parent[rootY] = rootX;
                        rank[rootX]++;
                    }
                }
            }
        }

        // Calculate Euclidean distance between two Vector3 points
        public static float EuclideanDistance(Vector3 p1, Vector3 p2)
        {
            return Vector3.Distance(p1, p2);  // Direct use of Unity's built-in distance function
        }
    }
}
