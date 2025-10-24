using DG.Tweening;
using UnityEngine;

namespace BeachHero
{
    public class TutorialHand : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform handRect;

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
        [SerializeField] private float punchScaleAmount = 0.2f;
        [SerializeField] private float punchDuration = 0.5f;
        [SerializeField] private float punchElasticity = 0.2f;

        private Tween activeTween;

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
            activeTween = handRect.DOAnchorPosY(anchoredPos.y + pointingMoveYOffset, pointingDuration)
                .SetEase(pointingEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void Hide()
        {
            KillActiveTween();
            if (handRect) handRect.gameObject.SetActive(false);
        }

        public void PlayPunchThenMoveLoop(Vector3 punchPosition, Vector3 movePosition)
        {
            if (!handRect) return;

            KillActiveTween();

            handRect.localScale = initialPunchScale * Vector3.one;
            handRect.localPosition = punchPosition;
            handRect.gameObject.SetActive(true);

            Sequence seq = DOTween.Sequence();

            //Add handrect punch y offset
            handRect.anchoredPosition = new Vector2(handRect.anchoredPosition.x, handRect.anchoredPosition.y + punchYOffset);
            seq.Append(handRect.DOPunchScale(Vector3.one * punchScaleAmount, punchDuration, 1, punchElasticity))
               .Append(handRect.DOAnchorPos(
                   new Vector2(movePosition.x, movePosition.y + moveYOffset),
                   moveDuration
               ).SetEase(moveEase)).SetLoops(-1,LoopType.Restart);

            activeTween = seq;
        }

        //Kill  active tween 
        private void KillActiveTween()
        {
            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill();
                activeTween = null;
            }

            handRect?.DOKill(); // Kill any tweens attached to handRect itself
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
