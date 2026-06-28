using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameLoseTab : BaseScreenTab
    {
        [SerializeField] private UIButton retryButton;
        [SerializeField] private UIButton skipLevelButton;
        [SerializeField] private UIButton homeButton;
        [SerializeField] private GameObject gameCurrencyBalanceObject;
        [SerializeField] private TextMeshProUGUI gameCurrencyBalanceText;
        [SerializeField] private Sprite unEarnedStarSprite;
        [SerializeField] private Sprite earnedStarSprite;
        [SerializeField] private Image[] starImages;

        private TweenHandle watchAdTween;

        public override void Open()
        {
            base.Open();
            SetGameCurrency();
            SetStars();
            retryButton.OnButtonReleased += (OnRetryClick);
            skipLevelButton.OnButtonPressed += (OnWatchRewardedAd);
            skipLevelButton.OnButtonReleased += OnSkipLevelClick;
            homeButton.OnButtonReleased += (OnHomeASync);
            watchAdTween = TweenManager.PlayIdleLoopAnimation(skipLevelButton.transform);   
        }
        public override void Close()
        {
            base.Close();
            retryButton.OnButtonReleased -= (OnRetryClick);
            skipLevelButton.OnButtonPressed -= (OnWatchRewardedAd);
            skipLevelButton.OnButtonReleased -= (OnSkipLevelClick);
            homeButton.OnButtonReleased -= (OnHomeASync);
            watchAdTween.Cancel();
        }
        private void OnWatchRewardedAd()
        {
            watchAdTween.Cancel();
        }
        private void SetStars()
        {
            foreach (var starImage in starImages)
            {
                starImage.sprite = unEarnedStarSprite;
            }
            for (int i = 0; i < GameController.GetInstance.LevelController.MedalsEarned; i++)
            {
                starImages[i].sprite = earnedStarSprite;
            }
        }
        private void SetGameCurrency()
        {
            int collectedGameCurrency = GameController.GetInstance.LevelController.GameCurrencyCount;
            GameController.GetInstance.StoreController.IncrementCoinsBalance(collectedGameCurrency);
            gameCurrencyBalanceText.text = GameController.GetInstance.StoreController.CoinsBalance.ToString();
        }
        private void OnSkipLevelClick()
        {
            AdController.GetInstance.ShowRewardedAd((reward) =>
            {
                // Callback after ad is watched.
                OnSkipLevelASync();
            });
        }
        private async void OnSkipLevelASync()
        {
            await UIController.GetInstance.LoadingUI.ShowLoadingScreen();
            GameController.GetInstance.SkipLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
            await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
            MapController.GetInstance.AnimateToLevel();
            GameController.GetInstance.SetGameState(GameState.Map);
        }

        private async void OnHomeASync()
        {
            await UIController.GetInstance.LoadingUI.ShowLoadingScreen();
            GameController.GetInstance.BackToMainMenu();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
        }
        private void OnRetryClick()
        {
            GameController.GetInstance.RetryLevel();
            GameController.GetInstance.StartGameplay();
            GameController.GetInstance.LevelController.PlaySpawnAnimations();
        }
    }
}
