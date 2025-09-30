using DG.Tweening;
using System;
using UnityEngine;

public enum TweenClipType
{
    Move,
    Scale,
    Rotate,
    AnchorPos,
    Fade
}
public enum MoveTargetType
{
    Transform,
    RectTransform,
    Rigidbody
}
public enum MoveInputType
{
    UseTarget,   // Animate the assigned Target transform
    UseVector3   // Animate a default target provided by the Sequencer, using From/To positions
}

[Serializable]
public abstract class TweenClipBase
{
    public TweenClipType clipType;
    public float startTime = 0f;
    public float duration = 1f;
    public Ease ease = Ease.Linear;
    public bool snapping = false;

    [NonSerialized] protected Tween _tween;

    public abstract Tween GetTween();

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

[Serializable]
public abstract class MoveClipBase : TweenClipBase
{
    public MoveTargetType moveTargetType = MoveTargetType.Transform;
    public MoveInputType moveDestinationType = MoveInputType.UseVector3;
    public Vector3 fromPosition;
}

[Serializable]
public class TransformMoveClip : MoveClipBase
{
    public Transform target;
    public Transform destinationTarget;
    public Vector3 toPosition;

    public override Tween GetTween()
    {
        if (target == null)
        {
            Debug.LogError("Target Transform is null.");
            return null;
        }
        Vector3 pos = moveDestinationType == MoveInputType.UseTarget ? destinationTarget.position : toPosition;
        return target.DOMove(pos, duration, snapping).SetEase(ease).SetAutoKill(false).Pause();
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
    public RectTransform destinationTarget;
    public Vector2 toAnchoredPosition;

    public override Tween GetTween()
    {
        if (target == null)
        {
            Debug.LogError("Target RectTransform is null.");
            return null;
        }
        Vector2 pos = moveDestinationType == MoveInputType.UseTarget ? destinationTarget.anchoredPosition : toAnchoredPosition;
        return target.DOAnchorPos(pos, duration, snapping).SetEase(ease).SetAutoKill(false).Pause();
    }

    public override void ApplyFromState()
    {
        if (target != null)
        {
            target.anchoredPosition = fromPosition;
        }
    }
}

[Serializable]
public class ScaleClip : TweenClipBase
{
    public Transform target;
    public Vector3 fromScale = Vector3.one;
    public Vector3 toScale = Vector3.one;

    public override Tween GetTween()
    {
        if (target == null)
        {
            Debug.LogError("Target Transform is null.");
            return null;
        }
        return target.DOScale(toScale, duration).SetEase(ease).SetAutoKill(false).Pause();
    }
    public override void ApplyFromState()
    {
        if (target != null)
        {
            target.localScale = fromScale;
        }
    }
}

