using Assets.Helper;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Algorithm
{
    public class TangentPathfinder
    {
        private Vector3 _destination;
        private Vector3 _origin;
        
        private List<string> _nodeTags;

        private bool _debug;
        private bool _checkTerrainGradientWhenPathing;
        
        private int _rakeLimit;
        private int _raycastValidMaxSamples = 20;

        private float _acceptableTerrainSlope = 30f;
        private float _raycastValidMaxHeight = 1.0f;

        // TODO: ik moet hier niet de tag mee te geven, maar het object waar ik naar toe wil.
        //       nu moet ik werken met een node pair. Denk dat het niet heel veel uitmaakt,
        //       maar op z'n minst is het semantisch handig om niet met de tag te werken
        public TangentPathfinder(Vector3 origin, Vector3 destination, List<string> nodeTags, bool debug = false, float acceptableTerrainSlope = 30f, int rakeLimit = 15, bool checkTerrainGradientWhenPathing = false, float maxPathHeightDifferential = 1.0f)
        {
            _origin = origin;
            _destination = destination;
            _debug = debug;
            _nodeTags = nodeTags;
            _acceptableTerrainSlope = acceptableTerrainSlope;
            _rakeLimit = rakeLimit;
            _checkTerrainGradientWhenPathing = checkTerrainGradientWhenPathing;
            _raycastValidMaxHeight = maxPathHeightDifferential;
        }

        public List<Vector3> GetPath()
        {            
            var segments = new List<Vector3>
            {
                _origin
            };

            if (!Physics.Raycast(_origin, _destination - _origin, out RaycastHit hit))
            {
                // dit zou niet moeten gebeuren, maar hee
                if (_debug)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(_origin, _destination);
                }

                return new List<Vector3>();
            }
            else
            {
                var point = hit.point;

                if (_debug)
                {
                    Handles.BeginGUI();
                    GUI.color = Color.black;
                    Handles.Label(point - (Vector3.right), "hit");
                    Handles.EndGUI();

                    Gizmos.DrawSphere(point, 0.3f);
                }

                if (_checkTerrainGradientWhenPathing)
                {
                    // voordat we gewoon aannemen dat er een collision is: even checken hoe het terrein eruit ziet
                    point = GetValidPointAlongRay(_origin, hit.point, _raycastValidMaxHeight, _raycastValidMaxSamples); // TODO; dat nummer bepalen op lengte van lijn
                }

                // we checken ook of de hit position hetzelfde is als de destination. De MST zou moeten voorkomen dat dit nodig is maar hee *haalt schouders op*
                if (_nodeTags.Contains(hit.collider.tag) && point == _destination)
                {
                    return segments;
                }

                return GetPathSegments(point, _origin, segments);
            }              
        }

        private List<Vector3> GetPathSegments(Vector3 hit, Vector3 newOrigin, List<Vector3> segments)
        {
            // zoek de tangents met een bepaalde radius voor nu, we kunnen die radius opkrikken als we geen geldige tangents vinden
            var t1acceptable = false;
            var t2acceptable = false;

            var t1 = Vector3.zero;
            var t2 = Vector3.zero;

            var r = 3f;
            var stepOnRake = _rakeLimit;

            while (!t1acceptable && !t2acceptable) // loop de loop tot er 1 geldig is
            {
                var tangents = VectorHelper.FindTangents(hit, newOrigin, r);
                if (tangents == null)
                {
                    if (_debug)
                    {
                        // wat is een no tangents color
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(hit, 0.25f);
                    }

                    // probeer nog eens maar negeer nu de radius
                    tangents = VectorHelper.FindTangents(hit, newOrigin, r, true);
                    if (tangents == null)
                    {
                        // todo: ook dit zou eigenlijk niet moeten gebeuren
                        Debug.Log("still no tangents found");
                        return segments;
                    }
                }

                t1 = tangents[0];
                t2 = tangents[1];

                if (_debug)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(t1, 0.4f);
                    Gizmos.DrawSphere(t2, 0.4f);

                    Handles.BeginGUI();
                    GUI.color = Color.black;
                    Handles.Label(t1 - (Vector3.right), "T1");
                    Handles.Label(t2 - (Vector3.right), "T2");
                    Handles.EndGUI();
                }

                var terrainHelper = new TerrainHelper();

                // de tangents moeten wel op het terrin liggen, niet erboven of eronder. 
                t1 = terrainHelper.AdjustVectorToTerrain(t1);
                t2 = terrainHelper.AdjustVectorToTerrain(t2);

                // nu zullen de tangents denk ik goed liggen (raycast op terrain + 1 omhoog voor de vorm), maar hoe is de slope? Normal checken dus denk ik?
                t1acceptable = terrainHelper.IsOnAcceptableTerrainSlope(t1, hit, _acceptableTerrainSlope);
                t2acceptable = terrainHelper.IsOnAcceptableTerrainSlope(t2, hit, _acceptableTerrainSlope);

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
                Debug.LogWarning("stepped on rake. Intended?");
                return segments;
            }

            var tpos = Vector3.zero;
            if (t1acceptable && t2acceptable)
            {
                // is via t1 of t2 sneller
                var t1d = Vector3.Distance(t1, hit) + Vector3.Distance(t1, _destination);
                var t2d = Vector3.Distance(t2, hit) + Vector3.Distance(t2, _destination);

                // goochelen met ternary operator, gelukkig is dit mijn code :)
                tpos = t1d < t2d ? t1 : t2;
            }
            else
            {
                // als het goed is hebben 1 acceptabele tangent, dus dit zou moeten kunnen
                tpos = t1acceptable ? t1 : t2;
            }

            // voeg de tangent (in feite onze nieuwe origin) toe aan segments
            segments.Add(tpos);

            if (_debug)
            {
                Handles.BeginGUI();
                GUI.color = Color.black;
                Handles.Label(tpos - (Vector3.right), "Tpos");
                Handles.EndGUI();

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(tpos, 0.25f);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(newOrigin, 0.25f);
            }

            // voor dat we het laatste deel van het pad doen, bepaal of nog een obstakel is
            if (Physics.Raycast(tpos, _destination - tpos, out RaycastHit newHit))
            {
                var point = newHit.point;
                if (_checkTerrainGradientWhenPathing)
                {
                    point = GetValidPointAlongRay(tpos, newHit.point, _raycastValidMaxHeight, _raycastValidMaxSamples);
                }

                // als we colliden met de geconfigureerde tag zijn we er in feite gewoon
                if (_nodeTags.Contains(newHit.collider?.tag) && Vector3.Distance(point, _destination) < 5.0f)
                {
                    // we kunnen de destination nu zien liggen, wil niet zeggen dat we er al zijn
                    // maar we kunnen ook niet op positie controleren, want we hebben het over de rigidbody (destionation) en een raycasthit op de collider.

                    segments.Add(_destination);

                    return segments;
                }

                return GetPathSegments(point, tpos, segments); // en nog een keer
            }

            // dit zou serieus niet moeten gebeuren, dan gaan we het in elk geval zeker weten :)
            throw new Exception("gitaarsolo");
        }

        private Vector3 GetValidPointAlongRay(Vector3 start, Vector3 end, float maxHeightDiff, int numSamples)
        {
            if (maxHeightDiff <= 0)
                return end;

            if (_debug)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(start, 0.3f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(start, 0.2f);
            }

            for (int i = 1; i <= numSamples; i++)
            {
                var t = i / (float)numSamples;
                var point = Vector3.Lerp(start, end, t);
                var terrainHeight = Terrain.activeTerrain.SampleHeight(point);

                // Check height difference
                var diff = Mathf.Abs(point.y - terrainHeight);
                if (diff > maxHeightDiff)
                {
                    point.y -= diff;

                    if (_debug)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawSphere(point, 0.25f);

                        Gizmos.DrawLine(point, new Vector3(point.x, point.y += diff, point.z));
                    }

                    return point; 
                }
            }

            return end; // If no issues, return the original endpoint
        }
    }
}
