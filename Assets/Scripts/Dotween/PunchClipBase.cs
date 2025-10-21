using DG.Tweening;
using System;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public abstract class PunchClipBase : TweenClipBase
    {
        public Transform target;
        public Vector3 punch = Vector3.one;
        public int vibrato = 10;
        public float elasticity = 1f;
        public Vector3 originalScale = Vector3.one;

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
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }
            return target.DOPunchPosition(punch, duration, vibrato, elasticity, snapping);
        }
    }

    [Serializable]
    public class PunchRotationClip : PunchClipBase
    {
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }
            return target.DOPunchRotation(punch, duration, vibrato, elasticity);
        }
    }

    [Serializable]
    public class PunchScaleClip : PunchClipBase
    {
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }
            return target.DOPunchScale(punch, duration, vibrato, elasticity);
        }
    }

    [Serializable]
    public class PunchAnchorPosClip : PunchClipBase
    {
        protected override Tween CreateTweenCore()
        {
            RectTransform rectTransform = target as RectTransform;
            if (rectTransform == null)
            {
                DebugUtils.LogError("Target is not a RectTransform.");
                return null;
            }
            return rectTransform.DOPunchAnchorPos(punch, duration, vibrato, elasticity, snapping);
        }
    }
}
