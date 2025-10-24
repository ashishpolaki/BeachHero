using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public enum TutorialType
    {
        None,
        MagnetPowerup,
        SpeedBoostPowerup,
        TapAndDrag,      // Tap + drag to save. Used in Level 1
        RescueAll,       // Save all drowning characters in a level. Used in Level 2
    }
    public class TutorialController : SingleTon<TutorialController>
    {
        #region Inspector Variables
        [SerializeField] private TutorialConfigSO tutorialConfig;
        [SerializeField] private TutorialHand tutorialHand;
        [SerializeField] private TutorialCharacter tutorialCharacter;
        [SerializeField] private GameObject blockerOverlay;
        [SerializeField] private RectTransform highlightRect;
        [SerializeField] private Image highlightImage;

        [Space(2), Header("Button Highlight Animation")]
        [SerializeField] private float buttonScaleDuration = 0.5f;
        [SerializeField] private Ease buttonScaleEase = Ease.OutBack;
        #endregion

        #region Events
        public event Action OnPlayerTapAction;
        public event Action OnPathDrawnAction;
        public event Action OnPowerupPressAction;
        #endregion

        #region Properties
        public TutorialType TutorialType { private set; get; }
        public TutorialCharacter TutorialCharacter => tutorialCharacter;
        public TutorialHand TutorialHand => tutorialHand;
        #endregion

        #region Unity Methods
        private void OnDestroy()
        {
            ClearButtonHighlight();
            tutorialHand.Hide();
        }
        #endregion

        #region Highlight COntrols
        public void HighlightButton(Transform button, Vector2 size, Sprite sprite, bool sliced = false)
        {
            blockerOverlay.SetActive(true);
            highlightRect.DOKill();

            highlightRect.sizeDelta = Vector2.zero;
            highlightRect.position = button.position;
            highlightRect.gameObject.SetActive(true);

            highlightImage.sprite = sprite;
            highlightImage.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            if (sliced)
                highlightImage.pixelsPerUnitMultiplier = 100f;

            highlightRect.DOSizeDelta(size, buttonScaleDuration)
                .SetEase(buttonScaleEase)
                .OnComplete(() =>
                {
                    EnsureTutorialCanvas(button.gameObject, "SpritesAboveUI", 2);
                    tutorialHand.ShowHandPointing(button);
                });
        }

        public void ClearButtonHighlight()
        {
            highlightRect.DOKill();
            highlightRect.sizeDelta = Vector2.zero;
            highlightRect.gameObject.SetActive(false);
        }
        #endregion

        #region Public Methods
        public void Init()
        {
            blockerOverlay.SetActive(false);
            tutorialHand.Init();
            highlightRect.gameObject.SetActive(false);
        }
        public void SetCurrentTutorialType(TutorialType tutorialType)
        {
            TutorialType = tutorialType;
        }
        public void HideBlockerOverlay()
        {
            blockerOverlay.SetActive(false);
        }
        /// <summary>
        /// Is the current level a Tutorial level? 
        /// </summary>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public bool IsTutorial(int levelNumber)
        {
            foreach (var item in tutorialConfig.entries)
            {
                if (item.levelNumber == levelNumber)
                {
                    SetCurrentTutorialType(item.tutorialType);
                    return true;
                }
            }
            return false;
        }
        public void OnPlayerTap()
        {
            OnPlayerTapAction?.Invoke();
        }
        public void OnPathDrawn()
        {
            OnPathDrawnAction?.Invoke();
        }
        public void OnPowerupPressed()
        {
            OnPowerupPressAction?.Invoke();
        }
        #endregion

        #region Canvas Utility
        /// <summary>
        /// Ensures the target GameObject has a Canvas configured for tutorial overlay rendering
        /// and a GraphicRaycaster for input handling. Returns the Canvas component.
        /// </summary>
        /// <param name="target">GameObject to attach or validate UI components on.</param>
        /// <param name="sortingLayer">Optional sorting layer name. Defaults to "UI".</param>
        /// <param name="sortingOrder">Optional sorting order. Defaults to 10.</param>
        /// <returns>The Canvas attached to the GameObject.</returns>
        public Canvas EnsureTutorialCanvas(GameObject target, string sortingLayer = "UI", int sortingOrder = 10)
        {
            if (target == null)
            {
                DebugUtils.LogError("TutorialUiUtility: EnsureTutorialCanvas called with null target.");
            }

            // Get or add a Canvas
            var canvas = target.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = target.AddComponent<Canvas>();
            }

            // Configure sorting settings
            if (!string.IsNullOrEmpty(sortingLayer))
            {
                canvas.overrideSorting = true;
                canvas.sortingLayerName = sortingLayer;
                canvas.sortingOrder = sortingOrder;
            }

            // Ensure input handling
            if (!target.TryGetComponent(out GraphicRaycaster _))
            {
                target.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        /// <summary>
        /// Removes any Canvas and GraphicRaycaster components from the target GameObject.
        /// </summary>
        /// <param name="target">The GameObject to clean up UI components from.</param>
        public void RemoveTutorialCanvas(GameObject target)
        {
            if (target == null)
            {
                DebugUtils.LogError("TutorialUiUtility: RemoveTutorialCanvas called with null target.");
            }

            if (target.TryGetComponent(out GraphicRaycaster raycaster))
                UnityEngine.Object.Destroy(raycaster);

            if (target.TryGetComponent(out Canvas canvas))
                UnityEngine.Object.Destroy(canvas);
        }
        #endregion
    }
}
