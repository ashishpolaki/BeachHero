// TweenManager.cs - Complete Static Manager for LitMotion
using UnityEngine;
using LitMotion;
using LitMotion.Extensions;

namespace BeachHero
{
    /// <summary>
    /// Static manager class for managing LitMotion tweens with automatic tracking and grouping capabilities.
    /// Zero-allocation, high-performance tween library wrapper.
    /// </summary>
    public static class TweenManager
    {
        #region Transform
        public static TweenHandle Move(
           Transform target,
           Vector3 to,
           float duration,
           Ease ease = Ease.Linear,
           System.Action onComplete = null)
        {
            var motion = LMotion.Create(target.position, to, duration)
                                .WithEase(ease);

            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            var handle = motion.BindToPosition(target);
            return new TweenHandle(handle);
        }

        public static TweenHandle LocalMove(
          Transform target,
          Vector3 to,
          float duration,
          Ease ease = Ease.Linear,
          System.Action onComplete = null)
        {
            var motion = LMotion.Create(target.localPosition, to, duration)
                                .WithEase(ease);

            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            var handle = motion.BindToLocalPosition(target);
            return new TweenHandle(handle);
        }

        #endregion

        #region Scale

        public static TweenHandle Scale(
      Vector3 from,
      Vector3 to,
      Transform target,
      float duration,
      Ease ease = Ease.Linear,
      System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration)
                                .WithEase(ease);

            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            var handle = motion.BindToLocalScale(target);
            return new TweenHandle(handle);
        }

        #endregion

        #region Rotation

        public static TweenHandle Rotate(
     Transform target,
     Quaternion to,
     float duration,
     Ease ease = Ease.Linear,
     System.Action onComplete = null)
        {
            var motion = LMotion.Create(target.rotation, to, duration)
                                .WithEase(ease);

            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            var handle = motion.BindToRotation(target);
            return new TweenHandle(handle);
        }

        public static TweenHandle RotateBy(
           Transform target,
           Vector3 byValue,
           float duration,
           Ease ease = Ease.Linear,
           System.Action onComplete = null)
        {
            var start = target.eulerAngles;
            var end = start + byValue;

            var motion = LMotion.Create(start, end, duration)
                                .WithEase(ease);

            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            var handle = motion.Bind(x => target.eulerAngles = x);
            return new TweenHandle(handle);
        }
        #endregion

        #region Rect
        public static TweenHandle AnchoredPositionX(
            RectTransform target,
            float from,
            float to,
            float duration,
            Ease ease = Ease.Linear,
            System.Action onComplete = null)
        {
            var motion = LMotion.Create(target.anchoredPosition.x, to, duration)
                                .WithEase(ease);
            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);
            var handle = motion.Bind(x => target.anchoredPosition = new Vector2(x, target.anchoredPosition.y));
            return new TweenHandle(handle);
        }
        #endregion

        public static TweenHandle Float(
            float from,
            float to,
            float duration,
            System.Action<float> setter,
            Ease ease = Ease.Linear,
            System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration);

            if (ease != Ease.Linear)
                motion = motion.WithEase(ease);

            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            var handle = motion.Bind(setter);

            return new TweenHandle(handle);
        }
    }

    public struct TweenHandle
    {
        private MotionHandle _handle;

        public bool IsActive => _handle.IsActive();

        public TweenHandle(MotionHandle handle)
        {
            _handle = handle;
        }

        public void Cancel()
        {
            if (_handle.IsActive())
                _handle.Cancel();
        }

        public void Complete()
        {
            if (_handle.IsActive())
                _handle.Complete();
        }
    }
}

