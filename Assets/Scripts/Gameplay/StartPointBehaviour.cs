using UnityEngine;
using LitMotion;

namespace BeachHero
{
    public class StartPointBehaviour : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer rippleRenderer;
        [SerializeField] private float maxScale = 1f;
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float duration = 1.5f;
        [SerializeField] private Ease easeType = Ease.OutCubic;

        private readonly TweenHandle[] scaleHandles = new TweenHandle[2];
        private readonly TweenHandle[] fadeHandles = new TweenHandle[2];
        private SpriteRenderer[] ripples;
        private TweenHandle delayedStartHandle;
        private bool isPlaying;
        private float cycleDuration;

        public void Init()
        {
            StopRippleAnimation();
            if (rippleRenderer == null || !isActiveAndEnabled)
                return;

            if (ripples == null)
            {
                var secondRipple = Instantiate(rippleRenderer, rippleRenderer.transform.parent);
                secondRipple.name = rippleRenderer.name + " Secondary Ripple";
                ripples = new[] { rippleRenderer, secondRipple };
            }

            cycleDuration = Mathf.Max(0.01f, duration);
            isPlaying = true;
            AnimateRipple(0);
            delayedStartHandle = TweenManager.RunCallback(() => AnimateRipple(1), cycleDuration * 0.5f);
        }

        public void StopRippleAnimation()
        {
            isPlaying = false;
            delayedStartHandle.Cancel();
            for (int i = 0; i < scaleHandles.Length; i++)
            {
                fadeHandles[i].Cancel();
                scaleHandles[i].Cancel();
                ResetRipple(ripples != null ? ripples[i] : (i == 0 ? rippleRenderer : null));
            }
        }

        private void AnimateRipple(int index)
        {
            if (!isPlaying || !isActiveAndEnabled || ripples[index] == null)
                return;

            var renderer = ripples[index];
            scaleHandles[index].Cancel();
            renderer.transform.localScale = Vector3.one * minScale;
            Color color = renderer.color;
            color.a = 1f;
            renderer.color = color;

            scaleHandles[index] = TweenManager.Scale(renderer.transform.localScale, maxScale * Vector3.one,
                renderer.transform, cycleDuration, easeType);

            // Restart only after the fade completes, so the previous fade cannot overwrite the new ring.
            fadeHandles[index] = TweenManager.SetFloat(1f, 0f, cycleDuration, val =>
            {
                Color c = renderer.color;
                c.a = val;
                renderer.color = c;
            }, onComplete: () => AnimateRipple(index));
        }

        private void ResetRipple(SpriteRenderer renderer)
        {
            if (renderer == null)
                return;

            Color color = renderer.color;
            color.a = 0f;
            renderer.color = color;
            renderer.transform.localScale = Vector3.one * minScale;
        }

        private void OnDisable()
        {
            StopRippleAnimation();
        }

        private void OnDestroy()
        {
            StopRippleAnimation();
        }
    }
}
