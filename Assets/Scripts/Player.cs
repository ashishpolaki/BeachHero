using System;
using UnityEngine;

namespace BeachHero
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float movementSpeed;
        [SerializeField] private float rotationSpeed;
        private Vector3[] pointsList;
        private int nextPointIndex;
        private bool canStartMovement;

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

        private void OnTriggerEnter(Collider other)
        {
            ICollectable collectable = other.GetComponent<ICollectable>();
            if (collectable != null)
            {
                collectable.Collect();
            }
            Debug.Log("Trigger Entered: " + other.name);
            if (other.CompareTag("Obstacle"))
            {
               
                // Start movement when the player enters the trigger
                //   StartMovement(pointsList);
            }
        }

        public void StartMovement(Vector3[] pointsList)
        {
            nextPointIndex = 1;
            canStartMovement = true;
            this.pointsList = pointsList;
        }

        #region States
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
