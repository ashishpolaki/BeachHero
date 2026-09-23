using LitMotion;
using System;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public abstract class SizeDeltaClipBase : TweenClipBase
    {
        public RectTransform target;
        public Vector2 fromSizeDelta;
        public Vector2 toSizeDelta;

        public SizeDeltaClipBase()
        {
            clipType = TweenClipType.SizeDelta;
        }

        public override bool IsTargetNull()
        {
            return target == null;
        }

        public virtual void CaptureFromState()
        {
            if (target != null)
            {
                fromSizeDelta = target.sizeDelta;
            }
        }

        public virtual void CaptureToState()
        {
            if (target != null)
            {
                toSizeDelta = target.sizeDelta;
            }
        }
    }

    [Serializable]
    public class SizeDeltaClip : SizeDeltaClipBase
    {
        public TransformAxis rectAxis = TransformAxis.XY;

        public SizeDeltaClip() : base()
        {
        }

        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target RectTransform is null.");
                return default;
            }

            switch (rectAxis)
            {
                case TransformAxis.X:
                    return TweenManager.SetSizeDeltaOnAxis(target, fromSizeDelta.x, toSizeDelta.x, duration, ease, rectAxis).Handle;
                case TransformAxis.Y:
                    return TweenManager.SetSizeDeltaOnAxis(target, fromSizeDelta.y, toSizeDelta.y, duration, ease, rectAxis).Handle;
                case TransformAxis.XY:
                default:
                    return TweenManager.SetSizeDelta(target, fromSizeDelta, toSizeDelta, duration, ease).Handle;
            }
        }

        public override void ApplyFromState()
        {
            if (target != null)
            {
                switch (rectAxis)
                {
                    case TransformAxis.X:
                        target.sizeDelta = new Vector2(fromSizeDelta.x, target.sizeDelta.y);
                        break;
                    case TransformAxis.Y:
                        target.sizeDelta = new Vector2(target.sizeDelta.x, fromSizeDelta.y);
                        break;
                    case TransformAxis.XY:
                    default:
                        target.sizeDelta = fromSizeDelta;
                        break;
                }
            }
        }

        public override void ApplyToState()
        {
            if (target != null)
            {
                switch (rectAxis)
                {
                    case TransformAxis.X:
                        target.sizeDelta = new Vector2(toSizeDelta.x, target.sizeDelta.y);
                        break;
                    case TransformAxis.Y:
                        target.sizeDelta = new Vector2(target.sizeDelta.x, toSizeDelta.y);
                        break;
                    case TransformAxis.XY:
                    default:
                        target.sizeDelta = toSizeDelta;
                        break;
                }
            }
        }
    }
}
