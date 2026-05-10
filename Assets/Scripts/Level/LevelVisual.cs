using TMPro;
using UnityEngine;

namespace BeachHero
{
    public class LevelVisual : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private SpriteRenderer levelIcon;
        [SerializeField] private TextMeshPro levelText;
        [SerializeField] private BoxCollider2D boxCollider2D;

        public bool IsCurrentLevel => levelData.IsCurrentLevel;

        public int LevelNumber => levelData.LevelNumber;

        public LevelData LevelData => levelData;

        public void SetPositions(Vector3 positions)
        {
            transform.position = positions;
        }
        public void Setup(int levelnumber, float scale)
        {
            levelData.LevelNumber = levelnumber;
            levelIcon.transform.localScale = Vector3.one * scale;
            boxCollider2D.size = Vector2.one * 2 * scale;
            levelText.text = levelData.LevelNumber.ToString();
            levelText.transform.rotation = Quaternion.identity;
            levelText.fontSizeMax = 10 * scale;
            // UpdateVisual();
            // levelText.text = levelData.LevelNumber.ToString();
            // levelText.rectTransform.rotation = Quaternion.Euler(0, 0, -(transform.parent.eulerAngles.z));
        }
        private void UpdateVisual()
        {
            //    complete.SetActive(false);
            //    current.SetActive(false);

            //    if (levelData.IsCompleted)
            //    {
            //        complete.SetActive(true);
            //    }
            //    else if (levelData.IsCurrentLevel)
            //    {
            //        current.SetActive(true);
            //    }
        }
    }
}
