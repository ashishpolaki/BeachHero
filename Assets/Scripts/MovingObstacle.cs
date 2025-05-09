using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class MovingObstacle : Obstacle
    {
        #region Private Variables
        [SerializeField] private int resolution;
        [SerializeField] private BezierKeyframe[] keyframes;
        [SerializeField] private float rotationSpeed = 0.3f;
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private int nextPointIndex;
        [SerializeField] private Vector3[] pointsList;
        #endregion

        #region Properties
        public BezierKeyframe[] Keyframes
        {
            get => keyframes;
            private set => keyframes = value;
        }
        protected Vector3[] PointsList
        {
            get => pointsList;
            private set => pointsList = value;
        }
        #endregion

        #region Public Methods
        public virtual void Init(MovingObstacleData movingObstacleData)
        {
            nextPointIndex = 1;
            keyframes = movingObstacleData.bezierKeyframes;
            pointsList = GeneratePath(keyframes, movingObstacleData.resolution);
            transform.position = pointsList[0];
            transform.LookAt(pointsList[1]);
        }

        private void OnDrawGizmos()
        {
            if (pointsList != null && pointsList.Length > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < pointsList.Length; i++)
                {
                    Gizmos.DrawSphere(pointsList[i], 0.1f);
                }
            }
        }
        public override void UpdateState()
        {
            base.UpdateState();
            if (nextPointIndex < pointsList.Length)
            {
                // Calculate the direction between the previous and next points
                Vector3 previousPoint = pointsList[nextPointIndex == 0 ? pointsList.Length - 1 : nextPointIndex - 1];
                Vector3 nextPoint = pointsList[nextPointIndex];
                Vector3 directionBetweenPoints = (nextPoint - previousPoint).normalized;

                // Smoothly move towards the next point
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    nextPoint,
                    movementSpeed * Time.deltaTime
                );

                // Rotate based on the direction between the previous and next points
                if (directionBetweenPoints != Vector3.zero) // Avoid errors when direction is zero
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionBetweenPoints);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * rotationSpeed // rotationSpeed controls how quickly the rotation happens
                    );
                }

                // Check if the object is close enough to the next point
                float distanceToNextPoint = Vector3.Distance(transform.position, nextPoint);
                if (distanceToNextPoint < 0.1f) // Threshold to determine if the point is reached
                {
                    nextPointIndex++;
                    if (nextPointIndex >= pointsList.Length)
                    {
                        nextPointIndex = 0; // Loop back to the first point
                                            //Reverse 
                        Array.Reverse(pointsList);
                        transform.position = pointsList[0]; // Reset position to the start point
                        transform.rotation = Quaternion.LookRotation((pointsList[1] - pointsList[0]).normalized); // Reset rotation
                    }
                }
            }
        }
        public override void ResetState()
        {
            base.ResetState();
            nextPointIndex = 1;
            transform.position = pointsList[0];
            transform.LookAt(pointsList[1]);
        }
        #endregion

        #region Private Methods
        private Vector3[] GeneratePath(BezierKeyframe[] bezierKeyframes, float resolution)
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

        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
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
        #endregion

    }
}
