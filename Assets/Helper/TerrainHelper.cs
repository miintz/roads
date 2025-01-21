// TODO: dit werkt op grote lijnen, maar is destructief op de asset. 
// even behouden voor nu:)

//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public static class TerrainHelper
//{
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
//}