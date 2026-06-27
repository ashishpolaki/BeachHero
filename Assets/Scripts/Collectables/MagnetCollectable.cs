using UnityEngine;

namespace BeachHero
{
    public class MagnetCollectable : Collectable
    {
        [SerializeField] private GameObject magnetGraphics;
        [SerializeField] private PowerupType powerupType;
        [SerializeField] private ParticleSystem pickUpParticle;

        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            magnetGraphics.SetActive(true);
            pickUpParticle.Stop();
            pickUpParticle.gameObject.SetActive(false);
        }
        public override void Collect()
        {
            base.Collect();
            magnetGraphics.SetActive(false);
            pickUpParticle.gameObject.SetActive(true);
            pickUpParticle.Play();
            GameController.GetInstance.PowerupController.OnPowerupCollected(powerupType,Count);
            AudioController.GetInstance.PlaySound(AudioType.BoosterCollect);
        }
    }
}
