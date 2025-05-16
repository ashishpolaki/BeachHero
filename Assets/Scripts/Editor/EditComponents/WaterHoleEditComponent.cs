#if UNITY_EDITOR
using UnityEngine;

namespace BeachHero
{
    public class WaterHoleEditComponent : MonoBehaviour
    {
        [Range(0, 10f)] public float cycloneRadius;
        private WaterHoleObstacle WaterHoleObstacle;
        private bool onValidate = false;

        public void Init(WaterHoleObstacleData waterHoleObstacleData)
        {
            cycloneRadius = waterHoleObstacleData.scale;
            WaterHoleObstacle = GetComponent<WaterHoleObstacle>();
            WaterHoleObstacle.Init(waterHoleObstacleData);
            onValidate = true;
        }

        private void OnValidate()
        {
            if (!onValidate) return;
            WaterHoleObstacle.Init(new WaterHoleObstacleData
            {
                position = transform.position,
                scale = cycloneRadius
            });
        }
    }
}
#endif
