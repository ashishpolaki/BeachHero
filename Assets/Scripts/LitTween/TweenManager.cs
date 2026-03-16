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
           Vector3 from,
           Vector3 to,
           float duration,
           Ease ease = Ease.Linear,
           System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration)
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
                   Quaternion from,
                   Quaternion to,
                   float duration,
                   int loops = 0,
                   Ease ease = Ease.Linear,
                   System.Action onComplete = null)
        {
            var motion = LMotion.Create(target.rotation, to, duration)
                                .WithEase(ease);

            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }

            if (loops != 0)
            {
                motion.WithLoops(loops);
            }

            var handle = motion.BindToRotation(target);
            return new TweenHandle(handle);
        }

        public static TweenHandle RotateEulerAngles(
           Transform target,
           Vector3 byValue,
           float duration,
           int loops = 0,
           Ease ease = Ease.Linear,
           System.Action onComplete = null)
        {
            Quaternion startRot = target.rotation;
            var motion = LMotion.Create(Vector3.zero, byValue, duration).WithEase(ease);

            if (loops != 0)
            {
                motion.WithLoops(loops);
            }

            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }

            var handle = motion.Bind(x => target.rotation = startRot * Quaternion.Euler(x));
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
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            var handle = motion.BindToAnchoredPositionX(target);
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

        public static TweenHandle RunCallback(System.Action onComplete = null)
        {
            var motion = LMotion.Create(0, 0, 0);
            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);
            var handle = motion.RunWithoutBinding();
            return new TweenHandle(handle);
        }

    }

    public struct TweenHandle
    {
        private MotionHandle _handle;

        public bool IsActive => _handle.IsActive();
        public MotionHandle Handle => _handle;

        public TweenHandle(MotionHandle handle)
        {
            _handle = handle;
        }

        public void SetPlaybackSpeed(float speed)
        {
            _handle.PlaybackSpeed = speed;
        }

        public void SetSlider(float val)
        {
            _handle.Time = val;
        }

        public void Resume()
        {
            _handle.PlaybackSpeed = 1;
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

    public struct TweenSequence
    {
        private MotionSequenceBuilder sequenceBuilder;
        private MotionHandle handle;

        public MotionHandle Handle => handle;
        public bool IsActive => Handle.IsActive();

        public TweenSequence(MotionSequenceBuilder builder)
        {
            handle = default;
            sequenceBuilder = builder;
        }

        public void Append(MotionHandle motionHandle)
        {
            sequenceBuilder.Append(motionHandle);
        }

        public void AppendInterval(float interval)
        {
            sequenceBuilder.AppendInterval(interval);
        }

        public void Join(MotionHandle motionHandle)
        {
            sequenceBuilder.Join(motionHandle);
        }

        public void Insert(float time, MotionHandle motionHandle)
        {
            sequenceBuilder.Insert(time, motionHandle);
        }

        public void Cancel()
        {
            if (Handle.IsActive())
            {
                handle.Cancel();
            }
        }

        public void Play()
        {
            if (!handle.IsActive())
            {
                handle = sequenceBuilder.Run();
            }
        }
        public void Preserve()
        {
            handle.Preserve();
        }
        public void SetPlaybackSpeed(float speed)
        {
            handle.PlaybackSpeed = speed;
        }
        public void Complete()
        {
            if (handle.IsActive())
                handle.Complete();
        }
        public void SetSlider(float val)
        {
            handle.Time = val;
        }
    }
}

