using LitMotion;
using UnityEngine;

namespace BeachHero
{
    public class GameplayUIScreen : BaseScreen
    {
        #region Inspector Variables
        [Header("References")]
        [SerializeField] private StarsPanelUI starsPanelUI;
        [SerializeField] private RectTransform pauseButtonRect;

        [SerializeField] private RectTransform retryButtonRect;
        [Header("Buttons")]
        [SerializeField] private UIButton pauseButton;
        [SerializeField] private UIButton retryButton;
        [SerializeField] private UIButton boatCustomisationBtn;
        [SerializeField] private UIButton shopBtn;
        [SerializeField] private UIButton noAdsBtn;

        [Header("Powerup Buttons")]
        [SerializeField] private PowerupUIButton shieldPowerupButton;
        [SerializeField] private PowerupUIButton speedBoostPowerupButton;

        [Header("UI Panels")]
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;

        [Header("Panel Animation Settings")]
        [SerializeField] private float panelSlideDuration = 0.5f;
        [SerializeField] private float panelSlideOffset = 200f;
        [SerializeField] private Ease panelSlideEase = Ease.OutBack;

        [Header("Top Buttons Animation Settings")]
        [SerializeField] private float topButtonsMoveOffset = 200f;
        [SerializeField] private float topButtonsMoveDuration = 0.3f;
        [SerializeField] private Ease topButtonsMoveEase = Ease.OutQuad;

        [Header("Tutorial Positions")]
        [SerializeField] private Vector3 tutorialCharacterPosition;
        [SerializeField] private Vector3 speechBubblePosition;
        #endregion

        #region Private Variables
        private float pauseInitialY;
        private float retryInitialY;
        private bool isTopButtonsCached;
        private bool isStarPanelPosCached = false;
        private Vector3 cachedStarPanelPos;
        #endregion

