using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Animator boatAnimator;
        [SerializeField] private ParticleSystem magnetParticle;
        [SerializeField] private Transform boatGraphicsHolder;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float speedMultiplier;

        private Boat currentBoat;
        private Vector3[] pointsList;
        private bool canStartMovement;
        private int nextPointIndex;
        private int sinkingAnimHash = Animator.StringToHash(StringUtils.SINKING_ANIM);
        private int idleAnimHash = Animator.StringToHash(StringUtils.IDLE_ANIM);

        private Dictionary<int, GameObject> boatObjects = new Dictionary<int, GameObject>();
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
        private void OnTriggerEnter(Collider other)
        {
            if (canStartMovement == false)
            {
                return;
            }
            if (other.CompareTag(StringUtils.CHARACTER_TAG))
            {
                DrownCharacter drownCharacter = other.GetComponent<DrownCharacter>();
                drownCharacter.OnPickUp();
            }
            ICollectable collectable = other.GetComponent<ICollectable>();
            if (collectable != null)
            {
                collectable.Collect();
            }

            if (other.CompareTag(StringUtils.OBSTACLE_TAG))
            {
                IObstacle obstacle = other.GetComponent<IObstacle>();
                if (obstacle != null)
                {
                    StopMovement();
                    obstacle.Hit();
                    if (obstacle.ObstacleType == ObstacleType.WaterHole)
                    {
                        boatAnimator.enabled = false;
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

        public void PlayVictoryAnimation()
        {
            currentBoat.PlayVictoryAnimation();
        }
        public void StopMovement()
        {
            canStartMovement = false;
            magnetParticle.Stop();
            magnetParticle.gameObject.SetActive(false);
        }
        private void OnBoatCollided()
        {
            boatAnimator.SetTrigger(sinkingAnimHash);
        }
        public void ActivateSpeedPowerup()
        {
            movementSpeed *= speedMultiplier;
            rotationSpeed *= speedMultiplier;
        }
        public void ActivateCoinMagnetPowerup()
        {
            magnetParticle.gameObject.SetActive(true);
            magnetParticle.Play();
        }
        public void StartMovement(Vector3[] pointsList)
        {
            canStartMovement = true;
            this.pointsList = pointsList;
        }
        public void Init()
        {
            magnetParticle.Stop();
            magnetParticle.gameObject.SetActive(false);
            boatAnimator.Play(idleAnimHash, -1, Random.Range(0f, 1f));
            canStartMovement = false;
            nextPointIndex = 1;
            pointsList = new Vector3[0];
        }
        public void GameStart(int boatIndex,float speed, GameObject boatPrefab)
        {
            foreach (var boatObject in boatObjects.Values)
            {
                boatObject.SetActive(false);
            }
            //Spawn Boat
            if (boatObjects.ContainsKey(boatIndex))
            {
                boatObjects.TryGetValue(boatIndex, out GameObject existingBoat);
                existingBoat.SetActive(true);
                currentBoat = existingBoat.GetComponent<Boat>();
            }
            else
            {
                if (boatPrefab != null)
                {
                    var boat = Instantiate(boatPrefab, boatGraphicsHolder);
                    boatObjects.Add(boatIndex, boat);
                    currentBoat = boat.GetComponent<Boat>();
                }
            }
            movementSpeed = speed; 
            currentBoat.PlayIdleAnimation();
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
                        if (GameController.GetInstance.LevelController.IsLevelPassed)
                        {
                            GameController.GetInstance.OnLevelPassedCameraEffect();
                            PlayVictoryAnimation();
                        }
                        else
                        {
                            GameController.GetInstance.OnLevelFailed();
                        }
                    }
                }
            }
        }
    }
}
