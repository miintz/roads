using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets.Helper
{
    public static class VectorHelper
    {
        /// <summary>
        /// Find tangents of position <para
        /// </summary>
        /// <param name="c">Position for which to find tangents around radius <paramref name="r"/>. </param>
        /// <param name="p">Outside position.</param>
        /// <param name="r">Float radius around <paramref name="c"/></param>
        /// <returns>Vector3 array with tangents, null if ||<paramref name="p"/> - <paramref name="c"/>|| < <paramref name="r"/></returns>
        public static Vector3[] FindTangents(Vector3 c, Vector3 p, float r = 3f, bool ignoreRadius = false)
        {            
            Vector3 pc = p - c;
            float d = pc.magnitude;

            if ((d < r) && !ignoreRadius)
            {
                // todo: dit uitgezet, want werkt niet als het om terrein gaat. Als je huizen dicht bij elkaar hebt sure, maar voor terrein? nee
                return null;
            }

            // genormalizeerde richting
            Vector3 pc_normalized = pc.normalized;

            // pak de orthogonaal van de richting (haaks op de rotatie dus)
            Vector3 orthoVector = Vector3.Cross(pc_normalized, Vector3.up).normalized;

            // bepaal met de orthogonaal de 2 tangents
            Vector3 tangentDirection1 = orthoVector * r;
            Vector3 tangentDirection2 = -orthoVector * r;

            // tangents vinden met de gedraaide vector (de orthogonaal)
            var t1 = c + tangentDirection1;
            var t2 = c + tangentDirection2;

            return new Vector3[] { t1, t2 };
        }

        public static Vector3 LineIntersectsWithAny(List<Vector3[]> lines, Vector3[] ray)
        {
            foreach (var line in lines)
            {
                var intersect = LineIntersectsWith(line, ray);
                if (intersect != Vector3.zero)
                {
                    return intersect;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Try to move a <see cref="Vector3"> to match the terrain
        /// </summary>
        /// <param name="tangentPoint"><see cref="Vector3"/> to adjust</param>
        /// <returns>Adjusted <see cref="Vector3"/> object.</returns>
        public static Vector3 AdjustVectorToTerrain(Vector3 tangentPoint)
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

            return tangentPoint; // TODO: en nu? tangent helemaal buiten het terrein??
        }

        public static bool IsOnAcceptableTerrainSlope(Vector3 tangent, Vector3 obstacleCenter, float maxSlopeAngle)
        {
            Vector3 terrainNormal = GetTerrainNormalAtPoint(tangent); // Optional for slope validation
            float slopeAngle = Vector3.Angle(Vector3.up, terrainNormal);

            return slopeAngle <= maxSlopeAngle; // Ensure slope is acceptable
        }
        
        private static Vector3 GetTerrainNormalAtPoint(Vector3 point)
        {
            Ray ray = new Ray(new Vector3(point.x, 1000, point.z), Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.normal;
            }

            Debug.LogWarning("Default valid slope used. Is this intended?");
            return Vector3.up; // TODO: ff zien wat te doen hier, want dit zal dus technisch gezien een valide slope zijn
        }

        /// <summary>
        /// Determine if <paramref name="ray" /> intersects with <paramref name="line"/>.
        /// </summary>
        /// <param name="line">The line to aim for.</param>
        /// <param name="ray">The ray that might intersect with <paramref name="line"/></param>
        /// <returns>Intersection position if <paramref name="ray"/> intersects with <paramref name="line"/>, otherwise Vector3.zero.</returns>
        public static Vector3 LineIntersectsWith(Vector3[] line, Vector3[] ray)
        {
            var p1 = line[0];
            var p2 = line[1];

            var q1 = ray[0];
            var q2 = ray[1];

            // Calculate direction vectors for both line segments
            Vector3 d1 = p2 - p1; // Direction of line segment 1
            Vector3 d2 = q2 - q1; // Direction of line segment 2

            // Calculate the denominator (determinant)
            float denominator = d1.x * d2.z - d1.z * d2.x;

            // Check if lines are parallel (denominator is zero)
            if (Mathf.Abs(denominator) < Mathf.Epsilon)
            {
                return Vector3.zero; // No intersection (lines are parallel)
            }

            // Calculate the parameters s and t
            Vector3 w = q1 - p1; // Vector from p1 to q1

            float s = (w.x * d2.z - w.z * d2.x) / denominator;
            float t = (w.x * d1.z - w.z * d1.x) / denominator;

            // Check if the intersection point is within the bounds of both segments
            if (s < 0 || s > 1 || t < 0 || t > 1)
            {
                return Vector3.zero; // Intersection is outside of one or both segments
            }

            // Calculate the intersection point
            return p1 + s * d1; // Using p1 and s to find the intersection point
        }
    }
}
