using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Helper;

public class RoadController : MonoBehaviour
{
    const string _buildingTag = "building";
    const string _obstacleTag = "obstacle";
    const string _terrainTag = "terrain";

    List<GameObject> Buildings;
    List<GameObject> Obstacles;
    List<Vector3> TerrainVertices;

    /// <summary>
    /// Bepaald de resolutie van de Catmull-Rom spline die ervoor zorgt dat de boel een beetje smooth is.
    /// 
    /// Hoe hoger deze waarde, hoe meer segmenten je krijgt.
    /// </summary>
    public float SplineResolution = 10.0f;

    void Start()
    {
        Buildings = GameObject.FindGameObjectsWithTag(_buildingTag).ToList();
        Obstacles = GameObject.FindGameObjectsWithTag(_obstacleTag).ToList();
        TerrainVertices = GameObject.FindGameObjectsWithTag(_terrainTag).SelectMany(g => g.GetComponent<MeshFilter>()?.sharedMesh.vertices).ToList();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        var terrainVertices = new List<Vector3>();

        // oogabooga
        var terrainData = GameObject.FindGameObjectsWithTag(_terrainTag).Select(a => a.GetComponent<Terrain>().terrainData).ToList().First();
        var heightmapWidth = terrainData.heightmapResolution;
        var heightmapHeight = terrainData.heightmapResolution;

        // Get heightmap data
        var heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

        // TODO: dit stuk zou nog wel eens een behoorlijke performance bottleneck kunnen zijn
        Vector3 terrainSize = terrainData.size;
        
        for (var y = 0; y < heightmapHeight; y++)
        {
            for (var x = 0; x < heightmapWidth; x++)
            {                
                var worldX = (x / (float)(heightmapWidth - 1)) * terrainSize.x;
                var worldY = heights[y, x] * terrainSize.y;
                var worldZ = (y / (float)(heightmapHeight - 1)) * terrainSize.z;

                var vertexPosition = new Vector3(worldX, worldY, worldZ);

                terrainVertices.Add(vertexPosition);
            }
        }

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

            RoadHelper.paths = new List<Vector3[]>();

            if (!Physics.Raycast(line[0], line[1] - line[0], out hit))
            {
                Gizmos.color = color;
                Gizmos.DrawLine(node.Item1, node.Item2);
            }
            else
            {
                Gizmos.color = Color.magenta;
                //Gizmos.DrawSphere(hit.point, 0.5f);

                Gizmos.color = new Color(255, 0, 126);

                RoadHelper.ContinuePathViaTangentsToTarget(hit, node.Item1, node.Item2, color);
            }
        }

        var obstacles = GameObject.FindGameObjectsWithTag(_obstacleTag).ToList();

        Gizmos.color = Color.red;
        foreach (GameObject node in obstacles)
        {
            Gizmos.DrawWireSphere(node.transform.position, 3.0f);
        }

        // catmull-rom to the rescue
        var smoothPath = CornerHelper.CatmullRomSpline(RoadHelper.paths.SelectMany(a => a).Distinct().ToList(), SplineResolution);
        
        Gizmos.color = Color.blue;
        for (int i = 0; i < smoothPath.Count - 1; i++)
        {
            Gizmos.DrawLine(smoothPath[i], smoothPath[i + 1]);
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
