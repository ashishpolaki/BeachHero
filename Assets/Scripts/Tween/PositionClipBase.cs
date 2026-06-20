using LitMotion;
using System;
using UnityEngine;

namespace BeachHero
{
    public enum MoveTargetType
    {
        Transform,
        RectTransform,
        Rigidbody
    }
    [Serializable]
    public abstract class PositionClipBase : TweenClipBase
    {
        public MoveTargetType moveTargetType = MoveTargetType.Transform;
        public Vector3 fromPosition;
       

        public PositionClipBase()
        {
            clipType = TweenClipType.Move;
        }
    }

    [Serializable]
    public class TransformPositionClip : PositionClipBase
    {
        [SerializeField, HideInInspector]
        private Transform _cachedTarget;
        public Transform target;
        public Vector3 toPosition;
        public TransformSpace positionSpace = TransformSpace.World;
        public TransformAxis transformAxis = TransformAxis.XYZ;

        public TransformPositionClip() : base()
        {
            moveTargetType = MoveTargetType.Transform;
        }

        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                //  return null;
            }
            Vector3 dest = toPosition;

            //Local
            if (positionSpace == TransformSpace.Local)
            {
                switch (transformAxis)
                {
                    case TransformAxis.X: return TweenManager.Move(target, fromPosition, new Vector3(dest.x, fromPosition.y, fromPosition.z), duration, spaceType: positionSpace, ease: ease, onComplete: null).Handle;
                    case TransformAxis.Y: return TweenManager.Move(target, fromPosition, new Vector3(fromPosition.x, dest.y, fromPosition.z), duration, spaceType: positionSpace, ease: ease, onComplete: null).Handle;
                    case TransformAxis.Z: return TweenManager.Move(target, fromPosition, new Vector3(fromPosition.x, fromPosition.y, dest.z), duration, spaceType: positionSpace, ease: ease, onComplete: null).Handle;
                    case TransformAxis.XYZ:
                    default: return TweenManager.Move(target, fromPosition, dest, duration, spaceType: positionSpace, ease: ease, onComplete: null).Handle;
                }
            }
            //World
            else
            {
                switch (transformAxis)
                {
                    case TransformAxis.X: return TweenManager.Move(target, fromPosition, new Vector3(dest.x, fromPosition.y, fromPosition.z), duration, spaceType: positionSpace, ease: ease, onComplete: null).Handle;
                    case TransformAxis.Y: return TweenManager.Move(target, fromPosition, new Vector3(fromPosition.x, dest.y, fromPosition.z), duration, spaceType: positionSpace, ease: ease, onComplete: null).Handle;
                    case TransformAxis.Z: return TweenManager.Move(target, fromPosition, new Vector3(fromPosition.x, fromPosition.y, dest.z), duration, spaceType: positionSpace, ease: ease, onComplete: null).Handle;
                    case TransformAxis.XYZ:
                    default: return TweenManager.Move(target, fromPosition, dest, duration, spaceType: positionSpace, ease: ease, onComplete: null).Handle;
                }
            }
        }

        public override void ApplyFromState()
        {
            if (target != null)
            {
                if (positionSpace == TransformSpace.Local)
                    target.localPosition = fromPosition;
                else
                    target.position = fromPosition;
            }
        }

        public override void ApplyToState()
        {
            if (target != null)
            {
                if (positionSpace == TransformSpace.Local)
                    toPosition = target.localPosition;
                else
                    toPosition = target.position;
            }
        }
    }

    [Serializable]
    public class AnchorPositionClip : PositionClipBase
    {
        [SerializeField, HideInInspector]
        private Transform _cachedTarget;
        public RectTransform target;
        public Vector2 toAnchoredPosition;
        public TransformAxis rectAxis = TransformAxis.XY;

        public AnchorPositionClip() : base()
        {
            moveTargetType = MoveTargetType.RectTransform;
        }

        protected override MotionHandle CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target RectTransform is null.");
                // return null;
            }
            switch (rectAxis)
            {
                case TransformAxis.X:
                    return TweenManager.MoveAnchorOnAxis(target, target.anchoredPosition.x, toAnchoredPosition.x, duration, ease, rectAxis).Handle;
                case TransformAxis.Y:
                    return TweenManager.MoveAnchorOnAxis(target, target.anchoredPosition.y, toAnchoredPosition.y, duration, ease, rectAxis).Handle;
                case TransformAxis.XY:
                    return TweenManager.MoveAnchor(target, target.anchoredPosition, toAnchoredPosition, duration, ease, rectAxis).Handle;
            }
            return default;
        }

        public override void ApplyFromState()
        {
            if (target != null)
            {
                switch (rectAxis)
                {
                    case TransformAxis.X:
                        target.anchoredPosition = new Vector2(fromPosition.x, target.anchoredPosition.y);
                        break;
                    case TransformAxis.Y:
                        target.anchoredPosition = new Vector2(target.anchoredPosition.x, fromPosition.y);
                        break;
                    case TransformAxis.XY:
                    default:
                        target.anchoredPosition = fromPosition;
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
                        target.anchoredPosition = new Vector2(toAnchoredPosition.x, target.anchoredPosition.y);
                        break;
                    case TransformAxis.Y:
                        target.anchoredPosition = new Vector2(target.anchoredPosition.x, toAnchoredPosition.y);
                        break;
                    case TransformAxis.XY:
                    default:
                        target.anchoredPosition = toAnchoredPosition;
                        break;

                }
            }
        }
    }
}
