using UnityEngine;

namespace BeachHero
{
    public class StaticObstacle : Obstacle
    {
        [Header("Explosion Settings")]
        [SerializeField] private Vector3 explodeScale = new Vector3(1.2f, 1.2f, 1.2f);
        [SerializeField] private float explodeScaleUpDuration = 0.1f;
        [SerializeField] private Vector3 shakeMagnitude = new Vector3(0.1f, 0.1f, 0.1f);
        [SerializeField] private int shakeFrequency = 7;
        [SerializeField] private float shakeDuration = 0.15f;
        [SerializeField] private float explodeScaleDownDuration = 0.2f;

        private TweenHandle scaleTween;
        private TweenHandle shakeTween;

        public virtual void Init(Vector3 position)
        {
            transform.position = position;
        }
        public override void Hit()
        {
            base.Hit();
        }
        public override void HitByDash(Vector3 dir)
        {
            base.HitByDash();
            scaleTween = TweenManager.Scale(transform.localScale, explodeScale, transform, explodeScaleUpDuration);
            shakeTween = TweenManager.ShakePosition(transform, transform.position, shakeMagnitude, shakeFrequency, shakeDuration,
                onComplete: () =>
                {
                    scaleTween.Cancel();
                    scaleTween = TweenManager.Scale(explodeScale, Vector3.zero, transform, explodeScaleDownDuration);
                });
        }
        public override void ResetObstacle()
        {
            base.ResetObstacle();
            transform.localScale = Vector3.one;
            scaleTween.Cancel();
            shakeTween.Cancel();
        }
    }
}
