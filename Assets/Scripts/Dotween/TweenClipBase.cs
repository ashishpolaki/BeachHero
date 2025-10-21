using DG.Tweening;
using System;

namespace BeachHero
{
    public enum TweenClipType
    {
        Move,
        Scale,
        Rotate,
        Shake,
        Punch,
        Jump,
        Blendable,
        CanvasGroup,
        Image
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
    public enum SpaceType
    {
        World,
        Local
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
}


