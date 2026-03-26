using UnityEngine;
using LitMotion;

namespace BeachHero
{
    public class TutorialHand : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform handRect;
        [SerializeField] private Canvas handCanvas;

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

        public void Init()
        {
            if (!handRect) return;

            KillActiveTween();

            handRect.localScale = Vector3.one;
            handRect.gameObject.SetActive(false);
        }

        public void ShowHandPointing(Transform target)
        {
            if (!handRect || !target) return;

            KillActiveTween();

            handRect.localScale = Vector3.one * pointingScale;
            handRect.position = target.position;
            handRect.anchoredPosition = new Vector2(handRect.anchoredPosition.x, handRect.anchoredPosition.y + pointingStartYOffset);
            handRect.gameObject.SetActive(true);

            Vector2 anchoredPos = handRect.anchoredPosition;
            moveTweenHandle = TweenManager.MoveAnchorOnAxis(handRect, handRect.anchoredPosition.y,
                handRect.anchoredPosition.y + pointingMoveYOffset, pointingDuration,
                pointingEase, TransformAxis.Y, -1, LoopType.Yoyo);
        }

        public void Hide()
        {
            KillActiveTween();
            if (handRect) handRect.gameObject.SetActive(false);
        }

        public void SetHandSortingLayer(string sortingLayer, int sortingOrder)
        {
            if (handCanvas != null)
            {
                handCanvas.sortingLayerName = sortingLayer;
                handCanvas.sortingOrder = sortingOrder;
            }
        }

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

        //Kill  active tween 
        private void KillActiveTween()
        {
            moveSequence.Cancel();
            moveTweenHandle.Cancel();
        }

        private void SetColor()
        {
            //handImageColor = handImage.color;
            //handImageColor.a = 1f; 
            //handImage.color = handImageColor;
        }

        private void FadeHand()
        {
            //handImageColor = handImage.color;
            //handImageColor.a = 0f;
            //handImage.DOKill();
            //  handImage.DOKill();
            //handImage.DOFade(0, panelFadeDuration).OnComplete(() =>
            //{
            //    handImage.color = handImageColor;
            //    Close();
            //});
        }

    }
}
