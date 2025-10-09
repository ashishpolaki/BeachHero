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

        public ShakeClipBase()
        {
            clipType = TweenClipType.Shake;
        }
    }

    [Serializable]
    public class ShakePositionClip : ShakeClipBase
    {
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                Debug.LogError("Target Transform is null.");
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
                Debug.LogError("Target Transform is null.");
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
                Debug.LogError("Target Transform is null.");
                return null;
            }
            return target.DOShakeScale(duration, strength, vibrato, randomness, fadeOut);
        }
    }
}
