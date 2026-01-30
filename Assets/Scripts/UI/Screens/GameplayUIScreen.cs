using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace BeachHero
{
    public class GameplayUIScreen : BaseScreen
    {
        [SerializeField] private UIButton pauseButton;
        [SerializeField] private UIButton retryButton;
        [SerializeField] private UIButton boatCustomisationBtn;
        [SerializeField] private UIButton shopBtn;
        [SerializeField] private UIButton noAdsBtn;
        [SerializeField] private PowerupUIButton magnetPowerupButton;
        [SerializeField] private PowerupUIButton speedBoostPowerupButton;

        [SerializeField] private RectTransform powerupsContainer;
        [SerializeField] private RectTransform boatCustomisationContainer;
        [SerializeField] private RectTransform shopContainer;
        [SerializeField] private RectTransform noAdsContainer;

        [SerializeField] private float containerMoveDuration = 0.5f;
        [SerializeField] private Ease containerMoveEase = Ease.OutBack;
        [SerializeField] private float containerMoveOffset = 200;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            ResetContainersPosition();
            pauseButton.OnButtonReleased += OnPause;
            retryButton.OnButtonReleased += OnRetry;
            boatCustomisationBtn.OnButtonReleased += OnBoatCustomize;
            shopBtn.OnButtonReleased += OnShop;
            noAdsBtn.OnButtonReleased += OnNoAds;
            //Powerups
            magnetPowerupButton.Init(PowerupType.Magnet, 3);
            speedBoostPowerupButton.Init(PowerupType.SpeedBoost, 3);
            GameController.GetInstance.LevelController.OnPlayerTouch += OnPlayerTouch;
        }

        private void ResetContainersPosition()
        {
            powerupsContainer.anchoredPosition = new Vector2(0, powerupsContainer.anchoredPosition.y);
            boatCustomisationContainer.anchoredPosition = new Vector2(0, boatCustomisationContainer.anchoredPosition.y);
            shopContainer.anchoredPosition = new Vector2(0, shopContainer.anchoredPosition.y);
            noAdsContainer.anchoredPosition = new Vector2(0, noAdsContainer.anchoredPosition.y);
        }

        private void OnPlayerTouch()
        {
            LMotion.Create(0, -containerMoveOffset, containerMoveDuration)
                .WithEase(containerMoveEase).BindToAnchoredPositionX(powerupsContainer);

            LMotion.Create(0, containerMoveOffset, containerMoveDuration)
                .WithEase(containerMoveEase).BindToAnchoredPositionX(boatCustomisationContainer);

            LMotion.Create(0, containerMoveOffset, containerMoveDuration)
                .WithEase(containerMoveEase).BindToAnchoredPositionX(shopContainer);

            LMotion.Create(0, containerMoveOffset, containerMoveDuration)
                .WithEase(containerMoveEase).BindToAnchoredPositionX(noAdsContainer);
        }

        public override void Close()
        {
            base.Close();
            pauseButton.OnButtonReleased -= OnPause;
            retryButton.OnButtonReleased -= OnRetry;
            boatCustomisationBtn.OnButtonReleased -= OnBoatCustomize;
            shopBtn.OnButtonReleased -= OnShop;
            noAdsBtn.OnButtonReleased -= OnNoAds;
            //Powerups
            magnetPowerupButton.DeInitialize();
            speedBoostPowerupButton.DeInitialize();
            GameController.GetInstance.LevelController.OnPlayerTouch -= OnPlayerTouch;
        }

        private void OnBoatCustomize()
        {
            GameController.GetInstance.SetGameState(GameState.Paused);
            UIController.GetInstance.ScreenEvent(ScreenType.BoatCustomisation, UIScreenEvent.Push);
        }

        private void OnShop()
        {
            GameController.GetInstance.SetGameState(GameState.Paused);
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Push);
        }

        private void OnNoAds()
        {
            GameController.GetInstance.SetGameState(GameState.Paused);
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Push);
        }

        private void OnPause()
        {
            GameController.GetInstance.SetGameState(GameState.Paused);
            OpenTab(ScreenTabType.GamePause);
        }
        private async void OnRetry()
        {
            GameController.GetInstance.SetGameState(GameState.Paused);
            await UIController.GetInstance.FadeUI.FadeInASync();
            GameController.GetInstance.RetryLevel();
            GameController.GetInstance.Play();
            await UIController.GetInstance.FadeUI.FadeOutASync();
        }
    }
}
