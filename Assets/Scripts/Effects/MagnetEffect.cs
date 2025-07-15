using DG.Tweening;
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
        [SerializeField] private Ease rippleEase = Ease.InOutQuad;
        [SerializeField] private float rotationSpeed = 90f; // degrees per second

        private SpriteRenderer[] ripples;
        private Sequence loopSeq;

        public bool IsPlaying => loopSeq != null && loopSeq.IsActive();

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

        public void StopRippleEffect()
        {
            KillTween();
        }

        private void KillTween()
        {
            // Kill previous sequence if it's still alive
            if (loopSeq != null && loopSeq.IsActive())
                loopSeq.Kill();

            // Reset all ripples to their initial state
            for (int i = 0; i < rippleCount; i++)
            {
                SpriteRenderer sr = ripples[i];
                sr.DOKill(); // Ensure each ripple is reset before starting the animation
                sr.transform.localScale = Vector3.one * startScale;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f); // Start transparent
            }

            transform.localRotation = Quaternion.identity; // Reset rotation
            transform.DOKill(); // Reset any previous rotation animations
        }

        void AnimateRipples()
        {
            KillTween();
            float duration = 360f / rotationSpeed;

            transform.DORotate(new Vector3(0, 360, 0), duration, RotateMode.WorldAxisAdd)
                     .SetEase(Ease.Linear)
                     .SetLoops(-1);

            loopSeq = DOTween.Sequence();

            for (int i = 0; i < rippleCount; i++)
            {
                SpriteRenderer sr = ripples[i];

                loopSeq.Insert(i * rippleInterval,
                    DOTween.Sequence().AppendCallback(() =>
                    {
                        sr.DOFade(1f, rippleFadeInDuration);
                    }).Append(sr.transform.DOScale(0f, rippleShrinkDuration).SetEase(rippleEase))
                );

                // Only attach restart to last ripple
                if (i == rippleCount - 1)
                {
                    loopSeq.OnComplete(() =>
                    {
                        DOVirtual.DelayedCall(delayBetweenLoops, AnimateRipples);
                    });
                }
            }
        }
    }
}
