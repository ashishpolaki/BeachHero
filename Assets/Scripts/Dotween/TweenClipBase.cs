using DG.Tweening;
using System;
using UnityEditor;
using UnityEngine;

public enum TweenClipType
{
    Move,
    Scale,
    Rotate,
    Shake,
}
[Serializable]
public abstract class TweenClipBase
{
    public TweenClipType clipType;
    public float startTime = 0f;
    public float duration = 1f;
    public Ease ease = Ease.Linear;
    public bool snapping = false;
    public float overshoot = 1.70158f; // for Back eases
    public float amplitude = 1f;  // for Elastic eases
    public float period = 0f; // for Elastic/Flash eases

    [NonSerialized] protected Tween _tween;

    public Tween GetTween()
    {
        var tween = CreateTweenCore();
        if (tween == null)
        {
            return null;
        }
        switch (ease)
        {
            case Ease.InBack:
            case Ease.OutBack:
            case Ease.InOutBack:
                tween.SetEase(ease, overshoot);
                break;

            case Ease.InElastic:
            case Ease.OutElastic:
            case Ease.InOutElastic:
            case Ease.InFlash:
            case Ease.OutFlash:
            case Ease.InOutFlash:
                tween.SetEase(ease, amplitude, period);
                break;

            default:
                tween.SetEase(ease);
                break;
        }

        tween.SetAutoKill(false).Pause();
        _tween = tween;
        return _tween;
    }

    protected abstract Tween CreateTweenCore();

    // Apply stored "from" state to the target(s). Override in subclasses.
    public virtual void ApplyFromState() { }

    public virtual void KillTween()
    {
        if (_tween != null && _tween.IsActive())
        {
            _tween.Kill();
            _tween = null;
        }
    }
}

#region Move
public enum MoveTargetType
{
    Transform,
    RectTransform,
    Rigidbody
}
public enum SpaceType
{
    World,
    Local
}
public enum Axis3D
{
    X,
    Y,
    Z,
    XYZ
}
public enum Axis2D
{
    X,
    Y,
    XY
}

[Serializable]
public abstract class MoveClipBase : TweenClipBase
{
    public MoveTargetType moveTargetType = MoveTargetType.Transform;
    public Vector3 fromPosition;

    public MoveClipBase()
    {
        clipType = TweenClipType.Move;
    }
}

[Serializable]
public class TransformMoveClip : MoveClipBase
{
    public Transform target;
    public Vector3 toPosition;
    public SpaceType positionSpace = SpaceType.World;
    public Axis3D transformAxis = Axis3D.XYZ;

    public TransformMoveClip() : base()
    {
        moveTargetType = MoveTargetType.Transform;
    }

    protected override Tween CreateTweenCore()
    {
        if (target == null)
        {
            Debug.LogError("Target Transform is null.");
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
            target.position = fromPosition;
        }
    }
}

[Serializable]
public class RectTransformMoveClip : MoveClipBase
{
    public RectTransform target;
    public Vector2 toAnchoredPosition;
    public Axis2D rectAxis = Axis2D.XY;

    public RectTransformMoveClip() : base()
    {
        moveTargetType = MoveTargetType.RectTransform;
    }

    protected override Tween CreateTweenCore()
    {
        if (target == null)
        {
            Debug.LogError("Target RectTransform is null.");
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
#endregion

#region Rotate
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

    protected override Tween CreateTweenCore()
    {
        if (target == null)
        {
            Debug.LogError("Target Transform is null.");
            return null;
        }
        return target.DORotate(toRotation, duration, rotateMode);
    }
    public override void ApplyFromState()
    {
        if (target != null)
        {
            target.eulerAngles = fromRotation;
        }
    }
}

[Serializable]
public class PunchRotateClip : RotateClipBase
{
    public int vibrato = 10;
    public float elasticity = 1f;

    protected override Tween CreateTweenCore()
    {
        if (target == null)
        {
            Debug.LogError("Target Transform is null.");
            return null;
        }
        return target.DOPunchRotation(toRotation, duration, vibrato, elasticity);
    }
    public override void ApplyFromState()
    {
        if (target != null)
        {
            target.eulerAngles = fromRotation;
        }
    }
}

[Serializable]
public class LocalRotateClip : RotateClipBase
{
    public RotateMode rotateMode = RotateMode.Fast;

    protected override Tween CreateTweenCore()
    {
        if (target == null)
        {
            Debug.LogError("Target Transform is null.");
            return null;
        }
        return target.DOLocalRotate(toRotation, duration, rotateMode);
    }
    public override void ApplyFromState()
    {
        if (target != null)
        {
            target.localEulerAngles = fromRotation;
        }
    }
}
#endregion

#region Scale
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
    protected override Tween CreateTweenCore()
    {
        if (target == null)
        {
            Debug.LogError("Target Transform is null.");
            return null;
        }
        return target.DOScale(toScale, duration);
    }
    public override void ApplyFromState()
    {
        if (target != null)
        {
            target.localScale = fromScale;
        }
    }
}

[Serializable]
public class PunchScaleClip : ScaleClipBase
{
    public int vibrato = 10;
    public float elasticity = 1f;

    protected override Tween CreateTweenCore()
    {
        if (target == null)
        {
            Debug.LogError("Target Transform is null.");
            return null;
        }
        return target.DOPunchScale(toScale, duration, vibrato, elasticity);
    }
    public override void ApplyFromState()
    {
        if (target != null)
        {
            target.localScale = fromScale;
        }
    }
}

[Serializable]
public class BlendableScaleClip : ScaleClipBase
{
    protected override Tween CreateTweenCore()
    {
        if (target == null)
        {
            Debug.LogError("Target Transform is null.");
            return null;
        }
        return target.DOBlendableScaleBy(toScale, duration);
    }
    public override void ApplyFromState()
    {
        if (target != null)
        {
            target.localScale = fromScale;
        }
    }
}
#endregion


