using UnityEngine;

namespace BeachHero
{
    public class MagnetCollectable : Collectable
    {
        [SerializeField] private GameObject magnetGraphics;
        [SerializeField] private PowerupType powerupType;

        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            magnetGraphics.SetActive(true);
        }
        public override void Collect()
        {
            base.Collect();
            magnetGraphics.SetActive(false);
            var particle = GameController.GetInstance.PoolManager.GameCurrencyParticlePool.GetObject().GetComponent<ParticleAutoDisable>();
            particle.PlayParticle(transform.position);
            GameController.GetInstance.PowerupController.OnPowerupCollected(powerupType,Count);
            gameObject.SetActive(false);
        }
    }
}
