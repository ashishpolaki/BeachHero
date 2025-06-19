using UnityEngine;

namespace BeachHero
{
    public class PoolController : MonoBehaviour
    {
        [SerializeField] private ScriptableObjectPool startPointPool;
        [SerializeField] private ScriptableObjectPool playerPool;
        [SerializeField] private ScriptableObjectPool savedCharacterPool;
        [SerializeField] private ScriptableObjectPool sharkPool;
        [SerializeField] private ScriptableObjectPool eelPool;
        [SerializeField] private ScriptableObjectPool mantaRayPool;
        [SerializeField] private ScriptableObjectPool waterHolePool;
        [SerializeField] private ScriptableObjectPool rockPool;
        [SerializeField] private ScriptableObjectPool barrelPool;
        [SerializeField] private ScriptableObjectPool coinsPool;
        [SerializeField] private ScriptableObjectPool coinParticlePool;
        [SerializeField] private ScriptableObjectPool magnetPowerupPool;
        [SerializeField] private ScriptableObjectPool speedPowerupPool;
        [SerializeField] private ScriptableObjectPool pathTrailPool;

        public ScriptableObjectPool StartPointPool => startPointPool;
        public ScriptableObjectPool PlayerPool => playerPool;
        public ScriptableObjectPool SavedCharacterPool => savedCharacterPool;
        public ScriptableObjectPool SharkPool => sharkPool;
        public ScriptableObjectPool EelPool => eelPool;
        public ScriptableObjectPool MantaRayPool => mantaRayPool;
        public ScriptableObjectPool WaterHolePool => waterHolePool;
        public ScriptableObjectPool RockPool => rockPool;
        public ScriptableObjectPool BarrelPool => barrelPool;
        public ScriptableObjectPool CoinsPool => coinsPool;
        public ScriptableObjectPool CoinParticlePool => coinParticlePool;
        public ScriptableObjectPool MagnetPowerupPool => magnetPowerupPool;
        public ScriptableObjectPool SpeedPowerupPool => speedPowerupPool;
        public ScriptableObjectPool PathTrailPool => pathTrailPool;

        public void Reset()
        {
            startPointPool.ResetState();
            playerPool.ResetState();
            savedCharacterPool.ResetState();
            sharkPool.ResetState();
            eelPool.ResetState();
            mantaRayPool.ResetState();
            waterHolePool.ResetState();
            rockPool.ResetState();
            barrelPool.ResetState();
            coinsPool.ResetState();
            coinParticlePool.ResetState();
            magnetPowerupPool.ResetState();
            speedPowerupPool.ResetState();
            pathTrailPool.ResetState();
        }
    }
}
