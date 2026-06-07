using UnityEngine;
using LitMotion;
using UnityEngine.UI;

namespace BeachHero
{
    public class TutorialHand : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform handRect;
        [SerializeField] private Canvas handCanvas;
        [SerializeField] private Image handImage;

        private HandAnimation currentHandAnimation;

        #region Initialization and Cleanup Methods
        public void Init()
        {
            if (!handRect) return;

            KillActiveTween();
            handRect.localScale = Vector3.one;
            handRect.gameObject.SetActive(false);
        }
        public void PlayAnimation(HandAnimation handAnimation, Transform target)
        {
            currentHandAnimation?.Kill();
            currentHandAnimation = handAnimation;
            handRect.gameObject.SetActive(true);
            currentHandAnimation.Play(handRect, handImage, target);
        }
        public void Hide()
        {
            KillActiveTween();
            if (handRect) handRect.gameObject.SetActive(false);
            moveTweenHandle.Cancel();
            moveSequence.Cancel();
        }
        private void KillActiveTween()
        {
            if (currentHandAnimation != null)
            {
                currentHandAnimation?.Kill();
                currentHandAnimation = null;
            }
            //Reset hand sorting layer
            SetHandSortingLayer(StringUtils.SPRITES_ABOVE_UI_LAYER, 3);
        }
        public void SetHandSortingLayer(string sortingLayer, int sortingOrder)
        {
            if (handCanvas != null)
            {
                handCanvas.sortingLayerName = sortingLayer;
                handCanvas.sortingOrder = sortingOrder;
            }
        }
        #endregion

        #region Delete Later
        [Header("Animations")]
        [SerializeField] private float pointingStartYOffset = 30f;
        [SerializeField] private float pointingMoveYOffset = 30f;
        [SerializeField] private float pointingDuration = 0.8f;
        [SerializeField] private float pointingScale = 1f;
        [SerializeField] private Ease pointingEase = Ease.InOutSine;

        [Header("Movement Settings")]
        [SerializeField] private float moveYOffset = 30f;
        [SerializeField] private float moveDuration = 0.8f;
        [SerializeField] private Ease moveEase = Ease.InOutSine;

        [Header("Punch Animation Settings")]
        [SerializeField] private float punchYOffset = 100f;
        [SerializeField] private float initialPunchScale = 0.8f;
        [SerializeField] private float punchStrength = 0.2f;
        [SerializeField] private float punchDuration = 0.5f;
        [SerializeField] private int punchFrequency = 1;

        private TweenSequence moveSequence;
        private TweenHandle moveTweenHandle;

        public void PlayPunchThenMoveLoop(Vector3 punchPosition, Vector3 movePosition)
        {
            if (!handRect) return;

            KillActiveTween();

            handRect.localScale = initialPunchScale * Vector3.one;
            handRect.localPosition = punchPosition;
            handRect.gameObject.SetActive(true);

            //Add handrect punch y offset
            handRect.anchoredPosition = new Vector2(handRect.anchoredPosition.x, handRect.anchoredPosition.y + punchYOffset);
            moveSequence = new TweenSequence(LSequence.Create());
            var punch = TweenManager.PunchScale(handRect.transform, Vector3.one * initialPunchScale, Vector3.one * punchStrength
                 , punchFrequency, 0, punchDuration).Handle;
            var move = TweenManager.MoveAnchor(handRect, new Vector2(handRect.anchoredPosition.x, handRect.anchoredPosition.y + punchYOffset),
                new Vector2(movePosition.x, movePosition.y + moveYOffset), moveDuration, moveEase, TransformAxis.XY).Handle;

            moveSequence.Append(punch);
            moveSequence.Append(move);
            moveSequence.AppendInterval(0.2f); // Small delay before looping
            var loopHandle = TweenManager.RunCallback(() => { moveSequence.SetTime(0); }).Handle;
            moveSequence.OnComplete(loopHandle);
            moveSequence.InitializeHandle();
            moveSequence.Preserve();
        }
        #endregion

    }
}
