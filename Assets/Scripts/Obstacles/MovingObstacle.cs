using UnityEngine;

namespace BeachHero
{
    public class MovingObstacle : Obstacle
    {
        #region Inspector Variables
        [SerializeField] private LineRenderer pathRenderer;
        [SerializeField] private float rotationSpeedMultiplier = 0.3f;
        [SerializeField] private int samples = 20;
        [SerializeField] private float movementSpeed = 5f;
        #endregion

        #region Private Variables
        private BezierKeyframe[] keyframes;
        private bool isLoopedMovement;
        private bool isInverseDirection;
        private bool isMovementActive;
        private int currentIndex = 0;
        private int direction = 1;
        private float distanceTravelled = 0f;
        private float[] segmentLengths;
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

            // Precompute segment lengths
            segmentLengths = new float[keyframes.Length - 1];
            for (int i = 0; i < keyframes.Length - 1; i++)
            {
                segmentLengths[i] = ApproximateLength(
                    keyframes[i].position,
                    keyframes[i].position + keyframes[i].outTangentLocal,
                    keyframes[i + 1].position + keyframes[i + 1].inTangentLocal,
                    keyframes[i + 1].position,
                    samples
                );
            }
        }
        public override void UpdateState()
        {
            if (isMovementActive == false)
                return;
            base.UpdateState();

            if (currentIndex < 0 || currentIndex >= keyframes.Length - 1) return;

            float segLength = segmentLengths[currentIndex];

            // Step 1: Advance by distance
            float deltaDist = movementSpeed * Time.deltaTime;
            distanceTravelled += direction * deltaDist;

            // Step 2: Handle overshoot without snapping
            while ((direction == 1 && distanceTravelled > segLength) ||
                   (direction == -1 && distanceTravelled < 0f))
            {
                distanceTravelled = (direction == 1) ? distanceTravelled - segLength : distanceTravelled + segLength;
                currentIndex += direction;

                bool loopPassedEnd = (direction == 1 && currentIndex >= keyframes.Length - 1) ||
                                     (direction == -1 && currentIndex < 0);

                if (isLoopedMovement && loopPassedEnd)
                {
                    if (isInverseDirection)
                    {
                        direction *= -1;
                        currentIndex = Mathf.Clamp(currentIndex, 0, keyframes.Length - 2);
                        distanceTravelled = (direction == 1) ? 0 : segmentLengths[currentIndex];
                        transform.position = keyframes[currentIndex].position;
                        if (direction == 1)
                        {
                            transform.rotation = Quaternion.LookRotation((keyframes[currentIndex + 1].position - keyframes[currentIndex].position).normalized);
                        }
                        else
                        {
                            transform.rotation = Quaternion.LookRotation((keyframes[currentIndex - 1].position - keyframes[currentIndex].position).normalized);
                        }
                    }
                    else
                    {
                        currentIndex = 0;
                        distanceTravelled = Mathf.Clamp(distanceTravelled, 0f, segmentLengths[currentIndex]);
                    }
                }
                else if (loopPassedEnd)
                {
                    // stop at end
                    isMovementActive = false;
                    return;
                }

                segLength = segmentLengths[currentIndex]; // update to new segment
            }

            // Step 3: Convert distance -> progress (t)
            float t = GetTForDistance(
                keyframes[currentIndex].position,
                keyframes[currentIndex].position + keyframes[currentIndex].outTangentLocal,
                keyframes[currentIndex + 1].position + keyframes[currentIndex + 1].inTangentLocal,
                keyframes[currentIndex + 1].position,
                distanceTravelled,
                segLength
            );

            // Step 4: Apply position & rotation
            Vector3 p0 = keyframes[currentIndex].position;
            Vector3 p1 = p0 + keyframes[currentIndex].outTangentLocal;
            Vector3 p2 = keyframes[currentIndex + 1].position + keyframes[currentIndex + 1].inTangentLocal;
            Vector3 p3 = keyframes[currentIndex + 1].position;

            Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
            Vector3 tangent = BezierCurveUtils.GetTangent(p0, p1, p2, p3, t).normalized;

            transform.position = pos;

            if (tangent.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = (direction == 1) ? tangent : -tangent;
                Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSpeedMultiplier * movementSpeed
                );
            }
        }

        public override void Hit()
        {
            base.Hit();
            isMovementActive = false;
        }
        #endregion

        #region Private
        private void ResetState()
        {
            currentIndex = 0;
            direction = 1;
            isMovementActive = false;
        }

        private float ApproximateLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int samples)
        {
            float length = 0f;
            Vector3 prev = p0;
            for (int i = 1; i <= samples; i++)
            {
                float t = i / (float)samples;
                Vector3 point = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
                length += Vector3.Distance(prev, point);
                prev = point;
            }
            return length;
        }

        private float GetTForDistance(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float dist, float totalLength)
        {
            int samples = 30; // higher = more accurate
            float accumulated = 0f;
            Vector3 prev = p0;

            for (int i = 1; i <= samples; i++)
            {
                float t = i / (float)samples;
                Vector3 point = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
                float d = Vector3.Distance(prev, point);
                if (accumulated + d >= dist)
                {
                    float overshoot = dist - accumulated;
                    return Mathf.Lerp((i - 1f) / samples, t, overshoot / d);
                }
                accumulated += d;
                prev = point;
            }
            return 1f;
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (segmentLengths == null || segmentLengths.Length < 2)
                return;
            Gizmos.color = Color.red;
            //Draw Sphere
            for (int i = 0; i < keyframes.Length - 1; i++)
            {
                Gizmos.DrawSphere(keyframes[i].position, 0.05f);
            }
        }
#endif
    }
}
