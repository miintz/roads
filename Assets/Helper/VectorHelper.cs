using UnityEngine;

namespace Assets.Helper
{
    public static class VectorHelper
    {
        /// <summary>
        /// Find tangents of position <paramref name="c"/> in <paramref name="r"/>. 
        /// </summary>
        /// <param name="c">Position for which to find tangents around radius <paramref name="r"/>. </param>
        /// <param name="p">Outside position.</param>
        /// <param name="r">Float radius around <paramref name="c"/></param>
        /// <returns>Vector3 array with tangents, null if ||<paramref name="p"/> - <paramref name="c"/>|| < <paramref name="r"/></returns>
        public static Vector3[] FindTangents(Vector3 c, Vector3 p, float r = 3f, bool ignoreRadius = false)
        {            
            var pc = p - c;
            float d = pc.magnitude;

            // normaliter willen we niet dat een tangent direct weer leid tot een collision binnen een object, maar voor terrein geld dat niet.
            // TODO:P dit moet ik nnog even bekijken, ben hier niet helemaal blij mee
            if ((d < r) && !ignoreRadius)
            {                
                return null;
            }

            // genormalizeerde richting
            var pc_normalized = pc.normalized;

            // pak de orthogonaal van de richting (haaks op de rotatie dus)
            var orthoVector = Vector3.Cross(pc_normalized, Vector3.up).normalized;

            // bepaal met de orthogonaal de 2 tangents
            var tangentDirection1 = orthoVector * r;
            var tangentDirection2 = -orthoVector * r;

            // tangents vinden met de gedraaide vector (de orthogonaal)
            var t1 = c + tangentDirection1;
            var t2 = c + tangentDirection2;

            return new [] { t1, t2 };
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

        /// <summary>
        /// Determine if the slope <paramref name="tangent"/> is on is deemed acceptable by <paramref name="maxSlopeAngle"/>.
        /// </summary>
        /// <param name="tangent"></param>
        /// <param name="obstacleCenter"></param>
        /// <param name="maxSlopeAngle"></param>
        /// <returns>Return true if the slope the tangent is on is deemed acceptaple.</returns>
        public static bool IsOnAcceptableTerrainSlope(Vector3 tangent, Vector3 obstacleCenter, float maxSlopeAngle)
        {
            // terrain normal = the angle.
            Vector3 terrainNormal = GetTerrainNormalAtPoint(tangent);
            float slopeAngle = Vector3.Angle(Vector3.up, terrainNormal);

            return slopeAngle <= maxSlopeAngle; 
        }
        
        private static Vector3 GetTerrainNormalAtPoint(Vector3 point)
        {
            var ray = new Ray(new Vector3(point.x, 1000, point.z), Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.normal;
            }

            Debug.LogWarning("Default valid slope used. Intended?");
            return Vector3.up; // TODO: ff zien wat te doen hier, want dit zal dus technisch gezien een valide slope zijn
        }
    }
}
