using UnityEngine;
using DG.Tweening;
using System;

namespace BeachHero
{
    [Serializable]
    public abstract class BlendableClipBase : TweenClipBase
    {
        public Transform target;
        public Vector3 byValue = Vector3.one;

        public BlendableClipBase()
        {
            clipType = TweenClipType.Blendable;
        }
    }

    [Serializable]
    public class BlendablePositionClip : BlendableClipBase
    {
        public SpaceType spaceType = SpaceType.World;
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }

            if (spaceType == SpaceType.Local)
            {
                return target.DOBlendableLocalMoveBy(byValue, duration, snapping);
            }
            else
            {
                return target.DOBlendableMoveBy(byValue, duration, snapping);
            }
        }
    }

    [Serializable]
    public class BlendableRotationClip : BlendableClipBase
    {
        public SpaceType spaceType = SpaceType.World;
        public RotateMode rotateMode;

        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }

            if (spaceType == SpaceType.Local)
            {
                return target.DOBlendableLocalRotateBy(byValue, duration, rotateMode);
            }
            else
            {
                return target.DOBlendableRotateBy(byValue, duration, rotateMode);
            }
        }
    }

    [Serializable]
    public class BlendableScaleClip : BlendableClipBase
    {
        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }
            return target.DOBlendableScaleBy(byValue, duration);
        }
    }

    [Serializable]
    public class BlendablePunchRotationClip : BlendableClipBase
    {
        public int vibrato = 10;
        public float elasticity = 1f;

        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }
            return target.DOPunchRotation(byValue, duration, vibrato, elasticity);
        }
    }
}
