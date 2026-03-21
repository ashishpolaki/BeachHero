using LitMotion;
using System;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public abstract class PunchClipBase : TweenClipBase
    {
        public Transform target;
        public Vector3 originalScale = Vector3.one;
        public Vector3 startValue = Vector3.one;
        public Vector3 strength = Vector3.one;
        public TransformSpace transformSpace = TransformSpace.World;
        public int frequency = 1;

        public PunchClipBase()
        {
            clipType = TweenClipType.Punch;
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
    public class PunchPositionClip : PunchClipBase
    {
        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
            }
            return TweenManager.PunchPosition(target, startValue, strength, frequency, 0, duration, ease, transformSpace, null).Handle;
        }
    }

    [Serializable]
    public class PunchScaleClip : PunchClipBase
    {
        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
            }
            return TweenManager.PunchScale(target, startValue, strength, frequency, 0, duration, ease, null).Handle;
        }
    }
}
