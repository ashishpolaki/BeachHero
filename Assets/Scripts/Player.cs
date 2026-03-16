using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BeachHero
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Animator boatAnimator;
        [SerializeField] private MagnetEffect magnetEffect;
        [SerializeField] private Transform boatGraphicsHolder;
        [SerializeField] private GameObject normalBoostObj;
        [SerializeField] private GameObject speedBoostObj;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float speedMultiplier;
        [SerializeField] private ParticleSystem explosionParticle;

        private Boat currentBoat;
        private Vector3[] pointsList;
        private bool canStartMovement;
        private bool isSpeedBoostEnabled;
        private float boatRotationSpeed;
        private int nextPointIndex;
        private int sinkingAnimHash = Animator.StringToHash(StringUtils.SINKING_ANIM);
        private int idleAnimHash = Animator.StringToHash(StringUtils.IDLE_ANIM);
        private Dictionary<int, GameObject> boatObjects = new Dictionary<int, GameObject>();

        public float MovementSpeed => movementSpeed;

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
            //Collide with DrownCharacter
            if (other.CompareTag(StringUtils.CHARACTER_TAG))
            {
                DrownCharacter drownCharacter = other.GetComponent<DrownCharacter>();
                drownCharacter.OnPickUp();
            }

            //Collide with Collectable
            ICollectable collectable = other.GetComponent<ICollectable>();
            if (collectable != null)
            {
                if (!collectable.IsCollected)
                {
                    collectable.Collect();
                }
            }

            //Collide with Obstacle
            if (other.CompareTag(StringUtils.OBSTACLE_TAG))
            {
                IObstacle obstacle = other.GetComponent<IObstacle>();
                if (obstacle == null)
                {
                    obstacle = other.GetComponentInParent<IObstacle>();
                }
                if (obstacle != null)
                {
                    StopMovement();
                    if (obstacle.ObstacleType == ObstacleType.Whirlpool)
                    {
                        boatAnimator.enabled = false;
                    }
                    else
                    {
                        OnBoatCollided();
                        explosionParticle.gameObject.SetActive(true);
                        //   explosionParticle.transform.position = other.ClosestPoint(transform.position);
                        explosionParticle.transform.position = transform.position;
                        explosionParticle.Play();
                        AudioController.GetInstance.PlaySound(AudioType.BoomExplosion);
                        CameraController.GetInstance.ShakeActiveCamera();
                    }
                    obstacle.Hit();
                }
            }

            //Collided with Shore
            if (other.CompareTag(StringUtils.GROUND_TAG))
            {
                StopMovement();
                GameController.GetInstance.OnLevelFailed(LevelFailDelayType.Short);
                OnBoatCollided();
            }
        }
        #endregion

        #region Powerups
        public void ActivateSpeedPowerup()
        {
            isSpeedBoostEnabled = true;
            movementSpeed *= speedMultiplier;
            boatRotationSpeed *= speedMultiplier;
        }
        public void ActivateMagnetPowerup()
        {
            magnetEffect.gameObject.SetActive(true);
            magnetEffect.PlayRippleEffect();
        }
        private void DeactivateMagnetPowerup()
        {
            magnetEffect.StopRippleEffect();
        }
        #endregion

        #region Boat
        private void OnBoatCollided()
        {
            boatAnimator.SetTrigger(sinkingAnimHash);
        }
        public void UpdateBoat(int boatIndex, int boatColorIndex, float speed, GameObject boatPrefab)
        {
            movementSpeed = speed;
            boatRotationSpeed = rotationSpeed;

            //Boat
            foreach (var boatObject in boatObjects.Values)
            {
                if (boatObject.activeSelf)
                {
                    boatObject.SetActive(false);
                }
            }
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
            currentBoat.SetBoatInit(boatIndex, boatColorIndex);
        }
        #endregion

        #region Animation
        public async void PlayVictoryAnimation()
        {
            currentBoat.PlayVictoryAnimation();
            CameraController.GetInstance.OnPlayerBlendCamera(this.transform);
            AudioController.GetInstance.PlaySound(AudioType.Joy);
            await Task.Delay(1000); // Wait for 1 seconds to allow the animation to play
            GameController.GetInstance.LevelWinFeedback();
        }
        #endregion

        #region Start/Stop Movement
        public void StopMovement()
        {
            ResetState();
        }
        public void StartMovement(Vector3[] pointsList)
        {
            normalBoostObj.SetActive(!isSpeedBoostEnabled);
            if (isSpeedBoostEnabled)
            {
                speedBoostObj.SetActive(true);
            }
            canStartMovement = true;
            this.pointsList = pointsList;
        }
        #endregion

        private void ResetState()
        {
            canStartMovement = false;
            if (isSpeedBoostEnabled)
            {
                speedBoostObj.SetActive(false);
                isSpeedBoostEnabled = false;
            }
            normalBoostObj.SetActive(false);
            pointsList = new Vector3[0];
            nextPointIndex = 1;
            DeactivateMagnetPowerup();
        }
        public void Init()
        {
            boatAnimator.SetTrigger(idleAnimHash);
            explosionParticle.gameObject.SetActive(false);
            explosionParticle.Stop();
            ResetState();
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
                        Time.deltaTime * boatRotationSpeed // rotationSpeed controls how quickly the rotation happens
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
                            PlayVictoryAnimation();
                        }
                        else
                        {
                            GameController.GetInstance.OnLevelFailed(LevelFailDelayType.None);
                        }
                        StopMovement();
                    }
                }
            }
        }
    }
}
