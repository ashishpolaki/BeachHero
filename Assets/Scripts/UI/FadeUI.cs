using TransitionsPlus;
using UnityEngine;
using System.Threading.Tasks;
using LitMotion;

namespace BeachHero
{
    [System.Serializable]
    public class FadeUI
    {
        [SerializeField] private TransitionAnimator fadeAnimator;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private Ease fadeInEase = Ease.InQuad;
        [SerializeField] private Ease fadeOutEase = Ease.InBack;

        private TweenHandle fadeTween;

        #region Public Methods
        public void FadeIn() => StartFade(1f, fadeInDuration);
        public void FadeOut() => StartFade(0f, fadeOutDuration);
        public Task FadeInASync(float delay = 0) => StartFadeAsync(1f, fadeInDuration,delay);
        public Task FadeOutASync(float delay = 0.5f) => StartFadeAsync(0f, fadeOutDuration, delay);
        #endregion 

        #region Private Methods
        private void StartFade(float endValue, float duration)
        {
            fadeTween.Cancel();

            bool isFadeIn = endValue == 1f ? true : false;
            fadeAnimator.profile.duration = duration;
            Ease ease = isFadeIn ? fadeInEase : fadeOutEase;
            float getter = isFadeIn ? 0f : 1f;
            fadeAnimator.ResetTransition(getter);
            TweenManager.SetFloat(getter, endValue, duration, x => fadeAnimator.SetProgress(x), ease);
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
