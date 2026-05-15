using TMPro;
using UnityEngine;

namespace BeachHero
{
    public enum LevelVisualState
    {
        Locked,
        Current,
        Completed
    }
    public class LevelVisual : MonoBehaviour
    {
        [System.Serializable]
        public struct LevelIconState
        {
            public LevelVisualState state;
            public Sprite icon;
        }
        [SerializeField] private LevelData levelData;
        [SerializeField] private LevelIconState[] levelIconStates;
        [SerializeField] private SpriteRenderer levelIcon;
        [SerializeField] private GameObject[] medals;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TextMeshPro levelText;
        [SerializeField] private BoxCollider boxCollider;

        public bool IsCurrentLevel => levelData.State == LevelVisualState.Current;

        public int LevelNumber => levelData.LevelNumber;

        public LevelData LevelData => levelData;

        public LevelVisualState State => levelData.State;

        public void Setup(int levelnumber, float scale)
        {
            levelData.LevelNumber = levelnumber;
            levelIcon.transform.localScale = Vector3.one * scale;
            boxCollider.size = new Vector3((Vector2.one * 2 * scale).x, (Vector2.one * 2 * scale).y, 0.1f);
            levelText.text = levelData.LevelNumber.ToString();
            levelText.transform.rotation = Quaternion.identity;
            lockIcon.transform.rotation = Quaternion.identity;
            levelText.fontSizeMax = 10 * scale;
            foreach (GameObject medal in medals)
            {
                medal.transform.rotation = Quaternion.identity;
                medal.gameObject.SetActive(false);
            }
            // levelText.text = levelData.LevelNumber.ToString();
            // levelText.rectTransform.rotation = Quaternion.Euler(0, 0, -(transform.parent.eulerAngles.z));
        }
        public void Setup(LevelData _levelData)
        {
            levelData = _levelData;
            UpdateVisual();
        }
        private void SetLevelIcon()
        {
            for (int i = 0; i < levelIconStates.Length; i++)
            {
                if (levelIconStates[i].state == levelData.State)
                {
                    levelIcon.sprite = levelIconStates[i].icon;
                    lockIcon.SetActive(levelData.State == LevelVisualState.Locked);
                    break;
                }
            }
        }
        private void SetMedals()
        {
            for (int i = 0; i < medals.Length; i++)
            {
                medals[i].SetActive(i < levelData.MedalsEarned);
            }
        }
        private void UpdateVisual()
        {
            SetLevelIcon();
            lockIcon.SetActive(levelData.State == LevelVisualState.Locked);
            levelText.gameObject.SetActive(levelData.State != LevelVisualState.Locked);
            boxCollider.enabled = levelData.State != LevelVisualState.Locked;
            SetMedals();
        }
        public void OnLevelComplete(int medals)
        {
            levelData.State = LevelVisualState.Completed;
            levelData.MedalsEarned = medals;
            UpdateVisual();
        }
        public void SetAsCurrentLevel()
        {
            levelData.State = LevelVisualState.Current;
            UpdateVisual();
        }
    }
}
