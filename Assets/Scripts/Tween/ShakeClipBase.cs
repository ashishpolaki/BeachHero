using LitMotion;
using System;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public abstract class ShakeClipBase : TweenClipBase
    {
        public Transform target;
        public Vector3 originalValue = Vector3.one;
        public Vector3 startValue = Vector3.one;
        public Vector3 strength = Vector3.one;
        public int frequency = 10;
        public float dampingRatio = 0.5f;
        public uint randomSeed = 0;

        public ShakeClipBase()
        {
            clipType = TweenClipType.Shake;
        }

        public override bool IsTargetNull()
        {
            return target == null;
        }
    }

    [Serializable]
    public class ShakePositionClip : ShakeClipBase
    {
        public override void ApplyFromState()
        {
            if (target == null)
            {
                return;
            }
            target.localPosition = originalValue;
        }
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
        public override void ApplyFromState()
        {
            if (target == null)
            {
                return;
            }
            target.localScale = originalValue;
        }
        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
            }
            return TweenManager.ShakeScale(target, startValue, strength, frequency, duration, dampingRatio, randomSeed, ease, null).Handle;
        }
    }

    [Serializable]
    public class ShakeRotationClip : ShakeClipBase
    {
        public override void ApplyFromState()
        {
            if (target == null)
            {
                return;
            }
            target.localEulerAngles = originalValue;
        }
        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
            }
            return TweenManager.ShakeRotation(target, startValue, strength, frequency, duration, dampingRatio, randomSeed, ease).Handle;
        }
    }
}
