using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BeachHero
{
    [System.Serializable]
    public class ParticleData
    {
        public ParticleType type;
        public ParticleSystem prefab;
        public int initialPoolSize = 5;
        public float disableDelay = 1.5f;
    }

    public enum ParticleType
    {
        DashHit,
        BoomExplosion
    }

    public class ParticleController : SingleTon<ParticleController>
    {
        [SerializeField] private List<ParticleData> particles;
        [SerializeField] private Transform poolRoot;

        private Dictionary<ParticleType, List<ParticleSystem>> particlePools;

        #region Initialize/Create
        public void Initialize()
        {
            particlePools = new Dictionary<ParticleType, List<ParticleSystem>>();

            foreach (var data in particles)
            {
                List<ParticleSystem> pool = new List<ParticleSystem>();

                for (int i = 0; i < data.initialPoolSize; i++)
                {
                    ParticleSystem ps = CreateNew(data);
                    ps.gameObject.SetActive(false);
                    pool.Add(ps);
                }

                particlePools[data.type] = pool;
            }
        }
        private ParticleSystem CreateNew(ParticleData data)
        {
            ParticleSystem ps = Instantiate(data.prefab, poolRoot);
            ps.name = data.type.ToString();
            return ps;
        }
        #endregion

        #region Spawn
        public void Spawn(ParticleType type, Vector3 position, Quaternion rotation = default)
        {
            if (!particlePools.ContainsKey(type))
            {
                DebugUtils.LogWarning($"Particle {type} not found");
                return;
            }

            ParticleSystem ps = GetAvailable(type);
            if (ps == null) return;

            ps.transform.SetPositionAndRotation(position, rotation);
            ps.gameObject.SetActive(true);

            ps.Clear(true);
            ps.Play(true);

            float delay = GetDisableDelay(type, ps);
            ReturnToPoolAsync(ps, delay).Forget();
        }
        private async UniTaskVoid ReturnToPoolAsync(ParticleSystem ps, float delay)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay));

            if (ps == null) return;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
        }
        #endregion

        #region Pool GEt Helpers
        private ParticleSystem GetAvailable(ParticleType type)
        {
            var pool = particlePools[type];

            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeInHierarchy)
                    return pool[i];
            }

            ParticleData data = particles.Find(p => p.type == type);
            if (data == null)
            {
                DebugUtils.LogError($"No ParticleData found for {type}");
                return null;
            }

            ParticleSystem newPs = CreateNew(data);
            newPs.gameObject.SetActive(false);
            pool.Add(newPs);

            return newPs;
        }
        private float GetDisableDelay(ParticleType type, ParticleSystem ps)
        {
            ParticleData data = particles.Find(p => p.type == type);

            if (data != null && data.disableDelay > 0f)
                return data.disableDelay;

            var main = ps.main;
            return main.duration + main.startLifetime.constantMax;
        }
        #endregion
    }
}
