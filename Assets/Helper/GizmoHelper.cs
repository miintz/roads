using System.Collections.Generic;
using UnityEngine;

namespace Assets.Helper
{
    public static class GizmoHelper
    {
        public static List<Vector3[]> paths =  new List<Vector3[]>();

        public static void DrawLineViaTangentsToTarget(RaycastHit hit, Vector3 node, Vector3 node2)
        {
            // je bent een obstakel (dus we moeten tangents vinden...)
            var tangents = VectorHelper.FindTangents(hit.transform.position, node);
            if (tangents == null)
            {
                return;
            }

            var t1 = tangents[0];
            var t2 = tangents[1];

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(t1, 0.2f);
            Gizmos.DrawSphere(t2, 0.2f);

            Gizmos.color = new Color(255, 0, 126);

            // is via t1 of t2 sneller
            var t1d = Vector3.Distance(t1, hit.transform.position) + Vector3.Distance(t1, node2);
            var t2d = Vector3.Distance(t2, hit.transform.position) + Vector3.Distance(t2, node2);

            var tpos = t1d < t2d ? t1 : t2;

            // maar raakt de nieuwe lijn 1 van de al bestaande lijnen
            var intersect = VectorHelper.LineIntersectsWithAny(paths, new Vector3[] { node, tpos });
  
            // dit hele stuk klopt volgens mij geen zak van
            if (intersect == Vector3.zero)
            {
                // pad raakt niets
                Gizmos.DrawLine(node, tpos);
                paths.Add(new Vector3[] { node, tpos });

                // voor dat we het laatste deel van het pad doen, bepaal of nog een obstakel is
                if (Physics.Raycast(tpos, node2 - tpos, out RaycastHit newHit))
                {
                    DrawLineViaTangentsToTarget(newHit, tpos, node2); // en nog een keer
                }
                else
                {
                    Gizmos.DrawLine(tpos, node2); // laatste deel van het pad
                    paths.Add(new Vector3[] { tpos, node2 });
                }
            }
            else
            {
                // intersect? blijkbaar?            
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(tpos, 0.4f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(intersect, 0.4f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(tpos, intersect);

                // vanaf hier moeten we raycasten naar het eindpunt (toch even proberen)
                if (Physics.Raycast(tpos, node2 - tpos, out hit))
                {
                    // recursie van tpos (de laatste tangent) naar de positie van de collider van het obstakel
                    DrawLineViaTangentsToTarget(hit, tpos, node2);                    
                }
                else
                {
                    // blijkbaar zijn we er al...                
                    Gizmos.DrawLine(tpos, node2);
                    paths.Add(new Vector3[] { tpos, node2 });
                }               
            }
        }     
    }
}
