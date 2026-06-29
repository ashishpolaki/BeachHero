using TMPro;
using UnityEngine;
using LitMotion;
using System;
using UnityEngine.Serialization;

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

        #region Serialized Fields
        [Header("References")]
        [SerializeField] private LevelData levelData;
        [SerializeField] private LevelIconState[] levelIconStates;
        [SerializeField] private SpriteRenderer levelIcon;
        [SerializeField] private SpriteRenderer[] medalsList;
        [FormerlySerializedAs("medalEarnedSprite")]
        [SerializeField] private Sprite enableStarSprite;
        [FormerlySerializedAs("medalUnearnedSprite")]
        [SerializeField] private Sprite disableStarSprite;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TextMeshPro levelText;
        [SerializeField] private BoxCollider boxCollider;

        [Header("Animation")]
        [SerializeField] private float pressedScale = 0.9f;
      //  [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float tweenDuration = 0.15f;
        [SerializeField] private Ease pressEase = Ease.OutBack;
        [SerializeField] private Ease releaseEase = Ease.OutBack;
        private TweenHandle scaleTween;
        private Vector3 _originalScale;
        #endregion

        #region Properties
        public bool IsCurrentLevel => levelData.State == LevelVisualState.Current;

        public int LevelNumber => levelData.LevelNumber;

        public LevelData LevelData => levelData;

        public LevelVisualState State => levelData.State;
        #endregion

#if UNITY_EDITOR
        public void Setup(int levelnumber, float scale)
        {
            levelData.LevelNumber = levelnumber;
            levelIcon.transform.localScale = Vector3.one * scale;
            boxCollider.size = new Vector3((Vector2.one * 2 * scale).x, (Vector2.one * 2 * scale).y, 0.1f);
            levelText.text = levelData.LevelNumber.ToString();
            levelText.transform.rotation = Quaternion.identity;
            levelText.fontSizeMax = 10 * scale;
            foreach (var medal in medalsList)
            {
                medal.gameObject.SetActive(false);
            }
        }
#endif

        public void SetIconRotation(Vector3 _rot)
        {
            levelIcon.transform.localRotation = Quaternion.Euler(_rot);
        }
       
        public void Setup(LevelData _levelData)
        {
            levelData = _levelData;
            _originalScale = levelIcon.transform.localScale;
            RefreshVisual();
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
        private void UpdateStars()
        {
            //If level visual is lower than current level number 
            if (levelData.StarsEarned > 0)
                for (int i = 0; i < medalsList.Length; i++)
                {
                    medalsList[i].gameObject.SetActive(true);
                    medalsList[i].sprite = i < levelData.StarsEarned ? enableStarSprite : disableStarSprite;
                }
        }
        private void RefreshVisual()
        {
            SetLevelIcon();
            lockIcon.SetActive(levelData.State == LevelVisualState.Locked);
            levelText.gameObject.SetActive(levelData.State != LevelVisualState.Locked);
            boxCollider.enabled = levelData.State != LevelVisualState.Locked;
            UpdateStars();
        }
        public void OnLevelComplete(int stars)
        {
            levelData.State = LevelVisualState.Completed;
            levelData.StarsEarned = stars;
            RefreshVisual();
        }
        public void SetAsCurrentLevel()
        {
            levelData.State = LevelVisualState.Current;
            RefreshVisual();
        }

        #region Animation
        public void PressAnimation(Action action = null)
        {
            AudioController.GetInstance.PlaySound(AudioType.Toggle);
            TweenScale(_originalScale * pressedScale, pressEase, action);
        }
        public void ReleaseAnimation(Action action = null)
        {
            TweenScale(_originalScale, releaseEase, action);
        }
        private void TweenScale(Vector3 target, Ease ease, Action onComplete = null)
        {
            scaleTween.Cancel();
            scaleTween = TweenManager.Scale(levelIcon.transform.localScale, target, levelIcon.transform, tweenDuration, ease, onComplete);
        }
        #endregion
    }
}
