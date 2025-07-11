using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameWinTab : BaseScreenTab
    {
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button multiplyGameCurrencyButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private GameObject gameCurrencyBalanceObject;
        [SerializeField] private TextMeshProUGUI gameCurrencyBalanceText;
        [SerializeField] private TextMeshProUGUI collectedGameCurrencyText;
        [SerializeField] private TextMeshProUGUI collectedGameCurrencyText2;

        private int collectedGameCurrency = 0;

        public override void Open()
        {
            base.Open();
            SetGameCurrency();
            nextLevelButton.ButtonRegister(OnNextLevel);
            multiplyGameCurrencyButton.ButtonRegister(OnWatchAd);
            homeButton.ButtonRegister(OnHomeASync);
        }
        public override void Close()
        {
            base.Close();
            nextLevelButton.ButtonDeRegister();
            multiplyGameCurrencyButton.ButtonDeRegister();
            homeButton.ButtonDeRegister();
            multiplyGameCurrencyButton.gameObject.SetActive(false);
        }
        private void SetGameCurrency()
        {
            collectedGameCurrency = GameController.GetInstance.LevelController.GameCurrencyCount;
            collectedGameCurrencyText.text = collectedGameCurrency.ToString();
            collectedGameCurrencyText2.text = collectedGameCurrency.ToString();
            if(collectedGameCurrency > 0)
            {
                multiplyGameCurrencyButton.gameObject.SetActive(true);
            }
            //Animate game currency balance object
            AddGameCurrency();
        }
        private void AddGameCurrency()
        {
            GameController.GetInstance.StoreController.IncrementGameCurrencyBalance(collectedGameCurrency);
            gameCurrencyBalanceText.text = GameController.GetInstance.StoreController.GameCurrencyBalance.ToString();
        }
        private async void OnHomeASync()
        {
            await UIController.GetInstance.FadeInASync();
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Close);
            GameController.GetInstance.RetryLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            UIController.GetInstance.FadeOut();
        }
        private async void OnNextLevel()
        {
            await UIController.GetInstance.FadeInASync();
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Close);
            GameController.GetInstance.NextLevel();
            await SceneLoader.GetInstance.LoadScene(StringUtils.MAP_SCENE, 0);
            GameController.GetInstance.CameraController.DisableCameras();
            await UIController.GetInstance.FadeOutASync();
            MapController.GetInstance.MoveBoatFromPrevToCurrentLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
        }
        private void OnWatchAd()
        {
            // Get more Game Currency by watching AD.
            AdController.GetInstance.ShowRewardedAd((reward) =>
            {
                AddGameCurrency();
            });
        }
    }
}
