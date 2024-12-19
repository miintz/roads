using UnityEngine;
using System.Collections.Generic;

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
        public static Vector3[] FindTangents(Vector3 c, Vector3 p, float r = 3.1f)
        {
            Vector3 pc = p - c;
            float d = pc.magnitude;

            if (d < r)
            {
                // object zit in de radius van het obstakel (kan denk ik niet omdat alles 3 is)
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
