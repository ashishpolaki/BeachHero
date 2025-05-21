using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public class SpeedCollectable : Collectable
    {
        [SerializeField] private GameObject speedGraphics;
        [SerializeField] private float particleTime = 5f;
        private ParticleSystem speedParticle;

        private void OnDisable()
        {
            StopAllCoroutines();
        }
        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            speedGraphics.SetActive(true);
            if (speedParticle != null)
            {
                speedParticle.Stop();
                GameController.GetInstance.PoolManager.CoinParticlePool.ReturnObject(speedParticle.gameObject);
            }
        }
        public override void Collect()
        {
            base.Collect();
            speedGraphics.SetActive(false);
            speedParticle = GameController.GetInstance.PoolManager.CoinParticlePool.GetObject().GetComponent<ParticleSystem>();
            speedParticle.transform.position = transform.position;
            speedParticle.Play();
            StartCoroutine(IEReturnToPool());
        }
        private IEnumerator IEReturnToPool()
        {
            yield return new WaitForSeconds(particleTime);
            speedParticle.Stop();
            GameController.GetInstance.PoolManager.CoinParticlePool.ReturnObject(speedParticle.gameObject);
        }

    }
}
