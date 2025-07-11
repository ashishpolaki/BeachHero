using UnityEngine;

namespace BeachHero
{
    public class SpeedCollectable : Collectable
    {
        [SerializeField] private GameObject speedGraphics;
        [SerializeField] private PowerupType powerupType;

        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            speedGraphics.SetActive(true);
        }
        public override void Collect()
        {
            base.Collect();
            speedGraphics.SetActive(false);
            var particle = GameController.GetInstance.PoolManager.GameCurrencyParticlePool.GetObject().GetComponent<ParticleAutoDisable>();
            particle.PlayParticle(transform.position);
            GameController.GetInstance.PowerupController.OnPowerupCollected(powerupType, Count);
            gameObject.SetActive(false);
        }
    }
}
