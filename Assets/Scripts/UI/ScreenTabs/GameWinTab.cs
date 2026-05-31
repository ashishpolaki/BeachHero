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
        [SerializeField] private TextMeshProUGUI gameCurrencyBalanceText;
        [SerializeField] private TextMeshProUGUI collectedGameCurrencyText;
        [SerializeField] private Sprite unEarnedStarSprite;
        [SerializeField] private Sprite earnedStarSprite;
        [SerializeField] private Image[] starImages;

        private int collectedGameCurrency = 0;
        private int adWatchGameCurrency = 0;

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
            nextLevelButton.ButtonDeRegisterAll();
            multiplyGameCurrencyButton.ButtonDeRegisterAll();
            homeButton.ButtonDeRegisterAll();
        }
        private void SetGameCurrency()
        {
            collectedGameCurrency = GameController.GetInstance.LevelController.GameCurrencyCount;
            collectedGameCurrencyText.text = $"You Earned {collectedGameCurrency}";
            SetMedals();
            SetADGameCurrency();
            SetGameCurrencyBalance(collectedGameCurrency);
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
        private void SetADGameCurrency()
        {
            if (collectedGameCurrency > 0)
            {
                adWatchGameCurrency = collectedGameCurrency * IntUtils.MULTIPLIER_GAME_CURRENCY_REWARD;
            }
            else
            {
                adWatchGameCurrency = IntUtils.BASE_GAME_CURRENCY_REWARD;
            }
            //Animate game currency balance object

        }
        private void SetGameCurrencyBalance(int gameCurrency)
        {
            GameController.GetInstance.StoreController.IncrementGameCurrencyBalance(gameCurrency);
            gameCurrencyBalanceText.text = GameController.GetInstance.StoreController.GameCurrencyBalance.ToString();
        }
        private async void OnHomeASync()
        {
            await UIController.GetInstance.FadeUI.FadeInASync();
            GameController.GetInstance.BackToMainMenu();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            UIController.GetInstance.FadeUI.FadeOut();
        }
        private void OnNextLevel()
        {
            bool rateUsShown = SaveSystem.LoadBool(StringUtils.RATE_US_SHOWN, false);
            if (!rateUsShown)
            {
                if (GameController.GetInstance.CurrentLevelIndex + 1 > RemoteConfig.GetInstance.RateUsShowLevel)
                {
                    UIController.GetInstance.ScreenEvent(ScreenType.RateUs, UIScreenEvent.Push);
                    return;
                }
            }
            if (AdController.GetInstance.ShouldShowInterstitial())
            {
                AdController.GetInstance.ShowInterstitialAd(ContinueToNextLevel);
            }
            else
            {
                ContinueToNextLevel();
            }
        }
        private async void ContinueToNextLevel()
        {
            await UIController.GetInstance.FadeUI.FadeInASync();
            GameController.GetInstance.NextLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
            await UIController.GetInstance.FadeUI.FadeOutASync();
            MapController.GetInstance.AnimateToLevel();
            GameController.GetInstance.SetGameState(GameState.Map);
        }
        private void OnWatchAd()
        {
            // Get more Game Currency by watching AD.
            AdController.GetInstance.ShowRewardedAd((reward) =>
            {
                SetGameCurrencyBalance(adWatchGameCurrency);
            });
        }
    }
}
