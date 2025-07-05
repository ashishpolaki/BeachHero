using UnityEngine;

namespace BeachHero
{
    public class LevelVisual : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private GameObject complete;
        [SerializeField] private GameObject current;

        public void Setup(LevelData data)
        {
            $"Level {data.LevelNumber}".Log();
            levelData = data;
            // spriteRenderer = GetComponent<SpriteRenderer>();

            UpdateVisual();
        }

        void UpdateVisual()
        {
            complete.SetActive(false);
            current.SetActive(false);

            if (levelData.IsCompleted)
                complete.SetActive(true);
            else if (levelData.IsCurrentLevel)
            {
                current.SetActive(false);
            }
        }

        private void OnMouseDown()
        {
            if (levelData?.IsCurrentLevel == true || true)
            {
                MapController.GetInstance.ZoomIn(levelData.WorldPosition);

                Debug.Log($"Level {levelData.LevelNumber} clicked!");
                // Load your level scene or show popup
            }
        }
    }
}
