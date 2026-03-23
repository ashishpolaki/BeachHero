using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LitMotion;

namespace BeachHero
{
    [Serializable]
    public class TriggerEvent
    {
        [Range(0f, 100f)] public float timePercent; // % of total duration
        public UnityEvent onTrigger;
    }

    public class TweenAnimator : MonoBehaviour
    {
        [SerializeReference] private TweenClipBase[] clips;
        [SerializeField] private List<TriggerEvent> triggerEvents = new();

        // public int loopCount = 0;              // 0 = no loop, -1 = infinite loop
        // public LoopType loopType = LoopType.Restart;
        public float delayFrames = 0;           // delay in seconds before starting the sequence
        public float timelineDuration = 1f;
        public TweenClipBase[] Clips => clips;

        public TweenSequence _sequence;

        public bool IsActive => _sequence.IsActive;
        public float Duration => _sequence.Duration;
        public void BuildSequence()
        {
            // kill previous
            _sequence.Cancel();

            ApplyAllFromStates();
            _sequence = new TweenSequence(LSequence.Create());
            _sequence.SetDelay(delayFrames);

#if UNITY_EDITOR
            //   if (!Application.isPlaying && loopCount < 0)
            //   {
            //      loopCount = 10; //  use a finite preview count in Edit Mode
            //  }
#endif

            // apply loop settings
            //_sequence.SetLoops(loopCount, loopType);

            if (clips == null)
            {
                return;
            }

            // Build Tweens
            foreach (var clip in clips)
            {
                if (clip == null)
                {
                    continue;
                }
                var tween = clip.GetTween();
                if (tween == null)
                {
                    continue;
                }
                _sequence.Insert(clip.startTime, tween);
            }

            // Add Trigger Events
            foreach (var trigger in triggerEvents)
            {
                if (trigger == null || trigger.onTrigger == null)
                {
                    continue;
                }
                float triggerTime = Mathf.Clamp01(trigger.timePercent / 100f) * timelineDuration;
                ;
                _sequence.Insert(triggerTime, TweenManager.RunCallback(() => trigger.onTrigger?.Invoke()).Handle);
            }
            _sequence.InitializeHandle();
            _sequence.Preserve();
            _sequence.SetPlaybackSpeed(0);
        }

        public void Play()
        {
            if (!_sequence.IsActive)
            {
                BuildSequence();
            }
            _sequence.SetPlaybackSpeed(1);
        }


        public void Kill()
        {
            KillAllClips();
            _sequence.Cancel();
        }

        private void KillAllClips()
        {
            if (clips == null) return;
            foreach (var c in clips)
            {
                c?.KillTween();
            }
        }

        public void ApplyAllFromStates()
        {
            if (clips == null) return;
            foreach (var c in clips)
            {
                c?.ApplyFromState();
            }
        }

        public void ApplyAllToStates()
        {
            if (clips == null) return;
            foreach (var c in clips)
            {
                c?.ApplyToState();
            }
        }
    }
}
