#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    public class MovingObstacleEditTool : MonoBehaviour
    {
        [ReadOnly]
        public Vector3[] pathPoints;
        public ObstacleType obstacleType;
        public BezierKeyframe[] Keyframes;
        public float resolution;
        public float movementSpeed = 4;
        public float rotationSpeedMultiplier = 0.3f;
        [HideInInspector] public Vector3 offsetPosition;
        [HideInInspector] public Vector3 offsetRotation;
        public bool loopedMovement;
        public bool inverseDirection;
        private LineRenderer pathRenderer;
        public bool canEditKeyFramesInScene;
        private bool canDrawGizmos;

        // Editor-movement state & helpers
        private int samples = 10;
        private float[] segmentLengths;
        private int direction = 1;
        public float startDistanceOffset = 0f;

        #region Keyframe Management
        public void AddKeyFrame(BezierKeyframe newKeyframe)
        {
            if (Keyframes == null)
            {
                Keyframes = new BezierKeyframe[0];
            }

            // Use ArrayUtility.Add with the backing field
            ArrayUtility.Add(ref Keyframes, newKeyframe);
        }
        public void RemoveKeyFrame()
        {
            if (Keyframes == null || Keyframes.Length == 0)
                return;

            // Use ArrayUtility.Remove with the backing field
            ArrayUtility.RemoveAt(ref Keyframes, Keyframes.Length - 1);
        }
        public void RemoveAllKeyFrames()
        {
            if (Keyframes == null || Keyframes.Length == 0)
                return;

            // Use ArrayUtility.Clear with the backing field
            ArrayUtility.Clear(ref Keyframes);
        }
        public void AddKeyframeAtIndex(int index, BezierKeyframe newKeyframe)
        {
            if (Keyframes == null || index < 0 || index > Keyframes.Length)
                return;

            // Use ArrayUtility.Insert with the backing field
            ArrayUtility.Insert(ref Keyframes, index + 1, newKeyframe);
        }
        public void RemoveKeyframeAtIndex(int index)
        {
            if (Keyframes == null || Keyframes.Length == 0 || index < 0 || index >= Keyframes.Length)
                return;

            // Use ArrayUtility.RemoveAt with the backing field
            ArrayUtility.RemoveAt(ref Keyframes, index);
        }
        public void SetKeyFrames(BezierKeyframe[] _keyFrames)
        {
            if (_keyFrames == null)
                return;

            Keyframes = new BezierKeyframe[_keyFrames.Length];
            for (int i = 0; i < _keyFrames.Length; i++)
            {
                Keyframes[i] = _keyFrames[i];
            }
        }
        #endregion

        private void OnDrawGizmos()
        {
            if (!canDrawGizmos) return;

            if (Keyframes == null || Keyframes.Length < 2)
            {
                if (Keyframes != null && Keyframes.Length == 1)
                {
                    transform.position = Keyframes[0].position;
                }
                return;
            }
            pathPoints = BezierCurveUtils.GeneratePath(Keyframes, resolution);

            Gizmos.color = Color.red;
            //Draw lines between the collected points
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                Gizmos.DrawSphere(pathPoints[i], 0.1f);
                // Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
            pathRenderer.positionCount = pathPoints.Length;
            pathRenderer.SetPositions(pathPoints);
        }
        private void GetPathRenderer()
        {
            GameObject pathRendererPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PathLineRenderer.prefab");
            GameObject pathRendererObject = (GameObject)PrefabUtility.InstantiatePrefab(pathRendererPrefab, transform);
            pathRenderer = pathRendererObject.GetComponent<LineRenderer>();
        }
        public void Init(MovingObstacleData movingObstacleData)
        {
            obstacleType = movingObstacleData.type;
            resolution = movingObstacleData.resolution;
            movementSpeed = movingObstacleData.movementSpeed;
            rotationSpeedMultiplier = movingObstacleData.rotationSpeedMultiplier;
            loopedMovement = movingObstacleData.loopedMovement;
            inverseDirection = movingObstacleData.inverseDirection;
            SetKeyFrames(movingObstacleData.bezierKeyframes);
            GetPathRenderer();
            canDrawGizmos = true;
            // Precompute segment lengths for editor preview
            if (Keyframes != null && Keyframes.Length > 1)
            {
                segmentLengths = new float[Keyframes.Length - 1];
                for (int i = 0; i < Keyframes.Length - 1; i++)
                {
                    segmentLengths[i] = ApproximateLength(
                        Keyframes[i].position,
                        Keyframes[i].position + Keyframes[i].outTangentLocal,
                        Keyframes[i + 1].position + Keyframes[i + 1].inTangentLocal,
                        Keyframes[i + 1].position,
                        samples
                    );
                }

                transform.position = Keyframes[0].position;
                if (Keyframes.Length > 1)
                    transform.rotation = Quaternion.LookRotation((Keyframes[1].position - Keyframes[0].position).normalized);
            }
        }
        // Editor-time movement preview (moved from MovingObstacle)
        public void UpdateStateEditor(float currentTime)
        {
            if (Keyframes == null || Keyframes.Length < 2 || segmentLengths == null || segmentLengths.Length < 1)
                return;

            int currentIndex = 0;
            float distanceTravelled = 0f;
            // total length of path
            float totalLen = 0f;
            for (int i = 0; i < segmentLengths.Length; i++) totalLen += segmentLengths[i];
            if (totalLen <= 0f) return;

            // compute distance travelled along path for absolute time
            float rawDist = movementSpeed * currentTime + startDistanceOffset;

            float dist = rawDist;

            if (loopedMovement)
            {
                if (inverseDirection)
                {
                    // ping-pong: period = 2 * totalLen
                    float period = 2f * totalLen;
                    float mod = Mathf.Repeat(rawDist, period);
                    if (mod <= totalLen)
                    {
                        dist = mod;
                        direction = 1;
                    }
                    else
                    {
                        dist = period - mod;
                        direction = -1;
                    }
                }
                else
                {
                    // loop wrap
                    dist = Mathf.Repeat(rawDist, totalLen);
                    direction = 1;
                }
            }
            else
            {
                // clamp to ends
                dist = Mathf.Clamp(rawDist, 0f, totalLen);
                direction = 1;
            }

            // find segment index and distance in segment
            int segCount = segmentLengths.Length;
            float accumulated = 0f;
            int idx = 0;
            for (int i = 0; i < segCount; i++)
            {
                if (accumulated + segmentLengths[i] >= dist)
                {
                    idx = i;
                    break;
                }
                accumulated += segmentLengths[i];
                if (i == segCount - 1) idx = segCount - 1;
            }

            float distanceInSegment = Mathf.Clamp(dist - accumulated, 0f, segmentLengths[idx]);

            // update tracking fields so subsequent editor calls have consistent state
            currentIndex = idx;
            distanceTravelled = distanceInSegment;

            // compute t for bezier and evaluate position/tangent
            float t = GetTForDistance(
                Keyframes[idx].position,
                Keyframes[idx].position + Keyframes[idx].outTangentLocal,
                Keyframes[idx + 1].position + Keyframes[idx + 1].inTangentLocal,
                Keyframes[idx + 1].position,
                distanceInSegment,
                segmentLengths[idx]
            );

            Vector3 p0 = Keyframes[idx].position;
            Vector3 p1 = p0 + Keyframes[idx].outTangentLocal;
            Vector3 p2 = Keyframes[idx + 1].position + Keyframes[idx + 1].inTangentLocal;
            Vector3 p3 = Keyframes[idx + 1].position;

            Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
            Vector3 tangent = BezierCurveUtils.GetTangent(p0, p1, p2, p3, t).normalized;

            transform.position = pos;

            if (tangent.sqrMagnitude > 0.0001f)
            {
                // respect current direction
                Vector3 dirVec = (direction == 1) ? tangent : -tangent;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirVec, Vector3.up),
                    Time.deltaTime * rotationSpeedMultiplier * movementSpeed);
            }

            // mark dirty for editor repaint
            EditorUtility.SetDirty(this);
            SceneView.RepaintAll();
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
            int localSamples = 30; // higher = more accurate
            float accumulated = 0f;
            Vector3 prev = p0;

            for (int i = 1; i <= localSamples; i++)
            {
                float t = i / (float)localSamples;
                Vector3 point = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
                float d = Vector3.Distance(prev, point);
                if (accumulated + d >= dist)
                {
                    float overshoot = dist - accumulated;
                    return Mathf.Lerp((i - 1f) / localSamples, t, overshoot / d);
                }
                accumulated += d;
                prev = point;
            }
            return 1f;
        }
    }
}
#endif
