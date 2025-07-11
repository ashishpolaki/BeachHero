using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public class GameCurrencyCollectable : Collectable
    {
        [SerializeField] private GameObject graphics;
        [SerializeField] private float particleTime = 5f;
        [SerializeField] private float rotateSpeed = 200f;
        [SerializeField] private float moveSpeed = 10f;

        private ParticleSystem particle;
        private Transform moveTarget;
        private bool canMoveToTarget;

        public bool CanMoveToTarget => canMoveToTarget;

        private void OnDisable()
        {
            StopAllCoroutines();
        }
        public void SetTarget(Transform target)
        {
            moveTarget = target;
            canMoveToTarget = true;
        }
        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            graphics.SetActive(true);
            if (particle != null)
            {
                particle.Stop();
                GameController.GetInstance.PoolManager.GameCurrencyParticlePool.ReturnObject(particle.gameObject);
            }
            canMoveToTarget = false;
        }
        public override void UpdateState()
        {
            base.UpdateState();
            if (canMoveToTarget)
            {
                // Smoothly move the coin toward the player
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    moveTarget.position,
                    moveSpeed * Time.deltaTime
                );

                // Optional: Add rotation to the coin for a dynamic effect
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            }
        }

        public override void Collect()
        {
            base.Collect();
            graphics.SetActive(false);
            particle = GameController.GetInstance.PoolManager.GameCurrencyParticlePool.GetObject().GetComponent<ParticleSystem>();
            particle.transform.position = transform.position;
            particle.Play();
            GameController.GetInstance.OnGameCurrencyPickup();
            StartCoroutine(IEReturnToPool());
        }

        private IEnumerator IEReturnToPool()
        {
            yield return new WaitForSeconds(particleTime);
            particle.Stop();
            GameController.GetInstance.PoolManager.GameCurrencyParticlePool.ReturnObject(particle.gameObject);
        }
    }
}
