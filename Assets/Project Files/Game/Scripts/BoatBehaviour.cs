using UnityEngine;

namespace Bokka.BeachRescue
{
    public class BoatBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        public float movementSpeed;

        [Header("References")]
        [SerializeField] Rigidbody graphicsRB;
        [SerializeField] Transform graphicsOriginTransform;
        [SerializeField] Transform graphicsHolderTransform;
        [SerializeField] Transform rotationPointerTransform;
        [SerializeField] Animator graphicsAnimator;
        [SerializeField] GameObject particleRef;

        private StartPointBehaviour startPoint;
        private TrailBehaviour trailRef;

        private Transform transformRef;
        private Vector3[] pointsList;

        public bool IsMovementActive { get; private set; }
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

        public void Init(StartPointBehaviour startPointRef, TrailBehaviour trailRef)
        {
            isOnTheStartPoint = true;

            graphicsOriginTransform.gameObject.SetActive(true);
            graphicsOriginTransform.position = transformRef.position;
            graphicsOriginTransform.rotation = new Quaternion();
            graphicsOriginTransform.SetParent(null);
            graphicsRB.isKinematic = true;
            startPoint = startPointRef;
            this.trailRef = trailRef;

            graphicsAnimator.Play(idleAnimHash, -1, Random.Range(0f, 1f));
            pointsList = null;

            particleRef.SetActive(false);
        }

        public void StarMovement(Vector3[] pointsList)
        {
            this.pointsList = pointsList;

            transformRef.position = graphicsOriginTransform.position = pointsList[0];
            transformRef.LookAt(pointsList[1]);
            graphicsOriginTransform.rotation = transformRef.rotation;

            nextPointIndex = 1;
            trailRef.RemovePointOnTheBeginning();

            graphicsRB.isKinematic = false;
            IsMovementActive = true;
            isOnTheStartPoint = false;
            particleRef.SetActive(true);
        }

        public void StopMovement()
        {
            IsMovementActive = false;
            graphicsRB.isKinematic = true;

            particleRef.SetActive(false);
        }

        public void ResetPosition()
        {
            StopMovement();

            transformRef.position = graphicsOriginTransform.position = startPoint.Position;
            transformRef.rotation = graphicsOriginTransform.rotation = startPoint.Rotation;

            graphicsAnimator.Play(idleAnimHash, -1, Random.Range(0f, 1f));
            isOnTheStartPoint = true;
        }

        private void FixedUpdate()
        {
            if (!IsMovementActive)
                return;

            if (pointsList == null)
            {
                StopMovement();
                return;
            }

            if (nextPointIndex < pointsList.Length)
            {
                float distanceToNextPoint = Vector3.Distance(transformRef.position, pointsList[nextPointIndex]);

                if (distanceToNextPoint < movementSpeed * Time.deltaTime)
                {
                    nextPointIndex++;
                    trailRef.RemovePointOnTheBeginning();

                    if (nextPointIndex >= pointsList.Length)
                    {
                        OnMovementCompleted();
                        return;
                    }
                }

                transformRef.LookAt(pointsList[nextPointIndex]);

                transformRef.position = Vector3.Lerp(transformRef.position, transformRef.position + transformRef.forward * movementSpeed * Time.fixedDeltaTime, 0.3f);

                graphicsRB.MovePosition(transformRef.position);
                graphicsRB.AddForce(Vector3.one * 0.01f);


                Quaternion oldRotation = rotationPointerTransform.rotation;
                rotationPointerTransform.LookAt(transformRef.position + transformRef.forward);
                rotationPointerTransform.rotation = Quaternion.Lerp(graphicsRB.rotation, rotationPointerTransform.rotation, 0.3f);

                graphicsRB.MoveRotation(rotationPointerTransform.rotation);
            }
        }

        private void OnMovementCompleted()
        {
            StopMovement();

            LevelController.Instance.OnMovementCompleted();
        }

        public void OnCollisionWithObstacle()
        {
            if (IsMovementActive)
            {
#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_HARD);
#endif

                OnMovementCompleted();
                graphicsAnimator.SetTrigger(sinkingAnimHash);

                LevelController.OnBoatDrowned();
            }
        }

        private void OnDisable()
        {
            if (graphicsOriginTransform != null)
            {
                graphicsOriginTransform.gameObject.SetActive(false);
            }
        }


    }
}