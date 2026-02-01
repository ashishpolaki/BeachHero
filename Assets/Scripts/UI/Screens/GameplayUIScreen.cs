using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace BeachHero
{
    public class GameplayUIScreen : BaseScreen
    {
        [Header("Buttons")]
        [SerializeField] private UIButton pauseButton;
        [SerializeField] private UIButton retryButton;
        [SerializeField] private UIButton boatCustomisationBtn;
        [SerializeField] private UIButton shopBtn;
        [SerializeField] private UIButton noAdsBtn;

        [Header("Powerup Buttons")]
        [SerializeField] private PowerupUIButton magnetPowerupButton;
        [SerializeField] private PowerupUIButton speedBoostPowerupButton;

        [Header("UI Containers")]
        [SerializeField] private RectTransform powerupsContainer;
        [SerializeField] private RectTransform boatCustomisationContainer;
        [SerializeField] private RectTransform shopContainer;
        [SerializeField] private RectTransform noAdsContainer;

        [Header("Containers Animation Settings")]
        [SerializeField] private float containerMoveDuration = 0.5f;
        [SerializeField] private float containerMoveOffset = 200;
        [SerializeField] private float containerMoveOpenDelay = 1f;
        [SerializeField] private Ease containerMoveEase = Ease.OutBack;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            SetContainersInitialPosition();
            PlayContainersAnimation();

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

        // Set the containers outside the screen initially
        private void SetContainersInitialPosition()
        {
            powerupsContainer.anchoredPosition = new Vector2(-containerMoveOffset, powerupsContainer.anchoredPosition.y);
            boatCustomisationContainer.anchoredPosition = new Vector2(containerMoveOffset, boatCustomisationContainer.anchoredPosition.y);
            shopContainer.anchoredPosition = new Vector2(containerMoveOffset, shopContainer.anchoredPosition.y);
            noAdsContainer.anchoredPosition = new Vector2(containerMoveOffset, noAdsContainer.anchoredPosition.y);
        }

        private void PlayContainersAnimation()
        {
            LMotion.Create(-containerMoveOffset, 0, containerMoveDuration)
                .WithEase(containerMoveEase).WithDelay(containerMoveOpenDelay).BindToAnchoredPositionX(powerupsContainer);
            LMotion.Create(containerMoveOffset, 0, containerMoveDuration)
                .WithEase(containerMoveEase).WithDelay(containerMoveOpenDelay).BindToAnchoredPositionX(boatCustomisationContainer);
            LMotion.Create(containerMoveOffset, 0, containerMoveDuration)
                .WithEase(containerMoveEase).WithDelay(containerMoveOpenDelay).BindToAnchoredPositionX(shopContainer);
            LMotion.Create(containerMoveOffset, 0, containerMoveDuration)
                .WithEase(containerMoveEase).WithDelay(containerMoveOpenDelay).BindToAnchoredPositionX(noAdsContainer);
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
