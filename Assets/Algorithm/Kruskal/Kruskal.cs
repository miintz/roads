using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Algorithm.Kruskal
{
    public class Kruskal
    {
        private List<Vector3> _nodes { get; set; }
        private bool _debug = false;

        public Kruskal(List<Vector3> nodes, bool debug = false)
        {
            _nodes = nodes;
            _debug = debug;
        }

        public List<KruskalEdge> GetMinimumSpanningTree()
        {
            var n = _nodes.Count();
            var edges = new List<KruskalEdge>();

            // Step 1: Create edges with their weights (distances)
            for (int i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    var distance = Vector3.Distance(_nodes[i], _nodes[j]);
                    edges.Add(new KruskalEdge(i, j, distance));
                }
            }

            // Step 2: Sort edges by weight (distance)
            edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

            // Step 3: Initialize the disjoint set (Union-Find)
            var ds = new KruskalDisjointSet(n);
            var mst = new List<KruskalEdge>();

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

            if (_debug)
            {
                Gizmos.color = Color.magenta;
                foreach (var edge in mst)
                {
                    Gizmos.DrawLine(_nodes[edge.StartNode], _nodes[edge.EndNode]);
                }
            }

            return mst;
        }  

    }
}
