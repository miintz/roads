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
    }
}
