using LitMotion;
using System;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public abstract class ScaleClipBase : TweenClipBase
    {
        public Vector3 fromScale = Vector3.one;
        public Vector3 toScale = Vector3.one;
        public Transform target;

        public ScaleClipBase()
        {
            clipType = TweenClipType.Scale;
        }
    }

    [Serializable]
    public class ScaleClip : ScaleClipBase
    {
        public TransformAxis scaleAxis = TransformAxis.XYZ;

        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
            }
            switch (scaleAxis)
            {
                case TransformAxis.X:
                    return TweenManager.ScaleOnAxis(fromScale.x, toScale.x, target, duration, ease, scaleAxis).Handle;
                case TransformAxis.Y:
                    return TweenManager.ScaleOnAxis(fromScale.y, toScale.y, target, duration, ease, scaleAxis).Handle;
                case TransformAxis.Z:
                    return TweenManager.ScaleOnAxis(fromScale.z, toScale.z, target, duration, ease, scaleAxis).Handle;
                case TransformAxis.XYZ:
                    return TweenManager.Scale(fromScale, toScale, target, duration, ease).Handle;
            }
            return default;
        }
        public override void ApplyFromState()
        {
            if (target != null)
            {
                target.localScale = fromScale;
            }
        }
        public override void ApplyToState()
        {
            if (target != null)
            {
                target.localScale = toScale;
            }
        }
    }
}
