using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Assets.Helper;
using UnityEditor.Experimental.GraphView;

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
        var buildings = GameObject.FindGameObjectsWithTag(_buildingTag).ToList();
        var scratchList = GameObject.FindGameObjectsWithTag(_buildingTag).ToList();

        foreach (GameObject node in buildings)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(node.transform.position, 3.0f);

            Gizmos.color = new Color(255, 0, 126);
            foreach (GameObject node2 in scratchList.Where(n => n != node))
            {                
                var line = new Vector3[] { node.transform.position, node2.transform.position };
                RaycastHit hit;

                GizmoHelper.paths = new List<Vector3[]>();

                if (!Physics.Raycast(line[0], line[1] - line[0], out hit))
                {
                    Gizmos.DrawLine(node.transform.position, node2.transform.position);
                }
                else
                {
                    GizmoHelper.DrawLineViaTangentsToTarget(hit, node.transform.position, node2.transform.position);
                }
            }

            scratchList.Remove(node);
        }

        var obstacles = GameObject.FindGameObjectsWithTag(_obstacleTag).ToList();

        Gizmos.color = Color.red;
        foreach (GameObject node in obstacles)
        {
            Gizmos.DrawWireSphere(node.transform.position, 3.0f);
        }
    }    
}
