using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class SplineHelper
{
    /// <summary>
    /// Generate a Catmull-Rom spline using a <see cref="List{Vector3}"/> of line segments.
    /// </summary>
    /// <param name="points">The line segments</param>
    /// <param name="resolution">Resolution of the final spline. Higher = more line segments = smoother path. Default = 10.</param>
    /// <returns>Catmull-Rom spline in a <see cref="List{Vector3}"/>.</returns>
    public static List<Vector3> CatmullRomSpline(List<Vector3> points, float resolution = 10.0f)
    {
        var spline = new List<Vector3>
        {
            points.First()
        };

        for (int i = 0; i < points.Count - 3; i++)
        {
            var P0 = points[i];
            var P1 = points[i + 1];
            var P2 = points[i + 2];
            var P3 = points[i + 3];

            for (int j = 0; j <= resolution; j++)
            {
                var t = j / resolution;
                var point = 0.5f * (
                    2 * P1 +
                    (-P0 + P2) * t +
                    (2 * P0 - 5 * P1 + 4 * P2 - P3) * t * t +
                    (-P0 + 3 * P1 - 3 * P2 + P3) * t * t * t
                );

                spline.Add(point);
            }
        }

        spline.Add(points.Last());

        return spline;
    }
}
