using DG.Tweening;
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
        public Axis3D scaleAxis = Axis3D.XYZ;

        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                Debug.LogError("Target Transform is null.");
                return null;
            }

            switch(scaleAxis)
            {
                case Axis3D.X: return target.DOScaleX(toScale.x, duration);
                case Axis3D.Y: return target.DOScaleY(toScale.y, duration);
                case Axis3D.Z: return target.DOScaleZ(toScale.z, duration);
                case Axis3D.XYZ:
                default:
                    return target.DOScale(toScale, duration);
            }

        }
        public override void ApplyFromState()
        {
            if (target != null)
            {
                target.localScale = fromScale;
            }
        }
    }
}
