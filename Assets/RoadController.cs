using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Helper;
using static UnityEditor.Progress;

public class RoadController : MonoBehaviour
{
    const string _buildingTag = "building";
    const string _obstacleTag = "obstacle";

    List<GameObject> Buildings;
    List<GameObject> Obstacles;

    void Start()
    {
        Buildings = GameObject.FindGameObjectsWithTag(_buildingTag).ToList();
        Obstacles = GameObject.FindGameObjectsWithTag(_obstacleTag).ToList();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        var buildings = GameObject.FindGameObjectsWithTag(_buildingTag).Select(o => o.transform.position).ToList();
        foreach (var node in buildings)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(node, 3.0f);
        }

        var pairs = DeterminePairsTheWackyWay(buildings);

        ColorHelper.SetColors(pairs.Count);
        Gizmos.color = new Color(255, 0, 126);

        for (var i = 0; i < pairs.Count; i++)
        {
            var node = pairs.Keys.Skip(i).Take(1).Single();

            var color = ColorHelper.colors[i];

            var line = new Vector3[] { node.Item1, node.Item2 };
            RaycastHit hit;

            GizmoHelper.paths = new List<Vector3[]>();

            if (!Physics.Raycast(line[0], line[1] - line[0], out hit))
            {
                Gizmos.color = color;
                Gizmos.DrawLine(node.Item1, node.Item2);
            }
            else
            {
                GizmoHelper.DrawLineViaTangentsToTarget(hit, node.Item1, node.Item2, color);
            }
        }

        var obstacles = GameObject.FindGameObjectsWithTag(_obstacleTag).ToList();

        Gizmos.color = Color.red;
        foreach (GameObject node in obstacles)
        {
            Gizmos.DrawWireSphere(node.transform.position, 3.0f);
        }
    }

    private Dictionary<(Vector3, Vector3), object> DeterminePairsTheWackyWay(List<Vector3> nodes)
    {
        var pairs = new Dictionary<(Vector3, Vector3), object>();
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                // beetje wacky maar is goed voor nu
                var pair = nodes[i].Equals(nodes[j])
                    ? (nodes[i], nodes[j])
                    : (nodes[j], nodes[i]);

                pairs[pair] = null; // dan maar zo
            }
        }

        // hey het werkt
        return pairs;

    }
}
