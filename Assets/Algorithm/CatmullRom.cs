using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Algorithm
{
    public class CatmullRom
    {
        private List<Vector3> _points { get; set; }
        private bool _valid => _points.Count > 0;

        public CatmullRom(List<Vector3> points)
        {
            _points = points;
        }

        /// <summary>
        /// Generate a Catmull-Rom spline using a <see cref="List{Vector3}"/>.
        /// 
        /// Note: if using a undirected graph, create instances of <see cref="CatmullRom"/> for each line segment.
        /// </summary>
        /// <param name="resolution">Resolution of the final spline. Higher = more line segments = smoother path. Default = 10.</param>
        /// <returns>Catmull-Rom spline in a <see cref="List{Vector3}"/>.</returns>
        public List<Vector3> GetSpline(float resolution = 10.0f)
        {
            if (!_valid)
            {
                Debug.LogWarning("No points provided to spline.");
                return new List<Vector3>();
            }

            var spline = new List<Vector3>
            {
                _points.First()
            };

            for (int i = 0; i < _points.Count - 3; i++)
            {
                var P0 = _points[i];
                var P1 = _points[i + 1];
                var P2 = _points[i + 2];
                var P3 = _points[i + 3];

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

            spline.Add(_points.Last());

            return spline;
        }
    }
}
