using UnityEngine;

namespace BeachHero
{
    public class EnvironmentController : SingleTon<EnvironmentController>
    {
        [Header("Water Animation")]
        [SerializeField] private Sprite[] waterFrames;
        [SerializeField] private SpriteRenderer waterRenderer;
        [SerializeField] private float waterAnimationSpeed = 5f;
        private int currentWaterFrame = 0;
        private float waterAnimationTimer = 0f;

        [Header("StarFishes Animation")]
        [SerializeField] private StarFish[] starFishes;

        public void Initialize()
        {
            foreach (var item in starFishes)
            {
                item.PlayRandomAnimation();
            }
        }
        public void DeInitialize()
        {
            foreach (var item in starFishes)
            {
                item.StopAnimation();
            }
        }

        public void UpdateWaterAnimation()
        {
            waterAnimationTimer += Time.deltaTime;
            if (waterAnimationTimer >= 1f / Application.targetFrameRate * waterAnimationSpeed)
            {
                currentWaterFrame = (currentWaterFrame + 1) % waterFrames.Length;
                waterRenderer.sprite = waterFrames[currentWaterFrame];
                waterAnimationTimer = 0f;
            }
        }
    }
}
