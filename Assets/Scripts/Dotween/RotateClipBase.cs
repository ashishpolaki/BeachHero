using DG.Tweening;
using System;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public abstract class RotateClipBase : TweenClipBase
    {
        public Vector3 fromRotation = Vector3.zero;
        public Vector3 toRotation = Vector3.zero;
        public Transform target;

        public RotateClipBase()
        {
            clipType = TweenClipType.Rotate;
        }
    }
    [Serializable]
    public class RotateClip : RotateClipBase
    {
        public RotateMode rotateMode = RotateMode.Fast;
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
                return target.DOLocalRotate(toRotation, duration, rotateMode);
            }
            else
            {
                return target.DORotate(toRotation, duration, rotateMode);
            }
        }
        public override void ApplyFromState()
        {
            if (target != null)
            {
                if (spaceType == SpaceType.World)
                {
                    target.eulerAngles = fromRotation;
                }
                else
                {
                    target.localEulerAngles = fromRotation;
                }
            }
        }

        public override void ApplyToState()
        {
            if (target != null)
            {
                if (spaceType == SpaceType.World)
                {
                    target.eulerAngles = toRotation;
                }
                else
                {
                    target.localEulerAngles = toRotation;
                }
            }
        }
    }
}
