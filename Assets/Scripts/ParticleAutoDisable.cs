using UnityEngine;

namespace BeachHero
{
    public class ParticleAutoDisable : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private float disableDelay = 2f; // Delay before disabling the object

        public void PlayParticle(Vector3 position)
        {
            transform.position = position;
            particle.Play();
            Invoke(nameof(DisableObject), disableDelay);
        }

        public void SetRotation(Vector3 euler)
        {
            var par = particle.main;
            par.startRotation = euler.y * Mathf.Deg2Rad;
        }

        public void DisableObject()
        {
            if (particle != null)
            {
                particle.Stop();
                GameController.GetInstance.PoolManager.GameCurrencyParticlePool.ReturnObject(particle.gameObject);
            }
        }

    }
}
