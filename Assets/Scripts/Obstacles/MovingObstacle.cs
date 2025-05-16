using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeachHero
{
    public class MovingObstacle : Obstacle
    {
        #region Private Variables
        [SerializeField] private BezierKeyframe[] keyframes;
        [SerializeField] private float rotationSpeed = 0.3f;
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private int nextPointIndex;
        [SerializeField] private Vector3[] pointsList;
        [SerializeField] private float spacing = 0.5f; // Spacing between points
        private bool isLoopedMovement;
        private bool isInverseDirection;
        private bool isMovementActive;
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
            ResetState();
            isMovementActive = true;
            nextPointIndex = 1;
            keyframes = movingObstacleData.bezierKeyframes;
            isLoopedMovement = movingObstacleData.loopedMovement;
            isInverseDirection = movingObstacleData.inverseDirection;
            pointsList = BezierCurveUtils.GeneratePath(keyframes, movingObstacleData.resolution);
            pointsList = GetEvenlySpacedPoints(pointsList.ToList(), spacing).ToArray();
            movementSpeed = movingObstacleData.movementSpeed;
            if (isInverseDirection)
            {
                Array.Reverse(pointsList);
            }
            transform.position = pointsList[0];
            transform.LookAt(pointsList[1]);
        }

        public override void UpdateState()
        {
            if (isMovementActive == false)
                return;
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
                    if (isLoopedMovement && nextPointIndex >= pointsList.Length)
                    {
                        Array.Reverse(pointsList);
                        nextPointIndex = 0; // Loop back to the first point
                        transform.position = pointsList[0]; // Reset position to the start point
                        transform.rotation = Quaternion.LookRotation((pointsList[1] - pointsList[0]).normalized); // Reset rotation
                    }
                }
            }
        }

        #endregion

        public override void Hit()
        {
            base.Hit();
            isMovementActive = false;
        }
        private void ResetState()
        {
            nextPointIndex = 1;
            isMovementActive = false;
        }
        #region Spline

        private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
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

        private List<Vector3> GetEvenlySpacedPoints(List<Vector3> pathPoints, float spacing)
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
        #endregion

#if UNITY_EDITOR
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
#endif
    }
}
