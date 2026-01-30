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

        public override void Open()
        {
            base.Open();
            SetGameCurrency();
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
            MapController.GetInstance.CheckForMapUpdate();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
            MapController.GetInstance.PlaceBoatAtPreviousLevel();
            await UIController.GetInstance.FadeUI.FadeOutASync();
            MapController.GetInstance.AnimateBoatToCurrentLevel();
            GameController.GetInstance.SetGameState(GameState.Map);
        }

        private async void OnHomeASync()
        {
            await UIController.GetInstance.FadeUI.FadeInASync();
            GameController.GetInstance.RetryLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            await UIController.GetInstance.FadeUI.FadeOutASync();
        }
        private async void OnRetryClick()
        {
            await UIController.GetInstance.FadeUI.FadeInASync();
            GameController.GetInstance.RetryLevel();
            GameController.GetInstance.Play();
            await UIController.GetInstance.FadeUI.FadeOutASync();
        }
    }
}
