using Assets.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Algorithm
{
    public class TangentPath
    {
        private Vector3 _destination;
        private Vector3 _origin;
        private bool _debug;
        private string _objectTag;
        private float _acceptableTerrainSlope;
        private int _rakeLimit;

        public TangentPath(Vector3 origin, Vector3 destination, string objectTag = "building", bool debug = false, float acceptableTerrainSlope = 30f, int rakeLimit = 15)
        {
            _origin = origin;
            _destination = destination;
            _debug = debug;
            _objectTag = objectTag;
            _acceptableTerrainSlope = acceptableTerrainSlope;
            _rakeLimit = rakeLimit;
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
                // we checken ook of de hit position hetzelfde is als de destination. De MST zou moeten voorkomen dat dit nodig is maar hee *haalt schouders op*
                if (hit.collider.tag.Equals(_objectTag) && hit.point == _destination)
                {
                    return segments;
                }

                return GetPathSegments(hit.point, _origin, segments);
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
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(hit, 0.5f);

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
                Debug.Log("stepped on rake. Intended?");

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

            // voor dat we het laatste deel van het pad doen, bepaal of nog een obstakel is
            if (Physics.Raycast(tpos, _destination - tpos, out RaycastHit newHit))
            {
                // als we colliden met de geconfigureerde tag zijn we er in feite gewoon
                if (newHit.collider?.tag == _objectTag)
                {
                    // vergeet de laatste tangent niet
                    segments.Add(_destination);

                    return segments;
                }

                return GetPathSegments(newHit.point, tpos, segments); // en nog een keer
            }

            // dit zou serieus niet moeten gebeuren, dan gaan we het in elk geval zeker weten :)
            throw new Exception("gitaarsolo");
        }
    }
}
