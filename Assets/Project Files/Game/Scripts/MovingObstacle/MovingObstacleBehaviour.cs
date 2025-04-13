using System.Collections.Generic;
using UnityEngine;
using static Watermelon.BeachRescue.BoatCollisionDetector;

namespace Watermelon.BeachRescue
{
    public class MovingObstacleBehaviour : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        public float pathPointsOffset;

        [Header("References")]
        [SerializeField] Transform graphicsOriginTransform;
        [SerializeField] Transform graphicsHolderTransform;
        [SerializeField] Transform rotationPointerTransform;
        [SerializeField] Animator graphicsAnimator;
        [SerializeField] GameObject particleRef;

        private MovingObstacleSave data;
        private Transform transformRef;
        private IPool pathRendererPool;
        private MovingObstaclePathRenderer pathRenderer;
        private Vector3[] pointsList;

        private bool isMovementActive;
        private bool isOnTheStartPoint;
        public bool IsOnTheStartPoint { get => isOnTheStartPoint; }

        private int nextPointIndex;

        private int idleAnimHash;
        private int sinkingAnimHash;

        private void Awake()
        {
            transformRef = transform;

            idleAnimHash = Animator.StringToHash("Idle");
            sinkingAnimHash = Animator.StringToHash("Sinking");
        }

        public void Init(MovingObstacleSave save)
        {
            pathRendererPool = PoolManager.GetPoolByName("MovingObstaclePathRenderer");

            data = save;

            pointsList = GeneratePointsList();

            transformRef.position = pointsList[0];
            transformRef.LookAt(pointsList[1]);

            pathRenderer = pathRendererPool.GetPooledObject().GetComponent<MovingObstaclePathRenderer>();
            pathRenderer.GeneratePath(data.type == MovingObstacleType.Circle ? pointsList : new List<Vector3>() { data.linearMovementStartPosition, data.linearMovementFinishPosition }.ToArray());

            isOnTheStartPoint = true;

            graphicsOriginTransform.gameObject.SetActive(true);
            graphicsOriginTransform.position = transformRef.position;
            graphicsOriginTransform.rotation = new Quaternion();

            graphicsAnimator.Play(idleAnimHash, -1, Random.Range(0f, 1f));
        }


        private Vector3[] GeneratePointsList()
        {
            if (data.type == MovingObstacleType.Linear)
            {
                return GenerateLinearPath();
            }
            else
            {
                return GenerateCircularPath();
            }
        }

        private Vector3[] GenerateLinearPath()
        {
            List<Vector3> pointsList = new List<Vector3>();
            // next code is temp
            Vector3 nextPointVector = (data.linearMovementFinishPosition - data.linearMovementStartPosition).normalized * pathPointsOffset;
            float pathLength = (data.linearMovementFinishPosition - data.linearMovementStartPosition).magnitude;
            float currentPathLength = 0;

            pointsList.Add(data.linearMovementStartPosition);

            while (currentPathLength < pathLength)
            {
                pointsList.Add(pointsList[pointsList.Count - 1] + nextPointVector);
                currentPathLength += pathPointsOffset;
            }

            if (currentPathLength > pathLength)
            {
                pointsList[pointsList.Count - 1] = data.linearMovementStartPosition;
            }

            if (data.inverseDirection)
            {
                pointsList.Reverse();
            }

            return pointsList.ToArray();
        }

        private Vector3[] GenerateCircularPath()
        {
            List<Vector3> pointsList = new List<Vector3>();
            float angleDelta = 180 * pathPointsOffset / (Mathf.PI * data.circlarMovementRadius) * Mathf.Deg2Rad;

            for (float angle = 0; angle > -2 * Mathf.PI; angle -= angleDelta)
            {
                pointsList.Add(data.circlarMovementCenter + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * data.circlarMovementRadius);
            }

            if (data.inverseDirection)
            {
                pointsList.Reverse();
            }

            return pointsList.ToArray();
        }

        public void UpdateSkin()
        {

        }

        public void StarMovement()
        {
            transformRef.position = graphicsOriginTransform.position = pointsList[0];
            transformRef.LookAt(pointsList[1]);
            graphicsOriginTransform.rotation = transformRef.rotation;

            nextPointIndex = 1;

            isMovementActive = true;
            isOnTheStartPoint = false;
            particleRef.SetActive(true);
        }

        public void StopMovement()
        {
            isMovementActive = false;
        }

        public void ResetPosition()
        {
            StopMovement();

            particleRef.SetActive(false);

            transformRef.position = graphicsOriginTransform.position = pointsList[0];
            transformRef.LookAt(pointsList[1]);
            graphicsOriginTransform.rotation = transformRef.rotation;


            graphicsAnimator.Play(idleAnimHash, -1, Random.Range(0f, 1f));
            isOnTheStartPoint = true;
        }

        private void FixedUpdate()
        {
            if (!isMovementActive)
                return;

            if (pointsList == null)
            {
                StopMovement();
                return;
            }

            if (nextPointIndex < pointsList.Length)
            {
                float distanceToNextPoint = Vector3.Distance(transformRef.position, pointsList[nextPointIndex]);

                if (distanceToNextPoint < data.movementSpeed * Time.deltaTime)
                {
                    nextPointIndex++;

                    // on final point reached
                    if (nextPointIndex >= pointsList.Length)
                    {
                        if (data.loopedMovement)
                        {
                            if (data.type == MovingObstacleType.Linear)
                            {
                                nextPointIndex = 1;
                                transformRef.position = pointsList[0];
                                transformRef.LookAt(pointsList[1]);
                                graphicsOriginTransform.rotation = transformRef.rotation;

                                particleRef.SetActive(false);
                                Tween.DelayedCall(0.2f, () => particleRef.SetActive(true));
                            }
                            else
                            {
                                nextPointIndex = 0;
                            }
                        }
                        else
                        {
                            OnMovementCompleted();
                            return;
                        }
                    }
                }

                Quaternion oldRotation = transformRef.rotation;
                transformRef.LookAt(pointsList[nextPointIndex]);

                transformRef.position = Vector3.Lerp(transformRef.position, transformRef.position + transformRef.forward * data.movementSpeed * Time.fixedDeltaTime, 0.3f);
                transformRef.rotation = Quaternion.Lerp(oldRotation, transformRef.rotation, 0.3f);
            }
        }

        private void OnMovementCompleted()
        {
            StopMovement();
        }

        private void OnDisable()
        {
            if (graphicsOriginTransform != null)
            {
                graphicsOriginTransform.gameObject.SetActive(false);
            }
        }

        public void Interact()
        {
            StopMovement();
        }

        public void Reinit()
        {
            ResetPosition();
        }

    }
}