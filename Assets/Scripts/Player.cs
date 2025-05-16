using UnityEngine;

namespace BeachHero
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float movementSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private Animator animator;
        private Vector3[] pointsList;
        private bool canStartMovement;
        private int nextPointIndex;
        private int sinkingAnimHash;
        private int idleAnimHash;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (pointsList != null && pointsList.Length > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < pointsList.Length; i++)
                {
                    Gizmos.DrawSphere(pointsList[i], 0.1f);
                }
            }
        }
#endif
        #region Unity methods
        private void Awake()
        {
            sinkingAnimHash = Animator.StringToHash("Sinking");
            idleAnimHash = Animator.StringToHash("Idle");
        }

        private void OnTriggerEnter(Collider other)
        {
            if(canStartMovement == false)
            {
                return;
            }
            if (other.CompareTag("Character"))
            {
                SavedCharacter savedCharacter = other.GetComponent<SavedCharacter>();
                savedCharacter.OnPickUp();
            }
            ICollectable collectable = other.GetComponent<ICollectable>();
            if (collectable != null)
            {
                collectable.Collect();
            }

            if (other.CompareTag("Obstacle"))
            {
                IObstacle obstacle = other.GetComponent<IObstacle>();
                if (obstacle != null)
                {
                    StopMovement();
                    obstacle.Hit();
                    if (obstacle.ObstacleType == ObstacleType.WaterHole)
                    {
                        animator.enabled = false;
                        other.GetComponent<WaterHoleObstacle>().OnPlayerHit(transform);
                    }
                    else
                    {
                        OnBoatCollided();
                    }
                }

            }
        }
        #endregion

        private void StopMovement()
        {
            canStartMovement = false;
        }
        private void OnBoatCollided()
        {
            animator.SetTrigger(sinkingAnimHash);
        }

        public void StartMovement(Vector3[] pointsList)
        {
            nextPointIndex = 1;
            canStartMovement = true;
            this.pointsList = pointsList;
        }

        #region States
        public void StartState()
        {
            animator.Play(idleAnimHash, -1, Random.Range(0f, 1f));
        }
        public void ResetState()
        {

        }
        public void UpdateState()
        {
            if (!canStartMovement)
            {
                return;
            }
            if (nextPointIndex < pointsList.Length)
            {
                // Calculate the direction between the previous and next points
                Vector3 previousPoint = pointsList[nextPointIndex == 0 ? pointsList.Length - 1 : nextPointIndex - 1];
                Vector3 nextPoint = pointsList[nextPointIndex];
                Vector3 directionBetweenPoints = (nextPoint - previousPoint).normalized;

                // Smoothly move towards the next point
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    nextPoint,
                    movementSpeed * Time.deltaTime
                );
                // rigid.MovePosition(Vector3.MoveTowards(
                //    transform.position,
                //    nextPoint,
                //    movementSpeed * Time.deltaTime
                //)); // Use Rigidbody to move the object

                // Rotate based on the direction between the previous and next points
                if (directionBetweenPoints != Vector3.zero) // Avoid errors when direction is zero
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionBetweenPoints);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * rotationSpeed // rotationSpeed controls how quickly the rotation happens
                    );
                }

                // Check if the object is close enough to the next point
                float distanceToNextPoint = Vector3.Distance(transform.position, nextPoint);
                if (distanceToNextPoint < 0.1f) // Threshold to determine if the point is reached
                {
                    nextPointIndex++;
                    if (nextPointIndex >= pointsList.Length)
                    {
                        Debug.Log("Reached the end of the path.");
                    }
                }
            }
        }
        #endregion
    }
}
