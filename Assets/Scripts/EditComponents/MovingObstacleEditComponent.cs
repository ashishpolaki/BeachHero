using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    public class MovingObstacleEditComponent : MonoBehaviour
    {
        [ReadOnly] public Vector3[] pathPoints;
        public ObstacleType obstacleType;
        public MovingObstacleMovementType movementType;
        public BezierKeyframe[] Keyframes;
        public BezierKeyframe[] TempKeyFrames;
        public float resolution;
        public float movementSpeed;
        public Vector3 offsetPosition;
        public Vector3 offsetRotation;
        public float circleRadius;
        public int circleSegments;
        public bool loopedMovement;
        public bool inverseDirection;
        private LineRenderer pathRenderer;
        public bool canEditKeyFramesInScene;

        public void AddKeyFrame(BezierKeyframe newKeyframe)
        {
            if (Keyframes == null)
            {
                Keyframes = new BezierKeyframe[0];
            }

            // Use ArrayUtility.Add with the backing field
            ArrayUtility.Add(ref Keyframes, newKeyframe);
            ArrayUtility.Add(ref TempKeyFrames, newKeyframe);
        }
        public void RemoveKeyFrame()
        {
            if (Keyframes == null || Keyframes.Length == 0)
                return;

            // Use ArrayUtility.Remove with the backing field
            ArrayUtility.RemoveAt(ref Keyframes, Keyframes.Length - 1);
            ArrayUtility.RemoveAt(ref TempKeyFrames, TempKeyFrames.Length - 1);
        }
        public void RemoveAllKeyFrames()
        {
            if (Keyframes == null || Keyframes.Length == 0)
                return;

            // Use ArrayUtility.Clear with the backing field
            ArrayUtility.Clear(ref Keyframes);
            ArrayUtility.Clear(ref TempKeyFrames);
        }
        public void SetKeyFrames(BezierKeyframe[] _keyFrames)
        {
            if (_keyFrames == null)
                return;

            Keyframes = new BezierKeyframe[_keyFrames.Length];
            TempKeyFrames = new BezierKeyframe[_keyFrames.Length];
            for (int i = 0; i < _keyFrames.Length; i++)
            {
                Keyframes[i] = _keyFrames[i];
                TempKeyFrames[i] = _keyFrames[i];
            }
        }

        private void OnValidate()
        {
            if (!canEditKeyFramesInScene)
            {
                ApplyOffset();
            }
        }
        private void OnDrawGizmos()
        {
            if (Keyframes == null || Keyframes.Length < 2)
                return;
            pathPoints = BezierCurveUtils.GeneratePath(Keyframes, resolution);

            Gizmos.color = Color.red;
            //Draw lines between the collected points
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
            pathRenderer.positionCount = pathPoints.Length;
            pathRenderer.SetPositions(pathPoints);
            transform.position = pathPoints[0];
            transform.LookAt(pathPoints[1]);
        }
        private void ApplyOffset()
        {
            if (Keyframes == null || Keyframes.Length < 2)
                return;
            for (int i = 0; i < Keyframes.Length; i++)
            {
                var rotationOffset = Quaternion.Euler(offsetRotation);

                Vector3 positionWithOffset = rotationOffset * TempKeyFrames[i].position + offsetPosition;
                Vector3 inTangentWithOffset = rotationOffset * TempKeyFrames[i].inTangentLocal;
                Vector3 outTangentWithOffset = rotationOffset * TempKeyFrames[i].outTangentLocal;

                Keyframes[i].position = positionWithOffset;
                Keyframes[i].inTangentLocal = inTangentWithOffset;
                Keyframes[i].outTangentLocal = outTangentWithOffset;
            }
        }
        private void GetPathRenderer()
        {
            GameObject pathRendererPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PathRenderer.prefab");
            GameObject pathRendererObject = (GameObject)PrefabUtility.InstantiatePrefab(pathRendererPrefab, transform);
            pathRenderer = pathRendererObject.GetComponent<LineRenderer>();
        }
        public void Init(MovingObstacleData movingObstacleData)
        {
            obstacleType = movingObstacleData.type;
            movementType = movingObstacleData.movementType;
            resolution = movingObstacleData.resolution;
            movementSpeed = movingObstacleData.movementSpeed;
            loopedMovement = movingObstacleData.loopedMovement;
            inverseDirection = movingObstacleData.inverseDirection;
            SetKeyFrames(movingObstacleData.bezierKeyframes);
            GetPathRenderer();
        }
    }
}
