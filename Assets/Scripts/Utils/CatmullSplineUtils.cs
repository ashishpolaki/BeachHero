using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class CatmullSplineUtils
    {
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
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

        public static Vector3 GetTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            // Derivative of Catmull-Rom spline
            return 0.5f * (
                -p0 + p2 +
                2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t +
                3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t * t
            ).normalized;
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
                    Vector3 interpolatedPoint = GetPoint(
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

        public static List<Vector3> GetEvenlySpacedPointsByCount(List<Vector3> pathPoints, int count)
        {
            if (count < 2 || pathPoints.Count < 4)
                ("Need at least 4 path points and count >= 2").LogError();

            List<Vector3> densePoints = new List<Vector3>();
            for (int i = 0; i < pathPoints.Count - 3; i++)
            {
                for (float t = 0; t <= 1f; t += 0.01f)
                {
                    Vector3 point = GetPoint(
                        pathPoints[i],
                        pathPoints[i + 1],
                        pathPoints[i + 2],
                        pathPoints[i + 3],
                        t
                    );
                    densePoints.Add(point);
                }
            }

            // Calculate total length
            float totalLength = 0f;
            for (int i = 0; i < densePoints.Count - 1; i++)
            {
                totalLength += Vector3.Distance(densePoints[i], densePoints[i + 1]);
            }

            float segmentLength = totalLength / (count - 1);
           // float distanceAccumulated = 0f;

            List<Vector3> finalPoints = new List<Vector3>();
            finalPoints.Add(densePoints[0]);

            float distanceSinceLastPoint = 0f;
            Vector3 lastPoint = densePoints[0];

            for (int i = 1; i < densePoints.Count; i++)
            {
                float distance = Vector3.Distance(lastPoint, densePoints[i]);
                distanceSinceLastPoint += distance;

                if (distanceSinceLastPoint >= segmentLength)
                {
                    finalPoints.Add(densePoints[i]);
                    distanceSinceLastPoint = 0f;

                    if (finalPoints.Count == count)
                        break;
                }

                lastPoint = densePoints[i];
            }

            // Ensure we always end at the last point
            if (finalPoints.Count < count)
                finalPoints.Add(densePoints[densePoints.Count - 1]);

            return finalPoints;
        }
    }
}