        #region Properties
        public Vector3 StarsPanelWorldPosition
        {
            get
            {
                if (!isStarPanelPosCached)
                {
                    isStarPanelPosCached = true;
                    var cameraPos = CameraController.GetInstance.GetCameraPosition(GameCameraType.GameView);
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(CameraController.GetInstance.GetMainCamera, starsPanelUI.StarPanel.position);
                    cachedStarPanelPos = CameraController.GetInstance.GetMainCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, cameraPos.y));
                }
                return cachedStarPanelPos;
            }
        }
        #endregion

        #region Interface Methods
        private void Awake()
        {
            if (!isTopButtonsCached)
            {
                isTopButtonsCached = true;
                pauseInitialY = pauseButtonRect.anchoredPosition.y;
                retryInitialY = retryButtonRect.anchoredPosition.y;
            }
        }
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            SetPanelsToHiddenPosition();
            TryShowTutorialHint();
            EnvironmentController.GetInstance.Initialize();
            starsPanelUI.Open();
            ResetTopButtons();

            //Powerups
            //  magnetPowerupButton.Init(PowerupType.Magnet, SaveSystem.LoadInt(StringUtils.MAGNET_BALANCE 3);
            speedBoostPowerupButton.Init(PowerupType.SpeedBoost);
            shieldPowerupButton.Init(PowerupType.Shield);

            //buttons
            pauseButton.OnButtonReleased += OnPause;
            retryButton.OnButtonReleased += OnRetry;
            boatCustomisationBtn.OnButtonReleased += OnBoatCustomize;
            shopBtn.OnButtonReleased += OnShop;
            noAdsBtn.OnButtonReleased += OnNoAds;

            // Events
            GameController.GetInstance.LevelController.OnPlayerTouch += HandleHidePanels;
            GameController.GetInstance.LevelController.OnDrawPathError += HandleShowPanels;
            GameController.GetInstance.LevelController.OnCompleteSpawnAnimation += HandleCompleteSPawnAnimation;
            GameController.GetInstance.LevelController.OnDrownCharactersCollected += AnimateTopButtons;
            GameController.GetInstance.PowerupController.OnBalanceChange += HandlePowerupBalance;
        }
        public override void Close()
        {
            base.Close();
            starsPanelUI.Close();
            HandleShowPanels();
            EnvironmentController.GetInstance.DeInitialize();

            //buttons
            pauseButton.OnButtonReleased -= OnPause;
            retryButton.OnButtonReleased -= OnRetry;
            boatCustomisationBtn.OnButtonReleased -= OnBoatCustomize;
            shopBtn.OnButtonReleased -= OnShop;
            noAdsBtn.OnButtonReleased -= OnNoAds;

            //Powerups
            // magnetPowerupButton.DeInitialize();
            speedBoostPowerupButton.DeInitialize();
            shieldPowerupButton.DeInitialize();

            //Events
            GameController.GetInstance.LevelController.OnPlayerTouch -= HandleHidePanels;
            GameController.GetInstance.LevelController.OnDrawPathError -= HandleShowPanels;
            GameController.GetInstance.LevelController.OnCompleteSpawnAnimation -= HandleCompleteSPawnAnimation;
            GameController.GetInstance.LevelController.OnDrownCharactersCollected -= AnimateTopButtons;
            GameController.GetInstance.PowerupController.OnBalanceChange -= HandlePowerupBalance;
        }
        #endregion

        #region Tutorial
        public void TryShowTutorialHint()
        {
            // If the player loses n in a row, show the hint with the speech bubble
            if (GameController.GetInstance.LevelController.ShouldShowConsecutiveLossHint())
            {
                GameController.GetInstance.LevelController.ResetLevelFailCounter();
                TutorialController.GetInstance.TutorialCharacter.PlayAnimation(TutorialCharacterState.Cry, tutorialCharacterPosition, () =>
                {
                    // choose hint based on whether powerups are unlocked
                    string hint = StringUtils.CONSECUTIVE_LOSE_HINT;
                    var pc = GameController.GetInstance.PowerupController;
                    if (pc != null && (pc.IsPowerupUnlocked(PowerupType.SpeedBoost) || pc.IsPowerupUnlocked(PowerupType.Shield)))
                    {
                        hint = StringUtils.CONSECUTIVE_LOSE_HINT_POWERUPS;
                    }
                    TutorialController.GetInstance.TutorialSpeechBubble.Show(hint, speechBubblePosition);
                });

                //Add a skip button.
                TutorialController.GetInstance.TutorialSkipOverlay.Show(() =>
                {
                    TutorialController.GetInstance.TutorialSpeechBubble.Hide();
                    TutorialController.GetInstance.TutorialCharacter.Hide();
                });
            }
        }
        #endregion

        #region Containers Animation
        private void HandleCompleteSPawnAnimation()
        {
            if (GameController.GetInstance.PowerupController.IsCurrentLevelUnlocksPowerup())
            {
                return;
            }
            HandleShowPanels();
        }
        private void SetPanelsToHiddenPosition()
        {
            // If the current level unlocks a powerup, we don't want to slide in the panels as it will reveal the new powerup.
            if (GameController.GetInstance.PowerupController.IsCurrentLevelUnlocksPowerup())
            {
                AdController.GetInstance.HideBanner();
                return;
            }
            leftPanel.anchoredPosition = new Vector2(-panelSlideOffset, leftPanel.anchoredPosition.y);
            rightPanel.anchoredPosition = new Vector2(panelSlideOffset, rightPanel.anchoredPosition.y);
        }
        private void AnimatePanels(bool show)
        {
            float leftPanelFromX = show ? -panelSlideOffset : 0f;
            float leftPanelToX = show ? 0f : -panelSlideOffset;

            float rightPanelFromX = show ? panelSlideOffset : 0f;
            float rightPanelToX = show ? 0f : panelSlideOffset;

            TweenManager.MoveAnchorOnAxis(leftPanel, leftPanelFromX, leftPanelToX, panelSlideDuration, panelSlideEase, TransformAxis.X);
            TweenManager.MoveAnchorOnAxis(rightPanel, rightPanelFromX, rightPanelToX, panelSlideDuration, panelSlideEase, TransformAxis.X);
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

        #region Top Button Animation
        private void ResetTopButtons()
        {
            pauseButton.enabled = true;
            retryButton.enabled = true;
            pauseButtonRect.anchoredPosition = new Vector2(pauseButtonRect.anchoredPosition.x, pauseInitialY);
            retryButtonRect.anchoredPosition = new Vector2(retryButtonRect.anchoredPosition.x, retryInitialY);
        }
        private void AnimateTopButtons()
        {
            pauseButton.enabled = false;
            retryButton.enabled = false;
            TweenManager.MoveAnchorOnAxis(pauseButtonRect, pauseButtonRect.anchoredPosition.y,
                pauseButtonRect.anchoredPosition.y + topButtonsMoveOffset,
           topButtonsMoveDuration, topButtonsMoveEase, TransformAxis.Y);
            TweenManager.MoveAnchorOnAxis(retryButtonRect, retryButtonRect.anchoredPosition.y,
               retryButtonRect.anchoredPosition.y + topButtonsMoveOffset,
          topButtonsMoveDuration, topButtonsMoveEase, TransformAxis.Y);
        }
        #endregion

        #region Handle Button Listener
        private void HandlePowerupBalance(PowerupType powerupType)
        {
            switch (powerupType)
            {
                case PowerupType.SpeedBoost:
                    speedBoostPowerupButton.UpdateBalance();
                    break;
                case PowerupType.Shield:
                    shieldPowerupButton.UpdateBalance();
                    break;
                default:
                    break;
            }
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
        private void OnRetry()
        {
            GameController.GetInstance.RetryLevel();
            GameController.GetInstance.StartGameplay();
            GameController.GetInstance.LevelController.PlaySpawnAnimations();
        }
        #endregion
    }
}
