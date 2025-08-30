using UnityEngine;

namespace BeachHero
{
    public class GameCurrencyCollectable : Collectable
    {
        [SerializeField] private GameObject graphics;
        [SerializeField] private float rotateSpeed = 200f;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private StarFish starFish;

        private Transform moveTarget;
        private bool canMoveToTarget;

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

                // Optional: Add rotation to the coin for a dynamic effect
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            }
        }
        public override void ResetState()
        {
            base.ResetState();
            canMoveToTarget = false;
            starFish.StopAnimation();
        }
        public override void Collect()
        {
            base.Collect();
            graphics.SetActive(false);
            var particle = GameController.GetInstance.PoolManager.GameCurrencyParticlePool.GetObject().GetComponent<ParticleAutoDisable>();
            particle.PlayParticle(transform.position);
            GameController.GetInstance.OnGameCurrencyPickup();
            starFish.StopAnimation();
            gameObject.SetActive(false);
        }
    }
}
