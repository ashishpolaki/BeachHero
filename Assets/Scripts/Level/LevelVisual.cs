using TMPro;
using UnityEngine;
using LitMotion;
using System;

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
        [SerializeField] private Sprite medalEarnedSprite;
        [SerializeField] private Sprite medalUnearnedSprite;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private TextMeshPro levelText;
        [SerializeField] private BoxCollider boxCollider;

        [Header("Animation")]
        [SerializeField] private float pressedScale = 0.9f;
        [SerializeField] private float hoverScale = 1.05f;
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

        public void SetRotation(Vector3 _rot)
        {
            levelIcon.transform.localRotation = Quaternion.Euler(_rot);
        }
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
        public void Setup(LevelData _levelData)
        {
            levelData = _levelData;
            _originalScale = levelIcon.transform.localScale;
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
            //If level visual is lower than current level number 
            if (levelData.MedalsEarned > 0)
                for (int i = 0; i < medalsList.Length; i++)
                {
                    medalsList[i].gameObject.SetActive(true);
                    medalsList[i].sprite = i < levelData.MedalsEarned ? medalEarnedSprite : medalUnearnedSprite;
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

        #region Animation
        public void PressAnimation(Action action = null)
        {
            AnimateScale(_originalScale * pressedScale, pressEase, action);
        }
        public void ReleaseAnimation(Action action = null)
        {
            AnimateScale(_originalScale, releaseEase, action);
        }
        private void AnimateScale(Vector3 target, Ease ease, System.Action onComplete = null)
        {
            scaleTween.Cancel();
            scaleTween = TweenManager.Scale(levelIcon.transform.localScale, target, levelIcon.transform, tweenDuration, ease, onComplete);
        }

        #endregion
    }
}
