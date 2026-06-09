using UnityEngine;

namespace BeachHero
{
    public class ShieldCollectable : Collectable
    {
        [SerializeField] private GameObject graphics;
        [SerializeField] private PowerupType powerupType;
        [SerializeField] private ParticleSystem pickUpParticle;

        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            graphics.SetActive(true);
            pickUpParticle.Stop();
            pickUpParticle.gameObject.SetActive(false);
        }
        public override void Collect()
        {
            base.Collect();
            graphics.SetActive(false);
            pickUpParticle.gameObject.SetActive(true);
            pickUpParticle.Play();
            GameController.GetInstance.PowerupController.OnPowerupCollected(powerupType, Count);
            AudioController.GetInstance.PlaySound(AudioType.Collect3);
        }
    }
}
