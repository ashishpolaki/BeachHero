using TransitionsPlus;
using UnityEngine;
using System.Threading.Tasks;

namespace BeachHero
{
    [System.Serializable]
    public class FadeUI
    {
        [SerializeField] private TransitionAnimator fadeAnimator;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        #region Public Methods
        public void FadeIn() => StartFade(1f, fadeInDuration);
        public void FadeOut() => StartFade(0f, fadeOutDuration);
        public Task FadeInASync(float delay = 0) => StartFadeAsync(1f, fadeInDuration,delay);
        public Task FadeOutASync(float delay = 0.2f) => StartFadeAsync(0f, fadeOutDuration, delay);
        #endregion

        #region Private Methods
        private void StartFade(float endValue, float duration)
        {
            //if (fadePanel != null)
            //{
            //    fadePanel.DOKill();
            //    fadePanel.DOFade(endValue, duration).SetEase(Ease.InOutSine);
            //}
            fadeAnimator.profile.invert = endValue == 0 ? true : false;
            fadeAnimator.profile.duration = duration;
            fadeAnimator.Play();
        }
        private async Task StartFadeAsync(float endValue, float duration,float delay)
        {
            //if (fadePanel != null)
            //{
            //    fadePanel.DOKill();
            //    await fadePanel.DOFade(endValue, duration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
            //}
            fadeAnimator.profile.invert = endValue == 0 ? true : false;
            fadeAnimator.profile.duration = duration;
            fadeAnimator.Play();
            await Task.Delay((int)(duration * 1000));
        }
        #endregion
    }
}
