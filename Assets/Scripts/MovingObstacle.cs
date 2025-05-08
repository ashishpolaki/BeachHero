using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class MovingObstacle : Obstacle
    {
        [SerializeField] private int resolution;
        [SerializeField] private BezierKeyframe[] keyframes;
        [SerializeField] private float rotationSpeed = 0.3f;
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private int nextPointIndex;
        [SerializeField] private Vector3[] pointsList;

        public BezierKeyframe[] Keyframes
        {
            get => keyframes;
            private set => keyframes = value;
        }

        #region Unity Methods
        private void Update()
        {
            if (nextPointIndex < pointsList.Length)
            {
                float distanceToNextPoint = Vector3.Distance(transform.position, pointsList[nextPointIndex]);

                if (distanceToNextPoint < movementSpeed * Time.deltaTime)
                {
                    nextPointIndex++;
                    if (nextPointIndex >= pointsList.Length)
                    {
                        nextPointIndex = 0;
                    }
                }
                Quaternion oldRotation = transform.rotation;
                transform.LookAt(pointsList[nextPointIndex]);
                transform.position = Vector3.Lerp(transform.position, transform.position + transform.forward * movementSpeed * Time.deltaTime, 0.3f);
                transform.rotation = Quaternion.Lerp(oldRotation, transform.rotation, rotationSpeed);
            }
        }
        private void Reset()
        {
            nextPointIndex = 1;
        }
        #endregion

        #region Public Methods
        public void Init(MovingObstacleData movingObstacleData)
        {
            nextPointIndex = 1;
            keyframes = movingObstacleData.bezierKeyframes;
            pointsList = GeneratePath(keyframes, movingObstacleData.circleSegments);
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
