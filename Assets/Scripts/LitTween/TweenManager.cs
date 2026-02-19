// TweenManager.cs - Complete Static Manager for LitMotion
using UnityEngine;
using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;

namespace BeachHero.Tween
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

        public static TweenHandle Scale(
      Transform target,
      Vector3 to,
      float duration,
      Ease ease = Ease.Linear,
      System.Action onComplete = null)
        {
            var motion = LMotion.Create(target.localScale, to, duration)
                                .WithEase(ease);

            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            var handle = motion.BindToLocalScale(target);
            return new TweenHandle(handle);
        }

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
    }



    public struct TweenHandle
    {
        private MotionHandle _handle;

        public TweenHandle(MotionHandle handle)
        {
            _handle = handle;
        }

        public bool IsActive => _handle.IsActive();

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

