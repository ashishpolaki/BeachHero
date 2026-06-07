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

        public virtual void Setup(RectTransform hand)
        {
            hand.localScale = scale;
            hand.localRotation = Quaternion.Euler(0, 0, rotationZ);
        }

        public abstract void Play(RectTransform hand, Image handCanvas, Transform target);
        public abstract void Kill();
    }

    [System.Serializable]
    public class HandPointAnimation : HandAnimation
    {
        [Header("Pointing Settings")]
        public TransformAxis moveAxis = TransformAxis.Y;
        public float startOffset = 0f;
        public float moveOffset = 0f;
        public float duration = 0.8f;
        public Ease ease = Ease.InOutSine;

        [Header("Fade Settings")]
        public float fadeDuration = 0.25f;

        private TweenHandle moveTweenHandle;
        private TweenHandle fadeTweenHandle;

        public override void Play(RectTransform hand, Image handImage, Transform target)
        {
            Setup(hand);

            hand.position = target.position;

            hand.anchoredPosition = moveAxis == TransformAxis.X
                ? new Vector2(hand.anchoredPosition.x + startOffset, hand.anchoredPosition.y)
                : new Vector2(hand.anchoredPosition.x, hand.anchoredPosition.y + startOffset);

            float from = moveAxis == TransformAxis.X ? hand.anchoredPosition.x : hand.anchoredPosition.y;
            fadeTweenHandle = TweenManager.Fade(handImage, 0, 1, fadeDuration,onComplete : ()=>
            {
                moveTweenHandle = TweenManager.MoveAnchorOnAxis(
                               hand,
                               from,
                               from + moveOffset,
                               duration,
                               ease,
                               moveAxis,
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

        public override void Play(RectTransform hand, Image handImage, Transform target)
        {
            Setup(hand);

            hand.position = target.position;

            TweenManager.PunchScale(
                hand.transform,
                scale,
                Vector3.one * punchStrength,
                punchFrequency,
                0,
                punchDuration);
        }

        public override void Kill()
        {
            // Punch tweens are one-shot, so no need to kill them
        }
    }

    [System.Serializable]
    public class HandDragAnimation : HandAnimation
    {
        [Header("Drag Settings")]
        public float duration = 1f;
        public Ease ease = Ease.InOutSine;

        public override void Play(RectTransform hand, Image handImage, Transform target)
        {
            Setup(hand);

            // You will expand this later with path
            hand.position = target.position;
        }
        public override void Kill()
        {
            // Punch tweens are one-shot, so no need to kill them
        }
    }

}
