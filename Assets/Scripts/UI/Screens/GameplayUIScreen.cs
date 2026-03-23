using LitMotion;
using UnityEngine;

namespace BeachHero
{
    public class GameplayUIScreen : BaseScreen
    {
        #region Inspector Variables
        [Header("Buttons")]
        [SerializeField] private UIButton pauseButton;
        [SerializeField] private UIButton retryButton;
        [SerializeField] private UIButton boatCustomisationBtn;
        [SerializeField] private UIButton shopBtn;
        [SerializeField] private UIButton noAdsBtn;

        [Header("Powerup Buttons")]
        [SerializeField] private PowerupUIButton magnetPowerupButton;
        [SerializeField] private PowerupUIButton speedBoostPowerupButton;

        [Header("UI Panels")]
        [SerializeField] private RectTransform powerupPanel;
        [SerializeField] private RectTransform boatPanel;
        [SerializeField] private RectTransform shopPanel;
        [SerializeField] private RectTransform noAdsPanel;

        [Header("Panel Animation Settings")]
        [SerializeField] private float panelSlideDuration = 0.5f;
        [SerializeField] private float panelSlideOffset = 200f;
        [SerializeField] private Ease panelSlideEase = Ease.OutBack;
        #endregion

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            SetPanelsToHiddenPosition();

            pauseButton.OnButtonReleased += OnPause;
            retryButton.OnButtonReleased += OnRetry;
            boatCustomisationBtn.OnButtonReleased += OnBoatCustomize;
            shopBtn.OnButtonReleased += OnShop;
            noAdsBtn.OnButtonReleased += OnNoAds;
            //Powerups
            magnetPowerupButton.Init(PowerupType.Magnet, 3);
            speedBoostPowerupButton.Init(PowerupType.SpeedBoost, 3);
            GameController.GetInstance.LevelController.OnPlayerTouch += HandleHidePanels;
            GameController.GetInstance.LevelController.OnDrawPathError += HandleShowPanels;
            GameController.GetInstance.LevelController.OnCompleteSpawnAnimation += HandleShowPanels;
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
            GameController.GetInstance.LevelController.OnPlayerTouch -= HandleHidePanels;
            GameController.GetInstance.LevelController.OnDrawPathError -= HandleShowPanels;
            GameController.GetInstance.LevelController.OnCompleteSpawnAnimation -= HandleShowPanels;
        }

        #region Containers Animation
        private void SetPanelsToHiddenPosition()
        {
            powerupPanel.anchoredPosition = new Vector2(-panelSlideOffset, powerupPanel.anchoredPosition.y);
            boatPanel.anchoredPosition = new Vector2(panelSlideOffset, boatPanel.anchoredPosition.y);
            shopPanel.anchoredPosition = new Vector2(panelSlideOffset, shopPanel.anchoredPosition.y);
            noAdsPanel.anchoredPosition = new Vector2(panelSlideOffset, noAdsPanel.anchoredPosition.y);
        }
        private void AnimatePanels(bool show)
        {
            float leftPanelFromX = show ? -panelSlideOffset : 0f;
            float leftPanelToX = show ? 0f : -panelSlideOffset;

            float rightPanelFromX = show ? panelSlideOffset : 0f;
            float rightPanelToX = show ? 0f : panelSlideOffset;

            TweenManager.MoveAnchorOnAxis(powerupPanel, leftPanelFromX, leftPanelToX, panelSlideDuration, panelSlideEase);
            TweenManager.MoveAnchorOnAxis(boatPanel, rightPanelFromX, rightPanelToX, panelSlideDuration, panelSlideEase);
            TweenManager.MoveAnchorOnAxis(shopPanel, rightPanelFromX, rightPanelToX, panelSlideDuration, panelSlideEase);
            TweenManager.MoveAnchorOnAxis(noAdsPanel, rightPanelFromX, rightPanelToX, panelSlideDuration, panelSlideEase);
        }

        private void HandleShowPanels()
        {
            AnimatePanels(true);
        }
        private void HandleHidePanels()
        {
            AnimatePanels(false);
        }
        #endregion

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
        private void OnRetry()
        {
            GameController.GetInstance.SetGameState(GameState.Paused);
            GameController.GetInstance.RetryLevel();
            GameController.GetInstance.StartGameplay();
            GameController.GetInstance.LevelController.PlaySpawnAnimations();
        }
    }
}
