using UnityEngine;

namespace BeachHero
{
    public class MovingObstacle : Obstacle
    {
        #region Inspector Variables
        [SerializeField] private LineRenderer pathRenderer;
        [SerializeField] private float rotationSpeedMultiplier = 0.3f;
        #endregion

        #region Private Variables
        private BezierKeyframe[] keyframes;
        private float movementSpeed = 5f;
        private bool isLoopedMovement;
        private bool isInverseDirection;
        private bool isMovementActive;
        private int currentIndex = 0;
        private float progress = 0f;
        private int direction = 1;
        #endregion

        #region Public Methods
        public virtual void Init(MovingObstacleData movingObstacleData)
        {
            ResetState();
            isMovementActive = true;
            keyframes = movingObstacleData.bezierKeyframes;
            isLoopedMovement = movingObstacleData.loopedMovement;
            isInverseDirection = movingObstacleData.inverseDirection;
            movementSpeed = movingObstacleData.movementSpeed;
            currentIndex = 0;
            direction = 1;
            transform.position = keyframes[0].position;
            transform.rotation = Quaternion.LookRotation((keyframes[1].position - keyframes[0].position).normalized);
            var pointsList = BezierCurveUtils.GeneratePath(keyframes, movingObstacleData.resolution);
            pathRenderer.positionCount = pointsList.Length;
            // Ensure the first point is at a height of 0.5f to fix rendering bug on water
            pointsList[0].y = 0.5f;
            pathRenderer.SetPositions(pointsList);
        }
        public override void UpdateState()
        {
            if (isMovementActive == false)
                return;
            base.UpdateState();

            Vector3 p0 = keyframes[currentIndex].position;
            Vector3 p1 = p0 + keyframes[currentIndex].outTangentLocal;
            Vector3 p2 = keyframes[currentIndex + 1].position + keyframes[currentIndex + 1].inTangentLocal;
            Vector3 p3 = keyframes[currentIndex + 1].position;

            // Apply Position and Rotation
            Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, progress);
            Vector3 forward = BezierCurveUtils.GetTangent(p0, p1, p2, p3, progress).normalized;
            transform.position = pos;
            if (forward != Vector3.zero) // Avoid errors when forward is zero
            {
                if (direction == 1)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * (rotationSpeedMultiplier * movementSpeed));
                }
                else
                {
                    Quaternion targetRotation = Quaternion.LookRotation(-forward, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * (rotationSpeedMultiplier * movementSpeed));
                }
            }

            // Threshold to determine if the point is reached
            progress += (direction * Time.deltaTime * movementSpeed);
            bool reachedEnd = (direction == 1 && progress >= 1f) || (direction == -1 && progress <= 0f);
            if (reachedEnd)
            {
                // Wrap progress for next segment
                progress = (direction == 1) ? progress - 1f : progress + 1f;
                currentIndex += direction;

                // Looping / reverse handling
                bool loopPassedEnd = (direction == 1 && currentIndex >= keyframes.Length - 1) ||
                                     (direction == -1 && currentIndex < 0);

                if (isLoopedMovement && loopPassedEnd)
                {
                    direction *= isInverseDirection ? -1 : 1; // Reverse direction if inverse is set
                    progress = direction == 1 ? 0 : 1; // Reset progress to start or end of the segment
                    currentIndex = direction == 1 ? 0 : keyframes.Length - 2; // Start at the last point if inverse direction
                    if (isInverseDirection)
                    {
                        transform.position = keyframes[currentIndex].position;
                        transform.rotation = Quaternion.LookRotation((keyframes[currentIndex + 1].position - keyframes[currentIndex].position).normalized);
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
            currentIndex = 0;
            direction = 1;
            isMovementActive = false;
        }
    }
}
