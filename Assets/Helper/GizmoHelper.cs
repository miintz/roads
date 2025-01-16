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
            float acceptableSlope = 30f;

            // zoek de tangents met een bepaalde radius voor nu, we kunnen die radius opkrikken als we geen geldige tangents vinden
            var t1acceptable = false;
            var t2acceptable = false;

            Vector3 t1 = Vector3.zero;
            Vector3 t2 = Vector3.zero;

            float r = 3f;
            int emergencyBrake = 15;

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

                emergencyBrake--;
                if (emergencyBrake == 0)
                {
                    break;
                }

                // krik de tangent radius op
                r += 2f;
            }

            if (emergencyBrake == 0)
            {
                // als we hier komen zijn er dus geen geldige tangents te vinden op deze oogabooga manier. 
                Debug.Log("emergency rake pulled. Intended?");
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

            // evil ternary operator, causes stackoverflow
            //var tpos = t1d < t2d ? t1acceptable ? t1 : t1 : t2acceptable ? t2 : t1;
            Gizmos.DrawSphere(tpos, 0.25f);

            var both = t1acceptable && t2acceptable;

            // maar raakt de nieuwe lijn 1 van de al bestaande lijnen
            var intersect = VectorHelper.LineIntersectsWithAny(paths, new Vector3[] { node, tpos });

            //if (intersect == Vector3.zero)
            //{
            // pad raakt niets (geintje, dit werkt helemaal niet!
            Gizmos.DrawLine(node, tpos);
            paths.Add(new Vector3[] { node, tpos });

            // voor dat we het laatste deel van het pad doen, bepaal of nog een obstakel is
            if (Physics.Raycast(tpos, node2 - tpos, out RaycastHit newHit))
            {
                // voordat we verder gaan: ff checken hoe ver we van de destination af zitten. Misschien is het goed geweest?
                // todo: dit moet ik echt even anders opzetten, dit gaat er dus van uit wanneer er GEEN raycast hit is, we bij de destination zijn. klopt natuurlijk niet :)

                var distnace = Vector3.Distance(tpos, node2);
                if (distnace < 10f) // radius van de collider
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(tpos, node2); // laatste deel van het pad
                    paths.Add(new Vector3[] { tpos, node2 });

                    Gizmos.color = color;

                    return;
                }

                DrawLineViaTangentsToTarget(newHit, tpos, node2, color); // en nog een keer
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(tpos, node2); // laatste deel van het pad
                paths.Add(new Vector3[] { tpos, node2 });

                Gizmos.color = color;
            }
            //}
            //else // dit hele stuk klopt volgens mij geen zak van, hoezo hebben we een intersect??
            //{
            //    // intersect? blijkbaar?            
            //    // TODO: er dus blijkbaar een intersect hier                   
            //    //Gizmos.color = color;
            //    //Gizmos.DrawCube(tpos, new Vector3(1f, 1f, 1f));
            //    //Gizmos.color = Color.yellow;
            //    //Gizmos.DrawCube(intersect, new Vector3(1f, 1f, 1f));

            //    Gizmos.color = color;
            //    Gizmos.DrawLine(tpos, intersect);

            //    // vanaf hier moeten we raycasten naar het eindpunt (toch even proberen)
            //    if (Physics.Raycast(tpos, node2 - tpos, out hit))
            //    {
            //        //Gizmos.color = Color.white;
            //        Debug.DrawRay(tpos, (node2 - tpos).normalized * 10000, Color.black, 5.0f);

            //        Gizmos.color = Color.magenta;
            //        Gizmos.DrawSphere(hit.point, 0.5f);
            //        Gizmos.color = color;
            //        // recursie van tpos (de laatste tangent) naar de positie van de collider van het obstakel
            //        DrawLineViaTangentsToTarget(hit, tpos, node2, color);

            //    }
            //    else
            //    {
            //        // blijkbaar zijn we er al...                
            //        Gizmos.DrawLine(tpos, node2);
            //        paths.Add(new Vector3[] { tpos, node2 });
            //    }               
            //}
        }
    }
}
