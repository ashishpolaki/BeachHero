using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public enum FTUETutorialType
    {
        None,
        TapAndDrag,    // Tap + drag to save
        RescueAll,       // Save all drowning characters
    }
    public class TutorialController : SingleTon<TutorialController>
    {
        #region Inspector Variables
        [SerializeField] private FTUEConfigSO fTUEConfig;
        [SerializeField] private GameObject blockerOverlay;
        [SerializeField] private RectTransform highlightRect;
        [SerializeField] private Image highlightImage;

        [Space(2), Header("Button Highlight Animation")]
        [SerializeField] private float buttonScaleDuration = 0.5f;
        [SerializeField] private Ease buttonScaleEase = Ease.OutBack;

        [Space(2), Header("Hand Animation")]
        [SerializeField] private RectTransform handPointer;
        [SerializeField] private float handMoveOffset = 50f;
        [SerializeField] private float handMoveDuration = 0.5f;
        [SerializeField] private Ease handMoveEase = Ease.InOutSine;
        #endregion

        #region Events
        public event Action OnPlayerTapAction;
        public event Action OnPathDrawnAction;
        public event Action OnPowerupPressAction;
        #endregion

        #region Properties
        public FTUETutorialType CurrentFTUEType { private set; get; }
        #endregion

        #region Unity Methods
        private void OnDestroy()
        {
            ClearButtonHighlight();
            HideHandPointer();
        }
        #endregion

        #region Public Methods
        public void Init()
        {
            blockerOverlay.SetActive(false);
            handPointer.gameObject.SetActive(false);
            highlightRect.gameObject.SetActive(false);
        }

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
                    ShowHandPointer(button);
                });
        }

        public void ClearButtonHighlight()
        {
            highlightRect.DOKill();
            highlightRect.sizeDelta = Vector2.zero;
            highlightRect.gameObject.SetActive(false);
        }
        public void ShowHandPointer(Transform target)
        {
            handPointer.DOKill();
            handPointer.position = target.position;
            handPointer.gameObject.SetActive(true);

            Vector2 anchoredPos = handPointer.anchoredPosition;
            handPointer.DOAnchorPosY(anchoredPos.y + handMoveOffset, handMoveDuration)
                .SetEase(handMoveEase)
                .SetLoops(-1, LoopType.Yoyo);
        }
        public void HideHandPointer()
        {
            handPointer.DOKill();
            handPointer.gameObject.SetActive(false);
        }
        public void HideBlockerOverlay()
        {
            blockerOverlay.SetActive(false);
        }
        /// <summary>
        /// Is the current level a FTUE(First Time User Experience) level? 
        /// </summary>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        public bool IsFTUE(int levelNumber)
        {
            foreach (var item in fTUEConfig.entries)
            {
                if (item.levelNumber == levelNumber)
                {
                    CurrentFTUEType = item.tutorialType;
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
