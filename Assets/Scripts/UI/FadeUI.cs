using TransitionsPlus;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;

namespace BeachHero
{
    [System.Serializable]
    public class FadeUI
    {
        [SerializeField] private TransitionAnimator fadeAnimator;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private Ease fadeInEase = Ease.InOutSine;
        [SerializeField] private Ease fadeOutEase = Ease.InOutSine;

        private Tween fadeTween;

        #region Public Methods
        public void FadeIn() => StartFade(1f, fadeInDuration);
        public void FadeOut() => StartFade(0f, fadeOutDuration);
        public Task FadeInASync(float delay = 0) => StartFadeAsync(1f, fadeInDuration,delay);
        public Task FadeOutASync(float delay = 0.5f) => StartFadeAsync(0f, fadeOutDuration, delay);
        #endregion 

        #region Private Methods
        private void StartFade(float endValue, float duration)
        {
            if (fadeTween != null)
            {
                fadeTween.Kill();
                fadeTween = null;
            }

            bool isFadeIn = endValue == 1f ? true : false;
            fadeAnimator.profile.duration = duration;
            Ease ease = isFadeIn ? fadeInEase : fadeOutEase;
            float getter = isFadeIn ? 0f : 1f;
            fadeAnimator.ResetTransition(getter);
            DOTween.To(() => getter, x => fadeAnimator.SetProgress(x), endValue, duration).SetEase(ease);
        }
        private async Task StartFadeAsync(float endValue, float duration,float delay)
        {
            if(delay > 0)
            {
                await Task.Delay((int)(delay * 1000));
            }
            StartFade(endValue, duration);
            await Task.Delay((int)(duration * 1000));
        }
        #endregion
    }
}
