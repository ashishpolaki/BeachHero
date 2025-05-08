using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    public class MovingObstacleEditComponent : MonoBehaviour
    {
        public Vector3[] pathPoints;
        public ObstacleType obstacleType;
        public MovingObstacleMovementType movementType;
        public BezierKeyframe[] Keyframes;
        public float resolution;
        public float movementSpeed;
        public Vector3 offsetPosition;
        public Vector3 offsetRotation;
        public float circleRadius;
        public int circleSegments;
        public bool loopedMovement;
        public bool inverseDirection;
        private LineRenderer pathRenderer;
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
        }

        private void ShowPath()
        {
            GameObject pathRendererPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PathRenderer.prefab");
            GameObject pathRendererObject = (GameObject)PrefabUtility.InstantiatePrefab(pathRendererPrefab,transform);
            pathRenderer = pathRendererObject.GetComponent<LineRenderer>();
        }

        public void Init(MovingObstacleData movingObstacleData)
        {
            obstacleType = movingObstacleData.type;
            movementType = movingObstacleData.movementType;
            Keyframes = movingObstacleData.bezierKeyframes;
            resolution = movingObstacleData.resolution;
            movementSpeed = movingObstacleData.movementSpeed;
            offsetPosition = movingObstacleData.offsetPosition;
            offsetRotation = movingObstacleData.offsetRotation;
            circleRadius = movingObstacleData.circleRadius;
            circleSegments = (int)movingObstacleData.circleSegments;
            loopedMovement = movingObstacleData.loopedMovement;
            inverseDirection = movingObstacleData.inverseDirection;

            pathPoints = BezierCurveUtils.GeneratePath(Keyframes, resolution);
            transform.position = pathPoints[0];
            transform.LookAt(pathPoints[1]);
            ShowPath();
        }
    }
}
