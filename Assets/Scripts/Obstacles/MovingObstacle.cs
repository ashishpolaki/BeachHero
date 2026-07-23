using LitMotion;
using UnityEngine;

namespace BeachHero
{
    public class MovingObstacle : Obstacle
    {
        #region Inspector Variables
        //  [SerializeField] private LineRenderer pathRenderer;
        [SerializeField] private float rotationSpeedMultiplier = 0.3f;
        [SerializeField] private int samples = 20;
        [SerializeField] private float movementSpeed = 5f;

        [Header("DashByHit Settings")]
        [SerializeField] private float dashByHitDistance = 3f;
        [SerializeField] private float dashByHitUpOffset = 1f;
        [SerializeField] private float dashByHitExtraForward = 2f; // added to forward distance for down position
        [SerializeField] private float dashByHitDownOffset = 2f;
        [SerializeField] private float dashByHitSpinStrength = 120f;
        [SerializeField] private float dashByHitMainSpinDegrees = 270f;
        [SerializeField] private float dashByHitMoveUpDuration = 0.35f;
        [SerializeField] private float dashByHitRotateDuration = 0.7f;
        [SerializeField] private float dashByHitMoveDownDuration = 0.35f;
        [SerializeField] private Ease dashByHitEaseUp = Ease.OutQuad;
        [SerializeField] private Ease dashByHitEaseRotate = Ease.Linear;
        [SerializeField] private Ease dashByHitEaseDown = Ease.InQuad;
        [SerializeField] private float dashByHitScaleMultiplier = 1.5f;
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
            rotationSpeedMultiplier = movingObstacleData.rotationSpeedMultiplier;
            currentIndex = 0;
            direction = 1;
            transform.position = keyframes[0].position;
            transform.rotation = Quaternion.LookRotation((keyframes[1].position - keyframes[0].position).normalized);
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
                currentIndex += direction;
                if (direction == 1)
                {
                    distanceTravelled = distanceTravelled - segLength;
                }
                else
                {
                    if (currentIndex < 0)
                    {
                        distanceTravelled = 0f; // reset to start
                    }
                    else
                        distanceTravelled = segmentLengths[currentIndex];
                }

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
                            if (currentIndex + 1 < keyframes.Length)
                            {
                                transform.rotation = Quaternion.LookRotation((keyframes[currentIndex + 1].position - keyframes[currentIndex].position).normalized);
                            }
                        }
                        else
                        {
                            if (currentIndex - 1 >= 0)
                            {
                                transform.rotation = Quaternion.LookRotation((keyframes[currentIndex - 1].position - keyframes[currentIndex].position).normalized);
                            }
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

        public override void HitByDash(Vector3 hitDirection = default)
        {
            base.HitByDash();
            isMovementActive = false;
            Vector3 dir = hitDirection.normalized;
            Vector3 startPos = transform.position;
            Vector3 midPos = startPos + (dir * dashByHitDistance) + (Vector3.up * dashByHitUpOffset);   // up + forward
            Vector3 downPos = startPos + (dir * (dashByHitDistance + dashByHitExtraForward)) + (Vector3.down * dashByHitDownOffset);

            Vector3 targetRotation = new Vector3(
               -dir.z * dashByHitSpinStrength,   // forward/back flip
               dir.x * dashByHitSpinStrength,    // side twist
               transform.rotation.z + dashByHitMainSpinDegrees);                   // main spin

            TweenSequence sequence = new TweenSequence(LSequence.Create());
            // Cache scales into variables so they're consistent and configurable
            Vector3 startScale = transform.localScale;
            Vector3 midScale = startScale * dashByHitScaleMultiplier;
            Vector3 endScale = Vector3.zero;

            sequence.Insert(0, TweenManager.Move(transform, startPos, midPos, dashByHitMoveUpDuration, ease: dashByHitEaseUp).Handle);
            sequence.Insert(0, TweenManager.Scale(startScale, midScale, transform, dashByHitMoveUpDuration, ease: dashByHitEaseUp).Handle);
            sequence.Insert(0f, TweenManager.RotateEulerAngles(transform, transform.eulerAngles,
                targetRotation, dashByHitRotateDuration, ease: dashByHitEaseRotate).Handle);
            sequence.Insert(dashByHitMoveUpDuration, TweenManager.Move(transform, midPos, downPos, dashByHitMoveDownDuration, ease: dashByHitEaseDown).Handle);
            sequence.Insert(dashByHitMoveUpDuration, TweenManager.Scale(midScale, endScale, transform, dashByHitMoveDownDuration, ease: dashByHitEaseDown).Handle);
            sequence.InitializeHandle();
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
