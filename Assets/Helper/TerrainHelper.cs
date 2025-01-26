// TODO: dit werkt op grote lijnen, maar is destructief op de asset. 
// even behouden voor nu:)

using UnityEngine;

public class TerrainHelper
{
    public TerrainHelper()
    {
        
    }

    //    public static void DeformTerrainAlongPath(List<Vector3> path, int radius = 5, float deformationAmount = 0.01f)
    //    {
    //        var terrain = Terrain.activeTerrain;
    //        var terrainData = terrain.terrainData;

    //        var terrainPos = terrain.transform.position;

    //        // Iterate through each point on the path
    //        foreach (Vector3 pathPoint in path)
    //        {
    //            // Convert world position to terrain heightmap coordinates
    //            var relativeX = (pathPoint.x - terrainPos.x) / terrainData.size.x;
    //            var relativeZ = (pathPoint.z - terrainPos.z) / terrainData.size.z;

    //            var centerX = Mathf.FloorToInt(relativeX * terrainData.heightmapResolution);
    //            var centerZ = Mathf.FloorToInt(relativeZ * terrainData.heightmapResolution);

    //            // Get the current heights for the deformation area
    //            var diameter = radius * 2 + 1;
    //            var heights = terrainData.GetHeights(
    //                Mathf.Max(0, centerX - radius),
    //                Mathf.Max(0, centerZ - radius),
    //                Mathf.Min(diameter, terrainData.heightmapResolution - centerX),
    //                Mathf.Min(diameter, terrainData.heightmapResolution - centerZ)
    //            );

    //            // Deform the terrain within the radius
    //            for (int x = -radius; x <= radius; x++)
    //            {
    //                for (int z = -radius; z <= radius; z++)
    //                {
    //                    int px = centerX + x;
    //                    int pz = centerZ + z;

    //                    if (px >= 0 && px < terrainData.heightmapResolution &&
    //                        pz >= 0 && pz < terrainData.heightmapResolution)
    //                    {
    //                        float distance = Mathf.Sqrt(x * x + z * z);
    //                        if (distance <= radius)
    //                        {
    //                            float factor = 1f - (distance / radius); // Smooth falloff
    //                            heights[z + radius, x + radius] -= deformationAmount * factor;
    //                        }
    //                    }
    //                }
    //            }

    //            // Apply the modified heights back to the terrain
    //            terrainData.SetHeights(
    //                Mathf.Max(0, centerX - radius),
    //                Mathf.Max(0, centerZ - radius),
    //                heights
    //            );
    //        }
    //    }


    /// <summary>
    /// Determine if the slope <paramref name="tangent"/> is on is deemed acceptable by <paramref name="maxSlopeAngle"/>.
    /// </summary>
    /// <param name="tangent"></param>
    /// <param name="obstacleCenter"></param>
    /// <param name="maxSlopeAngle"></param>
    /// <returns>Return true if the slope the tangent is on is deemed acceptaple.</returns>
    public bool IsOnAcceptableTerrainSlope(Vector3 tangent, Vector3 obstacleCenter, float maxSlopeAngle)
    {
        // terrain normal = the angle.
        Vector3 terrainNormal = GetTerrainNormalAtPoint(tangent);
        float slopeAngle = Vector3.Angle(Vector3.up, terrainNormal);

        return slopeAngle <= maxSlopeAngle;
    }

    /// <summary>
    /// Try to move a <see cref="Vector3"> to match the terrain
    /// </summary>
    /// <param name="tangentPoint"><see cref="Vector3"/> to adjust</param>
    /// <returns>Adjusted <see cref="Vector3"/> object.</returns>
    public Vector3 AdjustVectorToTerrain(Vector3 tangentPoint)
    {
        // TODO: dit zal niet lekker werken als er een object toevallig onder / boven de tangent zit anders dan het terrein

        var deathFromAbove = new Ray(new Vector3(tangentPoint.x, 10000, tangentPoint.z), Vector3.down); // werp een lijn van hoog over
        if (Physics.Raycast(deathFromAbove, out RaycastHit hitFromAbove))
        {
            return hitFromAbove.point + new Vector3(0, 0.7f, 0); // dit zou het terrein moeten zijn
        }

        // niks van boven, probeer van onder (als de tangent onder het terrein zit)
        var mears = new Ray(new Vector3(tangentPoint.x, -10000, tangentPoint.z), Vector3.up);
        if (Physics.Raycast(mears, out RaycastHit hitFromBelow))
        {
            return hitFromBelow.point + Vector3.up; // dit zou het terrein moeten zijn...
        }

        Debug.LogWarning("wait what now");

        return tangentPoint; // TODO: en nu? tangent helemaal buiten het terrein??
    }

    private Vector3 GetTerrainNormalAtPoint(Vector3 point)
    {
        var ray = new Ray(new Vector3(point.x, 1000, point.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.normal;
        }

        Debug.LogWarning("Default valid slope used. Intended?");
        return Vector3.up; // TODO: ff zien wat te doen hier, want dit zal dus technisch gezien een valide slope zijn
    }
}