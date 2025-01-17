using System.Collections.Generic;
using UnityEngine;

namespace Assets.Helper
{
    public static class GizmoHelper
    {
        public static List<Vector3[]> paths = new List<Vector3[]>();

        public static void DrawLineViaTangentsToTarget(RaycastHit hit, Vector3 node, Vector3 node2, Color color)
        {
            // laten we zeggen: 30 graden is ok (nog steeds tyfus steil maar voor nu prima)
            var acceptableSlope = 30f;

            // zoek de tangents met een bepaalde radius voor nu, we kunnen die radius opkrikken als we geen geldige tangents vinden
            var t1acceptable = false;
            var t2acceptable = false;

            var t1 = Vector3.zero;
            var t2 = Vector3.zero;

            var r = 3f;
            var stepOnRake = 15;

            while (!t1acceptable && !t2acceptable) // loop de loop tot er 1 geldig is
            {
                var tangents = VectorHelper.FindTangents(hit.point, node, r);
                if (tangents == null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(hit.point, 0.5f);

                    // probeer nog eens maar negeer nu de radius
                    tangents = VectorHelper.FindTangents(hit.point, node, r, true);
                    if (tangents == null)
                    {
                        Debug.Log("still no tangents found");
                        return;
                    }
                }

                t1 = tangents[0];
                t2 = tangents[1];

                // de tangents moeten wel op het terrin liggen, niet erboven of eronder. 
                t1 = VectorHelper.AdjustVectorToTerrain(t1);
                t2 = VectorHelper.AdjustVectorToTerrain(t2);

                // nu zullen de tangents denk ik goed liggen (raycast op terrain + 1 omhoog voor de vorm), maar hoe is de slope? Normal checken dus denk ik?
                t1acceptable = VectorHelper.IsOnAcceptableTerrainSlope(t1, hit.point, acceptableSlope);
                t2acceptable = VectorHelper.IsOnAcceptableTerrainSlope(t2, hit.point, acceptableSlope);

                Gizmos.color = t1acceptable ? Color.yellow : Color.red;
                Gizmos.DrawSphere(t1, 0.2f + r / 25);

                Gizmos.color = t2acceptable ? Color.yellow : Color.red;
                Gizmos.DrawSphere(t2, 0.2f + r / 25);

                Gizmos.color = color;

                stepOnRake--;
                if (stepOnRake == 0)
                {
                    break;
                }

                // krik de tangent radius op
                r += 2f;
            }

            if (stepOnRake == 0)
            {
                // als we hier komen zijn er dus geen geldige tangents te vinden op deze oogabooga manier. 
                Debug.Log("stepped on rake. Intended?");

                return;
            }

            var tpos = Vector3.zero;
            if (t1acceptable && t2acceptable)
            {
                // is via t1 of t2 sneller
                var t1d = Vector3.Distance(t1, hit.point) + Vector3.Distance(t1, node2);
                var t2d = Vector3.Distance(t2, hit.point) + Vector3.Distance(t2, node2);

                tpos = t1d < t2d ? t1 : t2;
            }
            else
            {
                // als het goed is hebben 1 acceptabele tangent, dus dit zou moeten kunnen
                tpos = t1acceptable ? t1 : t2;
            }

            Gizmos.DrawSphere(tpos, 0.25f);

            var both = t1acceptable && t2acceptable;
            
            Gizmos.DrawLine(node, tpos);
            paths.Add(new Vector3[] { node, tpos });

            // voor dat we het laatste deel van het pad doen, bepaal of nog een obstakel is
            if (Physics.Raycast(tpos, node2 - tpos, out RaycastHit newHit))
            {
                // als we colliden met een building zijn we er in feite gewoon
                if (newHit.collider?.tag == "building")
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(tpos, node2); // laatste deel van het pad

                    paths.Add(new Vector3[] { tpos, node2 });

                    Gizmos.color = color;

                    return;
                }

                DrawLineViaTangentsToTarget(newHit, tpos, node2, color); // en nog een keer
            }           
        }
    }
}
