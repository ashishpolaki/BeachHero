using DG.Tweening;
using UnityEngine;

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

        public void Init()
        {
            AnimateRipple();
        }

        public void Stop()
        {
            rippleRenderer.DOKill(); // Stop any ongoing animations
            rippleRenderer.color = new Color(1, 1, 1, 0); // Reset to transparent
            rippleRenderer.transform.localScale = Vector3.one * minScale; // Reset scale
        }

        void AnimateRipple()
        {
            rippleRenderer.DOKill(); // Stop any ongoing animations

            rippleRenderer.color = new Color(1, 1, 1, 1);
            rippleRenderer.transform.localScale = Vector3.one * minScale;

            rippleRenderer.transform.DOScale(maxScale, duration).SetEase(easeType);
            rippleRenderer.DOFade(fadeValue, duration).OnComplete(() =>
            {
                AnimateRipple(); // Loop
            });
        }
    }
}
