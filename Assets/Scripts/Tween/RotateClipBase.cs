using LitMotion;
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

        public override bool IsTargetNull()
        {
            return target == null;
        }
    }
    [Serializable]
    public class RotateClip : RotateClipBase
    {
        public TransformSpace spaceType = TransformSpace.World;

        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
            }

            return TweenManager.RotateEulerAngles(target, fromRotation, toRotation, duration, ease, spaceType).Handle;
        }
        public override void ApplyFromState()
        {
            if (target != null)
            {
                if (spaceType == TransformSpace.World)
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
                if (spaceType == TransformSpace.World)
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
