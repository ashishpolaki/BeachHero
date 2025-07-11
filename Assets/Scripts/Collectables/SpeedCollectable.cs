using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public class SpeedCollectable : Collectable
    {
        [SerializeField] private GameObject speedGraphics;
        [SerializeField] private float particleTime = 5f;
        [SerializeField] private PowerupType powerupType;
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
                GameController.GetInstance.PoolManager.GameCurrencyParticlePool.ReturnObject(speedParticle.gameObject);
            }
        }
        public override void Collect()
        {
            base.Collect();
            speedGraphics.SetActive(false);
            speedParticle = GameController.GetInstance.PoolManager.GameCurrencyParticlePool.GetObject().GetComponent<ParticleSystem>();
            speedParticle.transform.position = transform.position;
            speedParticle.Play();
            GameController.GetInstance.PowerupController.OnPowerupCollected(powerupType, Count);
            StartCoroutine(IEReturnToPool());
        }
        private IEnumerator IEReturnToPool()
        {
            yield return new WaitForSeconds(particleTime);
            speedParticle.Stop();
            GameController.GetInstance.PoolManager.GameCurrencyParticlePool.ReturnObject(speedParticle.gameObject);
        }

    }
}
