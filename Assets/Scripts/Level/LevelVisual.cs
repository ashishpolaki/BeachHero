using TMPro;
using UnityEngine;

namespace BeachHero
{
    public class LevelVisual : MonoBehaviour
    {
        public enum LevelVisualState
        {
            Locked,
            Unlocked,
            Completed
        }
        [System.Serializable]
        public struct LevelIconState
        {
            public LevelVisualState state;
            public Sprite icon;
        }
        [SerializeField] private LevelData levelData;
        [SerializeField] private LevelIconState[] levelIconStates;
        [SerializeField] private SpriteRenderer levelIcon;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TextMeshPro levelText;
        [SerializeField] private BoxCollider boxCollider;

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
            boxCollider.size = new Vector3((Vector2.one * 2 * scale).x, (Vector2.one * 2 * scale).y, 0.1f);
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
