using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Helper;
using Assets.Algorithm.Kruskal;
using Assets.Algorithm;

public class RoadController : MonoBehaviour
{
    List<GameObject> Buildings;
    List<GameObject> Obstacles;

    // dit werkt, maar is destructief (wel interessant verder)
    //public bool DeformTerrainAlongPath = false;

    public List<string> nodeLabels = new List<string> { "building" };
    public List<string> obstacleLabels = new List<string> { "obstacle" };
    //public List<string> terrainLabel = "terrain";

    public bool drawMSTGizmos = false;
    public bool drawTangentPathGizmos = false;
    public bool debugTangents = false;

    public bool checkTerrainGradienWhenPathfinding = true;

    // 0.75 seems to be the limit before the tangent pathfinder runs into issues with slope gradient (i think)
    [Range(0.75f, 2.0f)]
    public float maxPathHeightDifferential = 1.0f;

    [Range(7.5f, 45f)]
    public float acceptableTerrainSlope = 30f;

    public int rakeLimit = 15;

    public bool drawSplineGizmos = true;
    
    /// <summary>
    /// Bepaald de resolutie van de Catmull-Rom spline die ervoor zorgt dat de boel een beetje smooth is.
    /// 
    /// Hoe hoger deze waarde, hoe meer segmenten je krijgt. 10 lijkt goed te werken
    /// </summary>
    public float SplineResolution = 10.0f;

    public bool debugRaycast = false;

    // todo: dit werkt niet, als je dit vermedigvuldigd met een normal, die optelt bij een start vector3 roteer je de vector3 op basis van dit nummer
    public int debugRaycastLength = 1000;

    private Kruskal _kruskal;

    void Start()
    {
        Buildings = nodeLabels.SelectMany(l => GameObject.FindGameObjectsWithTag(l)).ToList();
        Obstacles = obstacleLabels.SelectMany(l => GameObject.FindGameObjectsWithTag(l)).ToList();
        //TerrainVertices = GameObject.FindGameObjectsWithTag(_terrainTag).SelectMany(g => g.GetComponent<MeshFilter>()?.sharedMesh.vertices).ToList();

        _kruskal = new Kruskal(Buildings.Select(g => g.transform.position).ToList());
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: hier moet ik de boel nu gaan inbakken
    }

    private void OnDrawGizmos()
    {
        #region TODO: dit hele stuk moet ik later even herzien, kan ik gebruiken voor terrain deformation
        // TODO: dit hele stuk moet ik later even herzien, kan ik gebruiken voor terrain deformation

        //var terrainVertices = new List<Vector3>();

        //// oogabooga
        //var terrainData = GameObject.FindGameObjectsWithTag(_terrainTag).Select(a => a.GetComponent<Terrain>().terrainData).ToList().First();
        //var heightmapWidth = terrainData.heightmapResolution;
        //var heightmapHeight = terrainData.heightmapResolution;

        //// Get heightmap data
        //var heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

        //// TODO: dit stuk zou nog wel eens een behoorlijke performance bottleneck kunnen zijn, maar dat zien we vanzelf :)
        //var terrainSize = terrainData.size;

        //for (var y = 0; y < heightmapHeight; y++)
        //{
        //    for (var x = 0; x < heightmapWidth; x++)
        //    {
        //        var worldX = (x / heightmapWidth - 1) * terrainSize.x;
        //        var worldY = heights[y, x] * terrainSize.y;
        //        var worldZ = (y / heightmapHeight - 1) * terrainSize.z;

        //        var vertexPosition = new Vector3(worldX, worldY, worldZ);

        //        terrainVertices.Add(vertexPosition);
        //    }
        //}

        #endregion

        var nodes = nodeLabels.SelectMany(l => GameObject.FindGameObjectsWithTag(l).ToList()).Select(o => o.transform.position).ToList();

        foreach (var node in nodes)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(node, 2.0f);
        }

        var kruskal = new Kruskal(nodes, drawMSTGizmos);
        var mst = kruskal.GetMinimumSpanningTree();

        // dit zou moeten werken... door de MST zou als het goed is de richting niet uit moeten maken.
        var pairs = mst.Select(a => new Vector3[] 
        { 
            nodes[a.EndNode], 
            nodes[a.StartNode] 
        }).ToList();

        ColorHelper.SetColors(pairs.Count);
        Gizmos.color = new Color(255, 0, 126);

        var pathfinders = new List<TangentPathfinder>();

        for (var i = 0; i < pairs.Count; i++)
        {
            var node = pairs[i];
            var color = ColorHelper.colors[i];

            // initialize pathfinder
            pathfinders.Add(new TangentPathfinder(
                node[0], 
                node[1], 
                nodeLabels,
                rakeLimit: rakeLimit,
                debug: debugTangents, 
                checkTerrainGradientWhenPathing: checkTerrainGradienWhenPathfinding,
                acceptableTerrainSlope: acceptableTerrainSlope,
                maxPathHeightDifferential: maxPathHeightDifferential));
        }

        var obstacles = nodeLabels.SelectMany(l => GameObject.FindGameObjectsWithTag(l).ToList()).ToList();

        Gizmos.color = Color.red;
        foreach (GameObject node in obstacles)
        {
            Gizmos.DrawWireSphere(node.transform.position, 3.0f);
        }

        // catmull-rom to the rescue
        // doen we voor elk """segment"""
        List<CatmullRom> _splines = new List<CatmullRom>();

        foreach (var pather in pathfinders)
        {
            var basicPath = pather.GetPath();

            if (drawTangentPathGizmos)
            {
                for (var i = 0; i < basicPath.Count - 1; i++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(basicPath[i], basicPath[i + 1]);
                }
            }

            var catmullrom = new CatmullRom(basicPath);

            _splines.Add(catmullrom);
        }

        var smoothPaths = _splines.Select(s => s.GetSpline(SplineResolution));

        if (!drawSplineGizmos)
            return;

        if (drawSplineGizmos)
        {
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
}
