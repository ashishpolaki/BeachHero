using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BeachHero
{
    [Serializable]
    public class TriggerEvent
    {
        [Range(0f, 100f)] public float timePercent; // % of total duration
        public UnityEvent onTrigger;
    }

    public class TweenSequencer : MonoBehaviour
    {
        [SerializeReference] private TweenClipBase[] clips;
        [SerializeField] private List<TriggerEvent> triggerEvents = new();

        public int loopCount = 0;              // 0 = no loop, -1 = infinite loop
        public float delayFrames = 0;           // delay in seconds before starting the sequence
        public LoopType loopType = LoopType.Restart;
        public float timelineDuration = 1f;
        public TweenClipBase[] Clips => clips;

        public Sequence _sequence;

        public void BuildSequence()
        {
            // kill previous
            if (_sequence != null && _sequence.IsActive())
            {
                _sequence.Kill();
                _sequence = null;
            }

            ApplyAllFromStates();
            _sequence.SetDelay(delayFrames);
            _sequence = DOTween.Sequence().SetAutoKill(false).Pause();

#if UNITY_EDITOR
            if (!Application.isPlaying && loopCount < 0)
            {
                loopCount = 10; //  use a finite preview count in Edit Mode
            }
#endif

            // apply loop settings
            _sequence.SetLoops(loopCount, loopType);

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
                _sequence.InsertCallback(triggerTime, () => trigger.onTrigger?.Invoke());
            }
        }

        public void Play()
        {
            if (_sequence == null || !_sequence.IsActive())
            {
                BuildSequence();
            }
            _sequence.Restart();
        }

        public void Pause()
        {
            _sequence?.Pause();
        }

        public void Kill()
        {
            KillAllClips();
            if (_sequence != null && _sequence.IsActive())
            {
                _sequence.Kill();
                _sequence = null;
            }
        }

        private void KillAllClips()
        {
            if (clips == null) return;
            foreach (var c in clips) c?.KillTween();
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
