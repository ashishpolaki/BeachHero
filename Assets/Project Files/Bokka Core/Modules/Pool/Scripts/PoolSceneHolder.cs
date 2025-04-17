using UnityEngine;

namespace Bokka
{
    public class PoolSceneHolder : MonoBehaviour
    {
        [SerializeField] Pool[] pools;

        private void Awake()
        {
            foreach(Pool pool in pools)
            {
                pool.Init();
            }
        }

        private void OnDestroy()
        {
            foreach (Pool pool in pools)
            {
                PoolManager.DestroyPool(pool);
            }
        }
    }
}