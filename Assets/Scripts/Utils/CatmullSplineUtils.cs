using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class CatmullSplineUtils
    {
        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            // Catmull-Rom spline formula
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        public static List<Vector3> GetEvenlySpacedPoints(List<Vector3> pathPoints, float spacing)
        {
            List<Vector3> evenlySpacedPoints = new List<Vector3>();
            float distanceSinceLastPoint = 0f;

            evenlySpacedPoints.Add(pathPoints[0]); // Start with the first point

            for (int i = 0; i < pathPoints.Count - 3; i++)
            {
                Vector3 previousPoint = pathPoints[i + 1]; // Start from the second control point

                for (float t = 0; t <= 1; t += 0.01f) // High resolution for accurate arc length calculation
                {
                    Vector3 interpolatedPoint = CatmullRom(
                        pathPoints[i],
                        pathPoints[i + 1],
                        pathPoints[i + 2],
                        pathPoints[i + 3],
                        t
                    );

                    // Accumulate distance between the previous point and the current interpolated point
                    distanceSinceLastPoint += Vector3.Distance(previousPoint, interpolatedPoint);

                    // If the accumulated distance exceeds the spacing, add a new point
                    if (distanceSinceLastPoint >= spacing)
                    {
                        evenlySpacedPoints.Add(interpolatedPoint);
                        distanceSinceLastPoint = 0f; // Reset the distance counter
                    }

                    previousPoint = interpolatedPoint; // Update the previous point
                }
            }

            if (evenlySpacedPoints.Contains(pathPoints[pathPoints.Count - 1]) == false)
                evenlySpacedPoints.Add(pathPoints[pathPoints.Count - 1]); // Add the last point if not already added

            return evenlySpacedPoints;
        }
    }
}
