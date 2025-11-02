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
        public float movementSpeed;
        public float rotationSpeedMultiplier = 0.3f;
        public Vector3 offsetPosition;
        public Vector3 offsetRotation;
        public bool loopedMovement;
        public bool inverseDirection;
        private LineRenderer pathRenderer;
        public bool canEditKeyFramesInScene;
        private bool canDrawGizmos;

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
            ArrayUtility.Insert(ref Keyframes, index, newKeyframe);
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

            if (pathPoints.Length > 1)
            {
                //Set the position of the obstacle to the first point in the path
                transform.position = pathPoints[0];

                //Make the obstacle look at the second point in the path
                transform.LookAt(pathPoints[1]);
            }
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
        }
    }
}
#endif
