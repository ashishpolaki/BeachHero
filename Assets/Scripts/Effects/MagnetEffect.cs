using LitMotion;
using UnityEngine;

namespace BeachHero
{
    public class MagnetEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ripplePrefab;
        [SerializeField] private int rippleCount = 4;
        [SerializeField] private float rippleInterval = 0.3f;
        [SerializeField] private float rippleFadeInDuration = 0.5f;
        [SerializeField] private float rippleShrinkDuration = 1f;
        [SerializeField] private float delayBetweenLoops = 1f;
        [SerializeField] private float startScale = 1.5f;
        [SerializeField] private Ease rippleEase = Ease.Linear;
        [SerializeField] private float rotationSpeed = 90f; // degrees per second

        private SpriteRenderer[] ripples;
        private TweenSequence loopSequence;
        private TweenHandle rotationTweenHandle;

        public void PlayRippleEffect()
        {
            if (ripples == null || ripples.Length <= 0)
            {
                ripples = new SpriteRenderer[rippleCount];
                for (int i = 0; i < rippleCount; i++)
                {
                    SpriteRenderer ripple = Instantiate(ripplePrefab, transform);
                    ripple.transform.localScale = Vector3.one * startScale;
                    ripples[i] = ripple;
                }
            }
            AnimateRipples();
        }

        private void AnimateRipples()
        {
            //KillTween();
            float duration = 360f / rotationSpeed;

            rotationTweenHandle = TweenManager.RotateEulerAngles(transform, new Vector3(0, 360, 0), duration, -1);
            loopSequence = new TweenSequence(LSequence.Create());

            for (int i = 0; i < rippleCount; i++)
            {
                SpriteRenderer sr = ripples[i];
                var fadeTween = TweenManager.Float(0f, 1f, rippleFadeInDuration, value =>
                 {
                     var c = sr.color;
                     c.a = value;
                     sr.color = c;
                 }, rippleEase);
                var scaleTween = TweenManager.Scale(sr.transform.localScale, Vector3.zero, sr.transform, rippleShrinkDuration, rippleEase);
                loopSequence.Insert(i * rippleInterval, fadeTween.Handle);
                loopSequence.Insert(i * rippleInterval, scaleTween.Handle);
            }
            var delayStartRipples = TweenManager.RunCallback(() =>
              {
                  KillTween();
                  AnimateRipples();
              });

            float time = (rippleInterval * rippleCount) + rippleShrinkDuration + delayBetweenLoops;
            loopSequence.Insert(time, delayStartRipples.Handle);
            loopSequence.Play();
        }

        public void StopRippleEffect()
        {
            KillTween();
            gameObject.SetActive(false);
        }

        private void KillTween()
        {
            // Kill previous sequence if it's still alive
            loopSequence.Cancel();
            rotationTweenHandle.Cancel();

            // Reset all ripples to their initial state
            if (ripples != null)
            {
                for (int i = 0; i < rippleCount; i++)
                {
                    SpriteRenderer sr = ripples[i];
                    sr.transform.localScale = Vector3.one * startScale;
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f); // Start transparent
                }
            }
        }
    }
}
