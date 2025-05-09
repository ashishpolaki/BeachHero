using System;
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
        private bool isLoopedMovement;
        private bool isInverseDirection;
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
            isLoopedMovement = movingObstacleData.loopedMovement;
            isInverseDirection = movingObstacleData.inverseDirection;
            pointsList = BezierCurveUtils.GeneratePath(keyframes, movingObstacleData.resolution);
            movementSpeed = movingObstacleData.movementSpeed;
            resolution = (int)movingObstacleData.resolution;
            if (isInverseDirection)
            {
                Array.Reverse(pointsList);
            }
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
        public override void ResetState()
        {
            base.ResetState();
            nextPointIndex = 1;
            transform.position = pointsList[0];
            transform.LookAt(pointsList[1]);
        }
        #endregion
    }
}
