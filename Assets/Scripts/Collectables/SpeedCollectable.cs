using UnityEngine;

namespace BeachHero
{
    public class SpeedCollectable : Collectable
    {
        [SerializeField] private GameObject speedGraphics;
        [SerializeField] private PowerupType powerupType;
        [SerializeField] private ParticleSystem pickUpParticle;

        public override void Init(CollectableData collectableData)
        {
            base.Init(collectableData);
            speedGraphics.SetActive(true);
            pickUpParticle.Stop();
            pickUpParticle.gameObject.SetActive(false);
        }
        public override void Collect()
        {
            base.Collect();
            speedGraphics.SetActive(false);
            pickUpParticle.gameObject.SetActive(true);
            pickUpParticle.Play();
            GameController.GetInstance.PowerupController.OnPowerupCollected(powerupType, Count);
            AudioController.GetInstance.PlaySound(AudioType.BoosterCollect);
        }
    }
}
