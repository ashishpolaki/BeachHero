using DG.Tweening;
using System;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public abstract class ShakeClipBase : TweenClipBase
    {
        public Transform target;
        public Vector3 strength = Vector3.one;
        public int vibrato = 10;
        public float randomness = 90f;
        public bool fadeOut = true;
        public Vector3 originalScale = Vector3.one;

        public ShakeClipBase()
        {
            clipType = TweenClipType.Shake;
        }

        public override void ApplyFromState()
        {
            if (target == null)
            {
                return;
            }
            target.localScale = originalScale;
        }
    }

    [Serializable]
    public class ShakePositionClip : ShakeClipBase
    {
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }
            return target.DOShakePosition(duration, strength, vibrato, randomness, snapping, fadeOut);
        }
    }

    [Serializable]
    public class ShakeRotationClip : ShakeClipBase
    {
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }
            return target.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut);
        }
    }

    [Serializable]
    public class ShakeScaleClip : ShakeClipBase
    {
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }
            return target.DOShakeScale(duration, strength, vibrato, randomness, fadeOut);
        }
    }
}
