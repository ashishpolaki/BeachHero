using LitMotion;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    [Serializable]
    public abstract class ImageClipBase : TweenClipBase
    {
        public Image target;
        public float fromValue = 1f;
        public float toValue = 0f;

        public ImageClipBase()
        {
            clipType = TweenClipType.Image;
        }

        public override void ApplyFromState()
        {
            if (target != null)
            {
                target.fillAmount = fromValue;
            }
        }
    }

    [Serializable]
    public class ImageFillAmountClip : ImageClipBase
    {
        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Image is null.");
            }
            return TweenManager.FillAmount(target, fromValue, toValue, duration, ease).Handle;
        }
    }
    [Serializable]
    public class ImageFadeClip : ImageClipBase
    {
        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Image is null.");
            }
            return TweenManager.Fade(target, fromValue, toValue, duration, ease).Handle;
        }
    }
    [Serializable]
    public class ImageGradientColorClip : ImageClipBase
    {
        public Color fromColor = Color.white;
        public Color toColor = Color.black;

        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Image is null.");
            }
            return TweenManager.Color(target, fromColor, toColor, duration, ease).Handle;
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
