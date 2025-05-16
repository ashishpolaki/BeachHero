using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public class Coin : Collectable
    {
        [SerializeField] private GameObject coinGraphics;
        [SerializeField] private ParticleSystem coinParticle;
        [SerializeField] private float rotateSpeed = 200f;
        [SerializeField] private float moveSpeed = 10f;

        private Transform moveTarget;
        private bool canMoveToTarget;

        public bool CanMoveToTarget => canMoveToTarget;

        public void SetTarget(Transform target)
        {
            moveTarget = target;
            canMoveToTarget = true;
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
            coinGraphics.SetActive(false);
            coinParticle = GameController.GetInstance.PoolManager.CoinParticlePool.GetObject().GetComponent<ParticleSystem>();
            coinParticle.transform.position = transform.position;
            coinParticle.Play();
            StartCoroutine(IEReturnToPool());
        }

        private IEnumerator IEReturnToPool()
        {
            yield return new WaitForSeconds(5f);
            coinParticle.Stop();
            GameController.GetInstance.PoolManager.CoinParticlePool.ReturnObject(coinParticle.gameObject);
        }
    }
}
