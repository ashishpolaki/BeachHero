using DG.Tweening;
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
        public Transform target;
        public Vector3 toPosition;
        public SpaceType positionSpace = SpaceType.World;
        public Axis3D transformAxis = Axis3D.XYZ;

        public TransformPositionClip() : base()
        {
            moveTargetType = MoveTargetType.Transform;
        }

        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target Transform is null.");
                return null;
            }
            Vector3 dest = toPosition;

            //Local
            if (positionSpace == SpaceType.Local)
            {
                switch (transformAxis)
                {
                    case Axis3D.X: return target.DOLocalMoveX(dest.x, duration, snapping);
                    case Axis3D.Y: return target.DOLocalMoveY(dest.y, duration, snapping);
                    case Axis3D.Z: return target.DOLocalMoveZ(dest.z, duration, snapping);
                    case Axis3D.XYZ:
                    default: return target.DOLocalMove(dest, duration, snapping);
                }
            }
            //World
            else
            {
                switch (transformAxis)
                {
                    case Axis3D.X: return target.DOMoveX(dest.x, duration, snapping);
                    case Axis3D.Y: return target.DOMoveY(dest.y, duration, snapping);
                    case Axis3D.Z: return target.DOMoveZ(dest.z, duration, snapping);
                    case Axis3D.XYZ:
                    default: return target.DOMove(dest, duration, snapping);
                }
            }
        }

        public override void ApplyFromState()
        {
            if (target != null)
            {
                if(positionSpace == SpaceType.Local)
                    target.localPosition = fromPosition;
                else
                    target.position = fromPosition;
            }
        }
    }

    [Serializable]
    public class AnchorPositionClip : PositionClipBase
    {
        public RectTransform target;
        public Vector2 toAnchoredPosition;
        public Axis2D rectAxis = Axis2D.XY;

        public AnchorPositionClip() : base()
        {
            moveTargetType = MoveTargetType.RectTransform;
        }

        protected override Tween CreateTweenCore()
        {
            if (target == null)
            {
                DebugUtils.LogError("Target RectTransform is null.");
                return null;
            }
            switch (rectAxis)
            {
                case Axis2D.X: return target.DOAnchorPosX(toAnchoredPosition.x, duration, snapping);
                case Axis2D.Y: return target.DOAnchorPosY(toAnchoredPosition.y, duration, snapping);
                case Axis2D.XY:
                default: return target.DOAnchorPos(toAnchoredPosition, duration, snapping);
            }
        }

        public override void ApplyFromState()
        {
            if (target != null)
            {
                target.anchoredPosition = fromPosition;
            }
        }
    }
}
