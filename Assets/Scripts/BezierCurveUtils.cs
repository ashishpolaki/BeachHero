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
                curvePoints.Add(previousPoint);

                for (int j = 1; j <= resolution; j++)
                {
                    float t = j / (float)resolution; // Calculate t based on resolution
                    Vector3 point = CalculateBezierPoint(t, start.position, start.OutTangentWorld, end.InTangentWorld, end.position);

                    curvePoints.Add(point); // Collect the point
                }
            }
            return curvePoints.ToArray();
        }

        private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 point = uuu * p0; // (1-t)^3 * P0
            point += 3 * uu * t * p1; // 3(1-t)^2 * t * P1
            point += 3 * u * tt * p2; // 3(1-t) * t^2 * P2
            point += ttt * p3;        // t^3 * P3

            return point;
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
            var keyframes  = new BezierKeyframe[totalSegments + 1]; // Add one more keyframe for the endpoint

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


    }
}
