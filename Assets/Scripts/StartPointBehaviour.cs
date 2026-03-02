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
        [SerializeField] private float fadeValue = 0.5f;
        [SerializeField] private Ease easeType = Ease.OutCubic;

        private TweenHandle scaleHandle;
        private TweenHandle fadeHandle;

        public void Init()
        {
            AnimateRipple();
        }

        public void StopRippleAnimation()
        {
            fadeHandle.Cancel();
            scaleHandle.Cancel();
            rippleRenderer.color = new Color(1, 1, 1, 0); // Reset to transparent
            rippleRenderer.transform.localScale = Vector3.one * minScale; // Reset scale
        }

        private void AnimateRipple()
        {
            StopRippleAnimation();

            rippleRenderer.color = new Color(1, 1, 1, 1);

            scaleHandle = TweenManager.Scale(rippleRenderer.transform.localScale, maxScale * Vector3.one,
                rippleRenderer.transform, duration, easeType, () =>
                {
                    AnimateRipple(); // Loop
                });

            fadeHandle = TweenManager.Float(1f, fadeValue, duration, (val) =>
            {
                Color c = rippleRenderer.color;
                c.a = val;
                rippleRenderer.color = c;
            });
        }
    }
}
