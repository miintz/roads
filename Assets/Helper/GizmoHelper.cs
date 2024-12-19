using System.Collections.Generic;
using UnityEngine;

namespace Assets.Helper
{
    public static class GizmoHelper
    {
        public static List<Vector3[]> paths =  new List<Vector3[]>();

        public static void DrawLineViaTangentsToTarget(RaycastHit hit, Vector3 node, Vector3 node2, Color color)
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

            Gizmos.color = color;

            // is via t1 of t2 sneller
            var t1d = Vector3.Distance(t1, hit.transform.position) + Vector3.Distance(t1, node2);
            var t2d = Vector3.Distance(t2, hit.transform.position) + Vector3.Distance(t2, node2);

            var tpos = t1d < t2d ? t1 : t2;

            // maar raakt de nieuwe lijn 1 van de al bestaande lijnen
            var intersect = VectorHelper.LineIntersectsWithAny(paths, new Vector3[] { node, tpos });
              
            if (intersect == Vector3.zero)
            {
                // pad raakt niets
                Gizmos.DrawLine(node, tpos);
                paths.Add(new Vector3[] { node, tpos });

                // voor dat we het laatste deel van het pad doen, bepaal of nog een obstakel is
                if (Physics.Raycast(tpos, node2 - tpos, out RaycastHit newHit))
                {
                    DrawLineViaTangentsToTarget(newHit, tpos, node2, color); // en nog een keer
                }
                else
                {
                    Gizmos.DrawLine(tpos, node2); // laatste deel van het pad
                    paths.Add(new Vector3[] { tpos, node2 });
                }
            }
            else // dit hele stuk klopt volgens mij geen zak van, hoezo hebben we een intersect??
            {
                // intersect? blijkbaar?            
                // TODO: er dus blijkbaar een intersect hier
                // TODO: volgens mij worden lijnen nog steeds beide kanten opgetekend, maar dan zou de dinges inf moeten zijn... of niet?
                Gizmos.color = color;
                Gizmos.DrawCube(tpos, new Vector3(1f, 1f, 1f));
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(intersect, new Vector3(1f, 1f, 1f));

                Gizmos.color = color;
                Gizmos.DrawLine(tpos, intersect);

                // vanaf hier moeten we raycasten naar het eindpunt (toch even proberen)
                if (Physics.Raycast(tpos, node2 - tpos, out hit))
                {
                    // recursie van tpos (de laatste tangent) naar de positie van de collider van het obstakel
                    DrawLineViaTangentsToTarget(hit, tpos, node2, color);                    
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
