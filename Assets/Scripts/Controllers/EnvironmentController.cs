using UnityEngine;

namespace BeachHero
{
    public class EnvironmentController : SingleTon<EnvironmentController>
    {
        [Header("Water Animation")]
        [SerializeField] private Sprite[] waterFrames;
        [SerializeField] private SpriteRenderer waterRenderer;
        private int currentWaterFrame = 0;
        private float waterAnimationTimer = 0f;

        public void UpdateWaterAnimation()
        {
            waterAnimationTimer += Time.deltaTime;
            if (waterAnimationTimer >= 1f / Application.targetFrameRate)
            {
                currentWaterFrame = (currentWaterFrame + 1) % waterFrames.Length;
                waterRenderer.sprite = waterFrames[currentWaterFrame];
                waterAnimationTimer = 0f;
            }
        }
    }
}
