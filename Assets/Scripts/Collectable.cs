using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public interface ICollectable
    {
        public CollectableType CollectableType { get; set; }
        public abstract void Collect();
    }
    public class Collectable : MonoBehaviour, ICollectable
    {
        [SerializeField] private CollectableType collectableType;
        [SerializeField] private GameObject coinGraphics;
        [SerializeField] private ParticleSystem coinParticle;

        public CollectableType CollectableType
        {
            get
            {
                return collectableType;
            }
            set
            {
                collectableType = value;
            }
        }

        public void Init(CollectableData collectableData)
        {
            transform.position = collectableData.position;
            collectableType = collectableData.type;
        }

        public virtual void Collect()
        {
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
