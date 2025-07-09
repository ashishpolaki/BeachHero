using UnityEngine;

namespace BeachHero
{
    public class LevelVisual : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private GameObject complete;
        [SerializeField] private GameObject current;

        public Vector3 WorldPosition => levelData.WorldPosition;

        public bool IsCurrentLevel => levelData.IsCurrentLevel;

        public int LevelNumber => levelData.LevelNumber;
        
        public LevelData LevelData => levelData;


        public void SetPositions(Vector3 positions)
        {
            transform.position = positions;
            levelData.WorldPosition = positions;
        }
        
        public void Setup(LevelData data)
        {
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
                current.SetActive(true);
            }
        }
    }
}
