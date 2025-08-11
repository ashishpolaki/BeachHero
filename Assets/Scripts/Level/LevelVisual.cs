using TMPro;
using UnityEngine;

namespace BeachHero
{
    public class LevelVisual : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private GameObject complete;
        [SerializeField] private GameObject current;
        [SerializeField] private TextMeshPro levelText;

        public Vector3 WorldPosition => levelData.WorldPosition;

        public bool IsCurrentLevel => levelData.IsCurrentLevel;

        public int LevelNumber => levelData.LevelNumber;

        public LevelData LevelData => levelData;

        public void SetPositions(Vector3 positions)
        {
            transform.position = positions;
        }

        public void Setup(LevelData data)
        {
            levelData = data;
            UpdateVisual();
            levelText.text = levelData.LevelNumber.ToString();
            levelText.rectTransform.rotation = Quaternion.Euler(0, 0, -(transform.parent.eulerAngles.z));
            levelData.WorldPosition = transform.position;
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
