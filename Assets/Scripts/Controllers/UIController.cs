using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public enum UIScreenEvent
    {
        Open,           // Close current screen and open a new one
        Close,          // Close the current screen
        Show,           // Show the screen without affecting others (e.g., reappear)
        Hide,           // Hide the screen without destroying it
        Push,           // Open a new screen while keeping the current one active (stack-based UI)
        ChangeTab   // Change the active tab within the current screen
    }
    public class UIController : SingleTon<UIController>
    {
        #region Inspector Variables
        [SerializeField] private UIScreenManager screenManager;

        [Header("Fade")]
        [SerializeField] private Image fadePanel;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("Notch SafeArea")]
        [SerializeField] private NotchSafeArea notchSafeArea;

        [Header("Loading")]
        [SerializeField] private Image loadingPanel;
        #endregion

        public NotchSafeArea NotchSafeArea => notchSafeArea;

        #region Public Methods
        public void ScreenEvent(ScreenType screenType, UIScreenEvent uIScreenEvent, ScreenTabType screenTabType = ScreenTabType.None)
        {
            screenManager.ScreenEvent(screenType, uIScreenEvent, screenTabType);
        }
        public void Close()
        {
            screenManager.CloseAll();
        }
        public void FadeIn() => StartFade(1f, fadeInDuration);
        public void FadeOut() => StartFade(0f, fadeOutDuration);
        public Task FadeInASync() => StartFadeAsync(1f, fadeInDuration);
        public Task FadeOutASync() => StartFadeAsync(0f, fadeOutDuration);
        #endregion

        #region Private Methods
        private void StartFade(float endValue, float duration)
        {
            if (fadePanel != null)
            {
                fadePanel.DOKill();
                fadePanel.DOFade(endValue, duration).SetEase(Ease.InOutSine);
            }
        }
        private async Task StartFadeAsync(float endValue, float duration)
        {
            if (fadePanel != null)
            {
                fadePanel.DOKill();
                await fadePanel.DOFade(endValue, duration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
            }
        }
        #endregion
    }
}