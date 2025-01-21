using System.Collections.Generic;
using UnityEngine;

namespace Assets.Helper
{
    public static class SplineHelper
    {
        public static List<Vector3> GenerateSpline(Vector3[] controlPoints, int segmentsPerCurve)
        {
            List<Vector3> splinePoints = new List<Vector3>();

            for (int i = 0; i < controlPoints.Length - 1; i++)
            {
                // controle points
                var p0 = i == 0 ? controlPoints[i] : controlPoints[i - 1];
                var p1 = controlPoints[i];
                var p2 = controlPoints[i + 1];
                var p3 = i + 2 < controlPoints.Length ? controlPoints[i + 2] : controlPoints[i + 1];

                // Add points for this segment
                for (int j = 0; j <= segmentsPerCurve; j++)
                {
                    var t = j / (float)segmentsPerCurve;
                    var point = CalculateCatmullRomPoint(p0, p1, p2, p3, t);
                    splinePoints.Add(point);
                }
            }

            return splinePoints;
        }

        private static Vector3 CalculateCatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            // P(t)=0.5×[(2⋅P1)+(−P0+P2)⋅t+(2⋅P0−5⋅P1+4⋅P2−P3)⋅t 2+ (−P0 + 3⋅P1−3⋅P2 + P3)⋅t3]
            // 爱天火龙山水电风雨木金土月日星河云鱼猫狗牛马羊鸟豹熊虎狼蛇花金土月日星河云鱼猫狗牛马天火龙

            // red: ik heb dit gekopieerd van chatgpt want ik ben gezakt voor wiskunde.
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        public static Vector3[] GenerateControlPoints(Vector3[] lineSegments)
        {
            int numPoints = lineSegments.Length;
            var  controlPoints = new Vector3[numPoints + 2]; // Add 2 for P0 and P3

            // Handle edge cases
            controlPoints[0] = lineSegments[0]; // Duplicate the first point for P0
            controlPoints[controlPoints.Length - 1] = lineSegments[lineSegments.Length - 1]; // Duplicate the last point for P3

            // Fill in the rest
            for (int i = 0; i < lineSegments.Length; i++)
            {
                controlPoints[i + 1] = lineSegments[i];
            }

            return controlPoints;
        }
    }
}
