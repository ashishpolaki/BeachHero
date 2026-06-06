// TweenManager.cs - Complete Static Manager for LitMotion
using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    /// <summary>
    /// Static manager class for managing LitMotion tweens with automatic tracking and grouping capabilities.
    /// Zero-allocation, high-performance tween library wrapper.
    /// </summary>
    public static class TweenManager
    {
        #region Transform
        public static TweenHandle Move(Transform target, Vector3 from, Vector3 to, float duration,
            int loopCount = 0, LoopType loopType = LoopType.Restart, TransformSpace spaceType = TransformSpace.World,
            Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            var handle = default(TweenHandle);

            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            switch (spaceType)
            {
                case TransformSpace.World:
                    handle = new TweenHandle(motion.BindToPosition(target));
                    break;
                case TransformSpace.Local:
                    handle = new TweenHandle(motion.BindToLocalPosition(target));
                    break;
            }
            if (loopCount != 0)
            {
                motion.WithLoops(loopCount, loopType);
            }
            return handle;
        }

        public static TweenHandle MoveOnAxis(Transform target, float from, float to, float duration, TransformAxis axis = TransformAxis.XYZ,
            TransformSpace spaceType = TransformSpace.World, Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            var handle = default(TweenHandle);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }

            if (spaceType == TransformSpace.Local)
            {
                switch (axis)
                {
                    case TransformAxis.X:
                        handle = new TweenHandle(motion.BindToLocalPositionX(target));
                        break;
                    case TransformAxis.Y:
                        handle = new TweenHandle(motion.BindToLocalPositionY(target));
                        break;
                    case TransformAxis.Z:
                        handle = new TweenHandle(motion.BindToLocalPositionZ(target));
                        break;
                }
            }
            else if (spaceType == TransformSpace.World)
            {
                switch (axis)
                {
                    case TransformAxis.X:
                        handle = new TweenHandle(motion.BindToPositionX(target));
                        break;
                    case TransformAxis.Y:
                        handle = new TweenHandle(motion.BindToPositionY(target));
                        break;
                    case TransformAxis.Z:
                        handle = new TweenHandle(motion.BindToPositionZ(target));
                        break;
                }
            }
            return handle;
        }
        #endregion

        #region Scale
        public static TweenHandle Scale(Vector3 from, Vector3 to, Transform target, float duration,
            Ease ease = Ease.Linear,
      Action onComplete = null, int loops = 0, LoopType loopType = LoopType.Restart)
        {
            var motion = LMotion.Create(from, to, duration)
                                .WithEase(ease);

            if (loops != 0)
            {
                motion.WithLoops(loops, loopType);
            }

            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            var handle = motion.BindToLocalScale(target);
            return new TweenHandle(handle);
        }

        public static TweenHandle ScaleOnAxis(float from, float to, Transform target,
            float duration, Ease ease = Ease.Linear, TransformAxis transformAxis = TransformAxis.XYZ, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            var handle = default(TweenHandle);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            switch (transformAxis)
            {
                case TransformAxis.X:
                    handle = new TweenHandle(motion.BindToLocalScaleX(target));
                    break;
                case TransformAxis.Y:
                    handle = new TweenHandle(motion.BindToLocalScaleY(target));
                    break;
                case TransformAxis.Z:
                    handle = new TweenHandle(motion.BindToLocalScaleZ(target));
                    break;
            }
            return handle;
        }

        #endregion

        #region Rotation
        public static TweenHandle Rotate(Transform target, Quaternion from, Quaternion to,
         float duration, Ease ease = Ease.Linear, TransformSpace spaceType = TransformSpace.World,
         int loops = 0, LoopType loopType = LoopType.Restart, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration)
                                .WithEase(ease);
            var handle = default(TweenHandle);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }

            if (loops != 0)
            {
                motion.WithLoops(loops, loopType);
            }

            switch (spaceType)
            {
                case TransformSpace.World:
                    handle = new TweenHandle(motion.BindToRotation(target));
                    break;
                case TransformSpace.Local:
                    handle = new TweenHandle(motion.BindToLocalRotation(target));
                    break;
            }

            return handle;
        }

        public static TweenHandle RotateEulerAngles(Transform target, Vector3 fromVal, Vector3 toValue,
            float duration, Ease ease = Ease.Linear, TransformSpace spaceType = TransformSpace.World,
           int loops = 0, LoopType loopType = LoopType.Restart, System.Action onComplete = null)
        {
            Quaternion startRot = target.rotation;
            var motion = LMotion.Create(fromVal, toValue, duration).WithEase(ease);

            if (loops != 0)
            {
                motion.WithLoops(loops, loopType);
            }

            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }

            var handle = motion.BindToEulerAngles(target);
            return new TweenHandle(handle);
        }
        #endregion

        #region Rect
        public static TweenHandle SetSizeDelta(RectTransform target, Vector2 from, Vector2 to, float duration,
            Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            var handle = default(TweenHandle);

            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            handle = new TweenHandle(motion.BindToSizeDelta(target));
            return handle;
        }
        public static TweenHandle MoveAnchorOnAxis(RectTransform target, float from, float to, float duration,
            Ease ease = Ease.Linear, TransformAxis transformAxis = TransformAxis.XY,
            int loops = 0, LoopType loopType = LoopType.Restart, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            var handle = default(TweenHandle);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            if (loops != 0)
            {
                motion = motion.WithLoops(loops, loopType);
            }
            switch (transformAxis)
            {
                case TransformAxis.X:
                    handle = new TweenHandle(motion.BindToAnchoredPositionX(target));
                    break;
                case TransformAxis.Y:
                    handle = new TweenHandle(motion.BindToAnchoredPositionY(target));
                    break;
            }

            return handle;
        }
        public static TweenHandle MoveAnchor(RectTransform target, Vector2 from, Vector2 to, float duration,
            Ease ease = Ease.Linear, TransformAxis transformAxis = TransformAxis.XY, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            var handle = default(TweenHandle);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            switch (transformAxis)
            {
                case TransformAxis.XY:
                    handle = new TweenHandle(motion.BindToAnchoredPosition(target));
                    break;
            }
            return handle;
        }
        #endregion

        #region Punch
        public static TweenHandle PunchPosition(Transform transform, Vector3 amplitude, Vector3 strength,
            int frequency, float dampingRatio, float duration, Ease ease = Ease.Linear,
            TransformSpace transformSpace = TransformSpace.World, System.Action onComplete = null)
        {
            var motion = LMotion.Punch.Create(amplitude, strength, duration).WithFrequency(frequency).WithDampingRatio(dampingRatio).WithEase(ease);
            TweenHandle handle = default;
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            if (transformSpace == TransformSpace.World)
            {
                handle = new TweenHandle(motion.BindToPosition(transform));
            }
            else
            {
                handle = new TweenHandle(motion.BindToLocalPosition(transform));
            }
            return handle;
        }

        public static TweenHandle PunchScale(Transform transform, Vector3 amplitude, Vector3 strength,
          int frequency, float dampingRatio, float duration, Ease ease = Ease.Linear,
          Action onComplete = null)
        {
            var motion = LMotion.Punch.Create(amplitude, strength, duration).WithFrequency(frequency)
                .WithDampingRatio(dampingRatio).WithEase(ease);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            TweenHandle handle = new TweenHandle(motion.BindToLocalScale(transform));
            return handle;
        }
        #endregion

        #region Shake
        public static TweenHandle ShakePosition(Transform transform, Vector3 startValue, Vector3 strength,
            int frequency, float duration, float dampingRatio = 0, uint randomSeed = 123,
            Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Shake.Create(startValue, strength, duration).WithFrequency(frequency)
             .WithDampingRatio(dampingRatio).WithRandomSeed(randomSeed).WithEase(ease);

            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }

            var handle = motion.BindToPosition(transform);
            return new TweenHandle(handle);
        }
        public static TweenHandle ShakeScale(Transform transform, Vector3 startValue, Vector3 strength,
            int frequency, float duration, float dampingRatio = 0, uint randomSeed = 123,
            Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Shake.Create(startValue, strength, duration).WithFrequency(frequency)
             .WithDampingRatio(dampingRatio).WithRandomSeed(randomSeed).WithEase(ease);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            var handle = motion.BindToLocalScale(transform);
            return new TweenHandle(handle);
        }
        #endregion

        #region Generic Value Tweens

        public static TweenHandle SetFloat(float from, float to, float duration,
            Action<float> setter = null, Ease ease = Ease.Linear, float delay = 0f,
            int loops = 0, LoopType loopType = LoopType.Restart, Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration);

            //Ease
            if (ease != Ease.Linear)
                motion = motion.WithEase(ease);

            //Delay
            if (delay > 0f)
            {
                motion = motion.WithDelay(delay, DelayType.FirstLoop);
            }

            //Loops
            if (loops != 0)
            {
                motion.WithLoops(loops, loopType);
            }

            //OnComplete
            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            var handle = motion.Bind(setter);

            return new TweenHandle(handle);
        }

        public static TweenHandle SetVector3(Vector3 from, Vector3 to, float duration,
            System.Action<Vector3> setter, int loops = 0, Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration);
            if (ease != Ease.Linear)
                motion = motion.WithEase(ease);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            if (loops != 0)
            {
                motion.WithLoops(loops);
            }
            var handle = motion.Bind(setter);
            return new TweenHandle(handle);
        }

        public static TweenHandle RunCallback(System.Action onComplete = null)
        {
            var motion = LMotion.Create(0, 0, 0f);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            var handle = motion.RunWithoutBinding();
            return new TweenHandle(handle);
        }
        #endregion

        #region Image
        public static TweenHandle FillAmount(Image target, float from, float to, float duration,
            Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            var handle = motion.BindToFillAmount(target);
            return new TweenHandle(handle);
        }
        public static TweenHandle Fade(Image target, float from, float to, float duration,
            Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            var handle = motion.BindToColorA(target);
            return new TweenHandle(handle);
        }
        public static TweenHandle Color(Image target, Color from, Color to, float duration,
            Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            var handle = motion.BindToColor(target);
            return new TweenHandle(handle);
        }
        #endregion

        #region CanvasGroup 
        public static TweenHandle Fade(CanvasGroup target, float from, float to, float duration,
          Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            var motion = LMotion.Create(from, to, duration).WithEase(ease);
            if (onComplete != null)
            {
                motion = motion.WithOnComplete(onComplete);
            }
            var handle = motion.BindToAlpha(target);
            return new TweenHandle(handle);
        }
        #endregion

        #region Animations
        public static TweenSequence PlayButtonIntroAttentionAnimation(Transform transform, float duration = 0.2f, float _scale = 1.1f)
        {
            TweenSequence seq = new TweenSequence(LSequence.Create());
            // Intro attention (snappy - Quad)
            var tweenHandle1 = Scale(Vector3.zero, Vector3.one * _scale, transform, duration, Ease.OutQuad);
            seq.Append(tweenHandle1.Handle);
            var tweenHandle2 = Scale(Vector3.one * _scale, Vector3.one, transform, duration, Ease.InQuad);
            seq.Append(tweenHandle2.Handle);
            return seq;
        }
        public static TweenHandle PlayIdleLoopAnimation(Transform transform, float duration = 0.8f, float _scale = 1.1f)
        {
            //  Idle loop (smooth - Sine)
            return Scale(Vector3.one, Vector3.one * _scale, transform, duration, Ease.InOutSine,
                  loops: -1, loopType: LoopType.Yoyo);
        }

        #endregion
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
        public bool IsPlaying => handle.IsPlaying();
        public bool IsValid => handle.IsValid();
        public float Duration => (float)handle.TotalDuration;
        public float CurrentDuration => (float)sequenceBuilder.CurrentDuration;

        public TweenSequence(MotionSequenceBuilder builder)
        {
            handle = default;
            sequenceBuilder = builder;
        }

        public void Append(MotionHandle motionHandle)
        {
            sequenceBuilder.Append(motionHandle);
        }

        public void SetDelay(float delay)
        {
            AppendInterval(delay);
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

        public void OnComplete(MotionHandle motionHandle)
        {
            sequenceBuilder.Insert(sequenceBuilder.CurrentDuration, motionHandle);
        }
        public void Cancel()
        {
            if (Handle.IsActive())
            {
                handle.Cancel();
            }
        }

        public void InitializeHandle()
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
        public void SetTime(float val)
        {
            handle.Time = val;
        }
    }
}

