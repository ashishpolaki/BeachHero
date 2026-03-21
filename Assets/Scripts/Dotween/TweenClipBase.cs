using System;
using LitMotion;

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
    public enum TransformAxis
    {
        X,
        Y,
        Z,
        XY,
        XYZ
    }
    public enum TransformSpace
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
        //public float overshoot = 1.70158f; // for Back eases
        //public float amplitude = 1f;  // for Elastic eases
        //public float period = 0f; // for Elastic/Flash eases

        [NonSerialized] protected MotionHandle _tween;

        public MotionHandle GetTween()
        {
            _tween = CreateTweenCore();
            return _tween;

            //if (tween == null)
            //{
            //    return null;
            //}
            //switch (ease)
            //{
            //    case Ease.InBack:
            //    case Ease.OutBack:
            //    case Ease.InOutBack:
            //        tween.SetEase(ease, overshoot);
            //        break;

            //    case Ease.InElastic:
            //    case Ease.OutElastic:
            //    case Ease.InOutElastic:
            //    //case Ease.InFlash:
            //    //case Ease.OutFlash:
            //    //case Ease.InOutFlash:
            //        tween.SetEase(ease, amplitude, period);
            //        break;

            //    default:
            //        tween.SetEase(ease);
            //        break;
            //}

            //tween.SetAutoKill(false).Pause();
        }

        protected abstract MotionHandle CreateTweenCore();

        // Apply stored "from" state to the target(s). Override in subclasses.
        public virtual void ApplyFromState() { }

        public virtual void ApplyToState() { }

        public virtual void KillTween()
        {
            //if (_tween.IsPlaying())
            //{
            //    _tween.Cancel();
            //}
        }
    }
}


