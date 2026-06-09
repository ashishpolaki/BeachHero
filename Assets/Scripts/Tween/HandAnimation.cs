using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public abstract class HandAnimation
    {
        [Header("Transform Settings")]
        public Vector3 scale = Vector3.one;
        public float rotationZ = 0f;
        protected RectTransform handRect;
        protected Image handImage;

        public virtual void Setup(RectTransform _hand, Image image)
        {
            handRect = _hand;
            handImage = image;
            handRect.localScale = scale;
            handRect.localRotation = Quaternion.Euler(0, 0, rotationZ);
        }

        public abstract void Play();
        public abstract void Kill();
    }

    [System.Serializable]
    public class HandPointAnimation : HandAnimation
    {
        [Header("Pointing Settings")]
        public Vector2 startOffset = new Vector2(0f, 0);
        public Vector2 moveOffset = new Vector2(0f, 0);
        public float duration = 0.8f;
        public Ease ease = Ease.InOutSine;

        [Header("Fade Settings")]
        public float fadeDuration = 0.25f;

        private TweenHandle moveTweenHandle;
        private TweenHandle fadeTweenHandle;
        private Transform target;

        public void SetTarget(Transform _target)
        {
            target = _target;
        }

        public override void Play()
        {
            handRect.position = target.position;
            handRect.anchoredPosition = handRect.anchoredPosition + startOffset;

            fadeTweenHandle = TweenManager.Fade(handImage, 0, 1, fadeDuration, onComplete: () =>
            {
                moveTweenHandle = TweenManager.MoveAnchor(
                               handRect,
                               handRect.anchoredPosition,
                               handRect.anchoredPosition + moveOffset,
                               duration,
                               ease,
                               TransformAxis.XY,
                               -1,
                               LoopType.Yoyo);
            });
        }

        public override void Kill()
        {
            moveTweenHandle.Cancel();
            fadeTweenHandle.Cancel();
        }
    }

    [System.Serializable]
    public class HandTapAnimation : HandAnimation
    {
        [Header("Tap / Punch Settings")]
        public float punchStrength = 0.2f;
        public float punchDuration = 0.5f;
        public int punchFrequency = 1;

        private Transform target;
        private TweenHandle tween;

        public void SetTarget(Transform _target)
        { this.target = _target; }

        public override void Play()
        {
            handRect.position = target.position;

            tween = TweenManager.PunchScale(
                handRect.transform,
                scale,
                Vector3.one * punchStrength,
                punchFrequency,
                0,
                punchDuration);
        }

        public override void Kill()
        {
            tween.Cancel();
        }
    }
    [System.Serializable]
    public class HandDrawPathAnimation : HandAnimation
    {
        #region Inspector Variables
        [Header("Offset")]
        [SerializeField] private Vector2 startOffset = new Vector2(-111.66f, 71.2f);
        [SerializeField] private Vector2 touchOffset = Vector2.zero; //Finger Tip Alignment

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.2f;

        [Header("Scale Settings")]
        [SerializeField] private Vector3 pressScale = new Vector3(0.8f, 0.8f, 0.8f);
        [SerializeField] private float pressDuration = 0.4f;

        [Header("Movement Settings")]
        [SerializeField] private float moveToFirstDuration = 0.4f;
        [SerializeField] private float moveToSecondDuration = 0.4f;
        [SerializeField] private Ease moveEase = Ease.InOutQuint;

        [Header("Rotation Settings")]
        [SerializeField] private float zRotation = 220f;
        [SerializeField] private float rotationDuration = 0.4f;

        [Header("Loop Settings")]
        [SerializeField] private float loopDelay = 0.4f;
        #endregion

        private TweenSequence sequence;
        private Vector2 target1;
        private Vector2 target2;

        public void SetTargets(Vector2 _target1, Vector2 _target2)
        {
            target1 = _target1;
            target2 = _target2;
        }

        public override void Play()
        {
            handRect.anchoredPosition = target1 + startOffset + touchOffset;

            sequence = new TweenSequence(LSequence.Create());
            sequence.Insert(0, TweenManager.Scale(Vector3.zero, Vector3.one, handRect.transform, fadeDuration).Handle);
            sequence.Insert(0, TweenManager.Fade(handImage, 0, 1, fadeDuration).Handle);

            sequence.Insert(fadeDuration, TweenManager.Scale(Vector3.one, pressScale, handRect.transform, pressDuration).Handle);
            sequence.Insert(fadeDuration, TweenManager.MoveAnchor(handRect, handRect.anchoredPosition, target1 + touchOffset, moveToFirstDuration).Handle);
            sequence.Insert(fadeDuration, TweenManager.RotateEulerAngles(handRect.transform, handRect.transform.localEulerAngles,
                new Vector3(0, 0, zRotation), rotationDuration, Ease.InOutQuint, spaceType: TransformSpace.Local).Handle);

            sequence.Insert(fadeDuration + moveToFirstDuration, TweenManager.MoveAnchorOnAxis(handRect, target1.y + touchOffset.y, target2.y + touchOffset.y, moveToSecondDuration, Ease.OutQuad, TransformAxis.Y).Handle);
            var loopHandle = TweenManager.RunCallback(() =>
            {
                sequence.SetTime(0);
            }, loopDelay).Handle;

            sequence.OnComplete(loopHandle);
            sequence.InitializeHandle();
            sequence.Preserve();
            sequence.SetPlaybackSpeed(1);
        }

        public override void Kill()
        {
            sequence.Cancel();
        }
    }
}
