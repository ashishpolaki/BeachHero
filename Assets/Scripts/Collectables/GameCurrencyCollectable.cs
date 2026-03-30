using UnityEngine;

namespace BeachHero
{
    public class GameCurrencyCollectable : Collectable
    {
        #region Inspector Variables
        [SerializeField] private GameObject graphics;
        [SerializeField] private float rotateSpeed = 200f;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private StarFish starFish;

        [Header("Float Animation Settings")]
        [SerializeField] float floatTiltSpeed = 2f;
        [SerializeField] float zTiltAmount = 5f;
        [SerializeField] float yTiltAmount = 6f;
        [SerializeField] float floatScaleAmount = 0.03f;
        #endregion

        #region Private Variables
        private Transform moveTarget;
        private bool canMoveToTarget;
        private float seed;
        private Quaternion baseRotation;
        #endregion

        public bool CanMoveToTarget => canMoveToTarget;

        public void SetTarget(Transform target)
        {
            moveTarget = target;
            canMoveToTarget = true;
        }
        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            graphics.SetActive(true);
            canMoveToTarget = false;
            starFish.Init();
            seed = Random.Range(0f, 100f);
            baseRotation = transform.rotation;
        }
        public override void UpdateState()
        {
            base.UpdateState();
            if (canMoveToTarget)
            {
                // Smoothly move towards the player
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    moveTarget.position,
                    moveSpeed * Time.deltaTime
                );

                // Optional: Add rotation for a dynamic effect
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            }
            else
            {
                OnFloatAnimation();
            }
        }

        private void OnFloatAnimation()
        {
            float t = Time.time + seed;
            float wave = Mathf.Sin(t * floatTiltSpeed);

            // Wave rotation offsets
            float zTilt = wave * zTiltAmount;
            float yTilt = wave * yTiltAmount;

            // Combine with base rotation
            Quaternion waveRotation = Quaternion.Euler(0f, yTilt, zTilt);

            transform.rotation = baseRotation * waveRotation;

            // Scale (your current version)
            float scale = 1 + wave * floatScaleAmount;
            transform.localScale = Vector3.Lerp( transform.localScale, Vector3.one * scale, Time.deltaTime);
        }

        public override void ResetState()
        {
            base.ResetState();
            starFish.StopAnimation();
            graphics.SetActive(false);
            canMoveToTarget = false;
        }
        public override void Collect()
        {
            base.Collect();
            var particle = GameController.GetInstance.PoolManager.GameCurrencyParticlePool.GetObject().GetComponent<ParticleAutoDisable>();
            particle.PlayParticle(transform.position);
            particle.SetRotation(transform.eulerAngles);
            AudioController.GetInstance.PlaySound(AudioType.Collect1);
            GameController.GetInstance.OnGameCurrencyPickup();
            starFish.StopAnimation();
            graphics.SetActive(false);
        }
    }
}
