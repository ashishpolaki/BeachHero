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
            homeButton.ButtonDeRegister();
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
            await UIController.GetInstance.FadeInASync();
            GameController.GetInstance.SkipLevel();
            await SceneLoader.GetInstance.LoadScene(StringUtils.MAP_SCENE, 0);
            GameController.GetInstance.CameraController.DisableCameras();
            await UIController.GetInstance.FadeOutASync();
            MapController.GetInstance.MoveBoatFromPrevToCurrentLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
        }
        private async void OnHomeASync()
        {
            await UIController.GetInstance.FadeInASync();
            GameController.GetInstance.RetryLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            await UIController.GetInstance.FadeOutASync();
        }
        private void OnRetryClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Push);
            //   Close();
            //  GameController.GetInstance.RetryLevel();
        }
    }
}
