using UnityEngine;

namespace BeachHero
{
    public class LevelVisual : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Setup(LevelData data)
        {
            $"Level {data.LevelNumber}".Log();
            levelData = data;
            spriteRenderer = GetComponent<SpriteRenderer>();

            UpdateVisual();
        }

        void UpdateVisual()
        {
            if (levelData.IsCompleted)
                spriteRenderer.color = Color.green;
            else if (levelData.IsUnlocked)
                spriteRenderer.color = Color.white;
            else
                spriteRenderer.color = Color.gray;
        }

        private void OnMouseDown()
        {
            if (levelData?.IsUnlocked == true || true)
            {
                MapController.Instance.ZoomIn(levelData.WorldPosition);

                Debug.Log($"Level {levelData.LevelNumber} clicked!");
                // Load your level scene or show popup
            }
        }
    }
}
