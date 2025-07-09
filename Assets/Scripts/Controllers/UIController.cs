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
    }
    public class UIController : SingleTon<UIController>
    {
        #region Inspector Variables
        [SerializeField] private UIScreenManager screenManager;

        [Header("Fade")]
        [SerializeField] private Image fadePanel;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            screenManager.Initialize();
        }
        #endregion

        #region Public Methods
        public void ScreenEvent(ScreenType screenType, UIScreenEvent uIScreenEvent, ScreenTabType screenTabType = ScreenTabType.None)
        {
            screenManager.ScreenEvent(screenType, uIScreenEvent, screenTabType);
        }
        public void Close()
        {
            screenManager.CloseAllScreens();
        }
        public void FadeIn()
        {
            if (fadePanel != null)
            {
                fadePanel.DOFade(1f, fadeInDuration).SetEase(Ease.InOutSine); 
            }
        }
        public async Task FadeInASync()
        {
            if (fadePanel != null)
            {
                fadePanel.DOKill();
                await fadePanel.DOFade(1f, fadeInDuration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
            }
        }
        public void FadeOut()
        {
            if (fadePanel != null)
            {
                fadePanel.DOKill();
                fadePanel.DOFade(0f, fadeOutDuration).SetEase(Ease.InOutSine);
            }
        }
        public async Task FadeOutASync()
        {
            if (fadePanel != null)
            {
                fadePanel.DOKill();
                await fadePanel.DOFade(0f, fadeOutDuration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
            }
        }
        #endregion
    }
}