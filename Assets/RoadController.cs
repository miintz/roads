using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Helper;
using Assets.Algorithm.Kruskal;
using Assets.Algorithm;

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

    // dit werkt, maar is destructief (wel interessant verder)
    //public bool DeformTerrainAlongPath = false;
    
    public bool drawMstGizmos = true;
    public bool drawSplineGizmos = true;
    public bool debugTangents = false;

    private Kruskal _kruskal;

    void Start()
    {
        Buildings = GameObject.FindGameObjectsWithTag(_buildingTag).ToList();
        Obstacles = GameObject.FindGameObjectsWithTag(_obstacleTag).ToList();
        TerrainVertices = GameObject.FindGameObjectsWithTag(_terrainTag).SelectMany(g => g.GetComponent<MeshFilter>()?.sharedMesh.vertices).ToList();

        _kruskal = new Kruskal(Buildings.Select(g => g.transform.position).ToList());
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
        var terrainSize = terrainData.size;

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

        var nodes = GameObject.FindGameObjectsWithTag(_buildingTag).Select(o => o.transform.position).ToList();

        foreach (var node in nodes)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(node, 2.0f);
        }

        var kruskal = new Kruskal(nodes, drawMstGizmos);
        var mst = kruskal.GetMinimumSpanningTree();

        // is het wacky: ja. Maar het zou moeten werken (?)
        var pairs = mst.Select(a => new Vector3[] { nodes[a.EndNode], nodes[a.StartNode] }).ToList();

        ColorHelper.SetColors(pairs.Count);
        Gizmos.color = new Color(255, 0, 126);

        List<TangentPath> pathers = new List<TangentPath>();

        for (var i = 0; i < pairs.Count; i++)
        {
            var node = pairs[i];
            var color = ColorHelper.colors[i];

            pathers.Add(new TangentPath(node[0], node[1], debug: debugTangents));
        }

        var obstacles = GameObject.FindGameObjectsWithTag(_obstacleTag).ToList();

        Gizmos.color = Color.red;
        foreach (GameObject node in obstacles)
        {
            Gizmos.DrawWireSphere(node.transform.position, 3.0f);
        }

        // catmull-rom to the rescue
        // doen we voor elk """segment"""
        List<CatmullRom> _splines = new List<CatmullRom>();

        foreach (var pather in pathers)
        {            
            var catmullrom = new CatmullRom(pather.GetPath());

            _splines.Add(catmullrom);
        }

        var smoothPaths = _splines.Select(s => s.GetSpline());

        if (!drawSplineGizmos)
            return;

        Gizmos.color = Color.blue;
        foreach (var smoothPath in smoothPaths)
        {
            for (int i = 0; i < smoothPath.Count - 1; i++)
            {
                Gizmos.DrawLine(smoothPath[i], smoothPath[i + 1]);
            }
        }
    }
}
