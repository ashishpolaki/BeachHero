using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameLoseTab : BaseScreenTab
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private Button skipLevelButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private GameObject gameCurrencyBalanceObject;
        [SerializeField] private TextMeshProUGUI gameCurrencyBalanceText;
        [SerializeField] private Sprite unEarnedStarSprite;
        [SerializeField] private Sprite earnedStarSprite;
        [SerializeField] private Image[] starImages;

        public override void Open()
        {
            base.Open();
            SetGameCurrency();
            SetMedals();
            retryButton.onClick.AddListener(OnRetryClick);
            skipLevelButton.onClick.AddListener(OnSkipLevelClick);
            homeButton.ButtonRegister(OnHomeASync);
        }
        public override void Close()
        {
            base.Close();
            retryButton.onClick.RemoveListener(OnRetryClick);
            skipLevelButton.onClick.RemoveListener(OnSkipLevelClick);
            homeButton.ButtonDeRegisterAll();
        }
        private void SetMedals()
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
            GameController.GetInstance.StoreController.IncrementGameCurrencyBalance(collectedGameCurrency);
            gameCurrencyBalanceText.text = GameController.GetInstance.StoreController.GameCurrencyBalance.ToString();
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
            await UIController.GetInstance.FadeUI.FadeInASync();
            GameController.GetInstance.SkipLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
            await UIController.GetInstance.FadeUI.FadeOutASync();
            MapController.GetInstance.AnimateToLevel();
            GameController.GetInstance.SetGameState(GameState.Map);
        }

        private async void OnHomeASync()
        {
            await UIController.GetInstance.FadeUI.FadeInASync();
            GameController.GetInstance.BackToMainMenu();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            await UIController.GetInstance.FadeUI.FadeOutASync();
        }
        private void OnRetryClick()
        {
            GameController.GetInstance.RetryLevel();
            GameController.GetInstance.StartGameplay();
            GameController.GetInstance.LevelController.PlaySpawnAnimations();
        }
    }
}
