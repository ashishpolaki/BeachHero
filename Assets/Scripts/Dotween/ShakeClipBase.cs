using LitMotion;
using System;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public abstract class ShakeClipBase : TweenClipBase
    {
        public Transform target;
        public Vector3 originalScale = Vector3.one;
        public Vector3 startValue = Vector3.one;
        public Vector3 strength = Vector3.one;
        public int frequency = 10;
        public float dampingRatio = 0.5f;
        public uint randomSeed = 0;

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
        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
            }
            return TweenManager.ShakePosition(target, startValue, strength, frequency, duration, dampingRatio, randomSeed, ease, null).Handle;
        }
    }

    [Serializable]
    public class ShakeScaleClip : ShakeClipBase
    {
        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
            }
            return TweenManager.ShakeScale(target, startValue, strength, frequency, duration, dampingRatio, randomSeed, ease, null).Handle;
        }
    }
}
