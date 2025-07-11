using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public class MagnetCollectable : Collectable
    {
        [SerializeField] private GameObject magnetGraphics;
        [SerializeField] private float particleTime = 5f;
        [SerializeField] private PowerupType powerupType;
        private ParticleSystem magnetParticle;

        private void OnDisable()
        {
            StopAllCoroutines();
        }
        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            magnetGraphics.SetActive(true);
            if (magnetParticle != null)
            {
                magnetParticle.Stop();
                GameController.GetInstance.PoolManager.GameCurrencyParticlePool.ReturnObject(magnetParticle.gameObject);
            }
        }
        public override void Collect()
        {
            base.Collect();
            magnetGraphics.SetActive(false);
            magnetParticle = GameController.GetInstance.PoolManager.GameCurrencyParticlePool.GetObject().GetComponent<ParticleSystem>();
            magnetParticle.transform.position = transform.position;
            magnetParticle.Play();
            GameController.GetInstance.PowerupController.OnPowerupCollected(powerupType,Count);
            StartCoroutine(IEReturnToPool());
        }
        private IEnumerator IEReturnToPool()
        {
            yield return new WaitForSeconds(particleTime);
            magnetParticle.Stop();
            GameController.GetInstance.PoolManager.GameCurrencyParticlePool.ReturnObject(magnetParticle.gameObject);
        }
    }
}
