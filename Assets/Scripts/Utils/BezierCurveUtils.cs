using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class BezierCurveUtils
    {
        public static Vector3[] GeneratePath(BezierKeyframe[] bezierKeyframes, float resolution)
        {
            List<Vector3> curvePoints = new List<Vector3>();
            for (int i = 0; i < bezierKeyframes.Length - 1; i++)
            {
                BezierKeyframe start = bezierKeyframes[i];
                BezierKeyframe end = bezierKeyframes[i + 1];

                Vector3 previousPoint = start.position;

                if (curvePoints.Contains(previousPoint)) // Avoid duplicates
                {
                    if (curvePoints[0] == previousPoint)
                    {
                        curvePoints.Add(previousPoint); // Collect the point
                    }
                }
                else
                {
                    curvePoints.Add(previousPoint); // Collect the point
                }

                for (int j = 1; j <= resolution; j++)
                {
                    float t = j / (float)resolution; // Calculate t based on resolution
                    Vector3 point = GetPoint(start.position, start.OutTangentWorld, end.InTangentWorld, end.position, t);

                    if (curvePoints.Contains(point)) // Avoid duplicates
                    {
                        if (curvePoints[0] == point)
                        {
                            curvePoints.Add(point); // Collect the point
                        }
                    }
                    else
                    {
                        curvePoints.Add(point); // Collect the point
                    }
                }
            }
            return curvePoints.ToArray();
        }

        public static BezierKeyframe[] CreateCircleShape(float radius, int segments)
        {
            var keyframes = new BezierKeyframe[segments + 1]; // Add one more keyframe for the endpoint

            float angleStep = 360f / segments; // Divide the circle into equal segmentsPerLoop

            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.Deg2Rad * (i * angleStep); // Convert angle to radians

                // Calculate the position of the keyframe on the circle
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                // Calculate the direction of the tangent
                Vector3 tangentDirection = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle)).normalized;

                // Calculate the tangents for smooth transitions
                Vector3 inTangent = -tangentDirection * (radius / segments); // In tangent points backward
                Vector3 outTangent = tangentDirection * (radius / segments); // Out tangent points forward

                // Create the keyframe
                keyframes[i] = new BezierKeyframe
                {
                    position = position,
                    inTangentLocal = inTangent,
                    outTangentLocal = outTangent,
                };
            }

            // Add the additional endpoint (duplicate the first keyframe)
            keyframes[segments] = new BezierKeyframe
            {
                position = keyframes[0].position,
                inTangentLocal = keyframes[0].inTangentLocal,
                outTangentLocal = keyframes[0].outTangentLocal
            };

            return keyframes;
        }

        public static BezierKeyframe[] CreateFigureEightShape(float radius, int segmentsPerLoop)
        {
            int totalSegments = segmentsPerLoop * 2; // Double the number of segmentsPerLoop for the figure-eight shape
            var keyframes = new BezierKeyframe[totalSegments + 1]; // Add one more keyframe for the endpoint

            float angleStep = 360f / segmentsPerLoop; // Divide the circle into equal segmentsPerLoop

            for (int i = 0; i < segmentsPerLoop; i++)
            {
                float angle = Mathf.Deg2Rad * (i * angleStep); // Convert angle to radians

                // Calculate the position of the keyframe on the circle
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius - radius, 0, Mathf.Sin(angle) * radius);

                // Calculate the direction of the tangent
                Vector3 tangentDirection = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle)).normalized;

                // Calculate the tangents for smooth transitions
                Vector3 inTangent = -tangentDirection * (radius / segmentsPerLoop); // In tangent points backward
                Vector3 outTangent = tangentDirection * (radius / segmentsPerLoop); // Out tangent points forward

                // Create the keyframe
                keyframes[i] = new BezierKeyframe
                {
                    position = position,
                    inTangentLocal = inTangent,
                    outTangentLocal = outTangent,
                };
            }

            for (int i = 0; i < segmentsPerLoop; i++)
            {
                float angle = Mathf.Deg2Rad * (180f - i * angleStep); // Start at 180 degrees

                // Calculate the position of the keyframe on the circle
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius + radius, 0, Mathf.Sin(angle) * radius);

                // Calculate the direction of the tangent
                Vector3 tangentDirection = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle)).normalized;

                // Calculate the tangents for smooth transitions
                Vector3 inTangent = -tangentDirection * (radius / segmentsPerLoop); // In tangent points backward
                Vector3 outTangent = tangentDirection * (radius / segmentsPerLoop); // Out tangent points forward

                // Create the keyframe
                keyframes[segmentsPerLoop + i] = new BezierKeyframe
                {
                    position = position,
                    inTangentLocal = inTangent,
                    outTangentLocal = outTangent,
                };
            }

            // Add the additional endpoint (duplicate the first keyframe)
            keyframes[totalSegments] = new BezierKeyframe
            {
                position = keyframes[0].position,
                inTangentLocal = keyframes[0].inTangentLocal,
                outTangentLocal = keyframes[0].outTangentLocal
            };
            return keyframes;
        }

        // Cubic Bezier curve: p0, p1, p2, p3
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * oneMinusT * p0 +
                   3f * oneMinusT * oneMinusT * t * p1 +
                   3f * oneMinusT * t * t * p2 +
                   t * t * t * p3;
        }

        public static Vector3 GetTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float oneMinusT = 1f - t;

            return
                3f * oneMinusT * oneMinusT * (p1 - p0) +
                6f * oneMinusT * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
        }

        public static List<Vector3> GetEvenlySpacedPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int count)
        {
            List<Vector3> sampled = new List<Vector3>();
            List<float> cumulative = new List<float>();
            int resolution = 1000;
            float totalDist = 0;
            Vector3 prev = GetPoint(p0, p1, p2, p3, 0);

            sampled.Add(prev);
            cumulative.Add(0);

            for (int i = 1; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                Vector3 pt = GetPoint(p0, p1, p2, p3, t);
                totalDist += Vector3.Distance(prev, pt);
                sampled.Add(pt);
                cumulative.Add(totalDist);
                prev = pt;
            }

            List<Vector3> even = new List<Vector3>();
            float interval = totalDist / (count - 1);
            float distTarget = 0f;
            int index = 0;

            for (int i = 0; i < count; i++)
            {
                while (index < cumulative.Count - 1 && cumulative[index] < distTarget)
                    index++;

                if (index == 0)
                {
                    even.Add(sampled[0]);
                }
                else
                {
                    float d0 = cumulative[index - 1];
                    float d1 = cumulative[index];
                    float t = Mathf.InverseLerp(d0, d1, distTarget);
                    Vector3 pt = Vector3.Lerp(sampled[index - 1], sampled[index], t);
                    even.Add(pt);
                }

                distTarget += interval;
            }

            return even;
        }

        public static Vector3 GetClosestPointOnBezierPath(List<BezierPoint> bezierPoints, Vector3 targetPosition, int resolution = 30)
        {
            Vector3 closestPoint = Vector3.zero;
            float minSqrDist = float.MaxValue;

            for (int i = 0; i < bezierPoints.Count - 1; i++)
            {
                BezierPoint bp0 = bezierPoints[i];
                BezierPoint bp1 = bezierPoints[i + 1];

                if (bp0.anchorPoint == null || bp1.anchorPoint == null)
                    continue;

                Vector3 p0 = bp0.anchorPoint;
                Vector3 p1 = p0 + bp0.outTangent;
                Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
                Vector3 p3 = bp1.anchorPoint;

                for (int j = 0; j <= resolution; j++)
                {
                    float t = j / (float)resolution;
                    Vector3 pointOnCurve = GetPoint(p0, p1, p2, p3, t);
                    float sqrDist = (pointOnCurve - targetPosition).sqrMagnitude;

                    if (sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        closestPoint = pointOnCurve;
                    }
                }
            }

            return closestPoint;
        }

        public static void SampleEvenlySpacedPointsWithTangents(List<BezierPoint> bezierPoints, int totalSamples, out List<Vector3> sampledPoints, out List<Vector3> tangentVectors)
        {
            sampledPoints = new List<Vector3>();
            tangentVectors = new List<Vector3>();

            float totalLength = 0f;
            List<float> segmentLengths = new List<float>();

            // Estimate total curve length
            for (int i = 0; i < bezierPoints.Count - 1; i++)
            {
                Vector3 p0 = bezierPoints[i].anchorPoint;
                Vector3 p1 = p0 + bezierPoints[i].outTangent;
                Vector3 p3 = bezierPoints[i + 1].anchorPoint;
                Vector3 p2 = p3 + bezierPoints[i + 1].inTangent;

                float length = EstimateCurveLength(p0, p1, p2, p3, 20);
                segmentLengths.Add(length);
                totalLength += length;
            }

            float spacing = totalLength / (totalSamples - 1);
            float distanceSoFar = 0f;
            Vector3 lastPoint = bezierPoints[0].anchorPoint;

            sampledPoints.Add(lastPoint);
            tangentVectors.Add(Vector3.forward); // placeholder

            for (int seg = 0; seg < bezierPoints.Count - 1; seg++)
            {
                Vector3 p0 = bezierPoints[seg].anchorPoint;
                Vector3 p1 = p0 + bezierPoints[seg].outTangent;
                Vector3 p3 = bezierPoints[seg + 1].anchorPoint;
                Vector3 p2 = p3 + bezierPoints[seg + 1].inTangent;

                float segmentLength = segmentLengths[seg];
                int steps = Mathf.CeilToInt(segmentLength * 10);
                Vector3 prev = p0;

                for (int i = 1; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    Vector3 point = EvaluateBezier(p0, p1, p2, p3, t);
                    Vector3 tangent = EvaluateBezierDerivative(p0, p1, p2, p3, t).normalized;

                    float dist = Vector3.Distance(prev, point);
                    if (distanceSoFar + dist >= spacing)
                    {
                        float overshoot = spacing - distanceSoFar;
                        Vector3 newPoint = Vector3.Lerp(prev, point, overshoot / dist);
                        Vector3 newTangent = tangent;

                        sampledPoints.Add(newPoint);
                        tangentVectors.Add(newTangent);

                        distanceSoFar = 0f;
                        prev = newPoint;
                        i--; // Re-evaluate this t
                    }
                    else
                    {
                        distanceSoFar += dist;
                        prev = point;
                    }
                    if (sampledPoints.Count >= totalSamples - 1)
                    {
                        // Ensure we don't overshoot by one before final point
                        break;
                    }
                }
                if (sampledPoints.Count >= totalSamples - 1)
                    break;
            }
            // Add final point if needed (ensures exactly totalSamples)
            if (sampledPoints.Count < totalSamples)
            {
                Vector3 end = bezierPoints[bezierPoints.Count - 1].anchorPoint;
                Vector3 finalTangent = (end - sampledPoints[sampledPoints.Count - 1]).normalized;

                sampledPoints.Add(end);
                tangentVectors.Add(finalTangent);
            }
        }

        #region Private Methods
        private static float EstimateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int subdivisions = 10)
        {
            float length = 0f;
            Vector3 prev = p0;
            for (int i = 1; i <= subdivisions; i++)
            {
                float t = i / (float)subdivisions;
                Vector3 point = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
                length += Vector3.Distance(prev, point);
                prev = point;
            }
            return length;
        }
        private static Vector3 EvaluateBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            return u * u * u * p0 +
                   3 * u * u * t * p1 +
                   3 * u * t * t * p2 +
                   t * t * t * p3;
        }

        private static Vector3 EvaluateBezierDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            return 3 * u * u * (p1 - p0) +
                   6 * u * t * (p2 - p1) +
                   3 * t * t * (p3 - p2);
        }
        #endregion
    }
}
