using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    [Serializable]
    public abstract class ImageClipBase : TweenClipBase
    {
        public Image target;
        public float fromFillAmount = 1f;
        public float toFillAmount = 0f;

        public ImageClipBase()
        {
            clipType = TweenClipType.Image;
        }

        public override void ApplyFromState()
        {
            if (target != null)
            {
                target.fillAmount = fromFillAmount;
            }
        }
    }

    [Serializable]
    public class ImageFillAmountClip : ImageClipBase
    {
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Image is null.");
                return null;
            }

            return target.DOFillAmount(toFillAmount, duration);
        }
    }

    public class ImageFadeClip : ImageClipBase
    {
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Image is null.");
                return null;
            }

            return target.DOFade(toFillAmount, duration);
        }
    }

    public class ImageGradientColorClip : ImageClipBase
    {
        public Color fromColor = Color.white;
        public Color toColor = Color.black;

        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Image is null.");
                return null;
            }
            return target.DOColor(toColor, duration);
        }

        public override void ApplyFromState()
        {
            if (target != null)
            {
                target.color = fromColor;
            }
        }
    }
}
