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
        [SerializeField] private RectTransform starProgressBar;
        [SerializeField] private RectTransform powerupPanel;
        [SerializeField] private RectTransform boatPanel;
        [SerializeField] private RectTransform shopPanel;
        [SerializeField] private RectTransform noAdsPanel;

        [Header("Panel Animation Settings")]
        [SerializeField] private float panelSlideDuration = 0.5f;
        [SerializeField] private float panelSlideOffset = 200f;
        [SerializeField] private Ease panelSlideEase = Ease.OutBack;

        [Header("Tutorial Positions")]
        [SerializeField] private Vector3 tutorialCharacterPosition;
        [SerializeField] private Vector3 speechBubblePosition;

        //Star Bar
        [Header("Star Bar")]
        [SerializeField] private Vector3 starBarPunchScale = new Vector3(0.3f, 0.3f, 0.3f);
        [SerializeField] private float starBarPunchDuration = 0.3f;
        [SerializeField] private Ease starBarPunchEase = Ease.OutBounce;
        private bool isStarBarWorldPosCached = false;
        private Vector3 cachedStarBarWorldPos;
        public Vector3 StarBarWorldPosition
        {
            get
            {
                if (!isStarBarWorldPosCached)
                {
                    isStarBarWorldPosCached = true;
                    var cameraPos = CameraController.GetInstance.GetCameraPosition(GameCameraType.GameView);
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(CameraController.GetInstance.GetMainCamera, starProgressBar.position);
                    cachedStarBarWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, cameraPos.y));
                }
                return cachedStarBarWorldPos;
            }
        }
        private TweenHandle coinCollectionTween;
        #endregion

        #region INterface Methods
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            SetPanelsToHiddenPosition();
            TryShowTutorialHint();

            pauseButton.OnButtonReleased += OnPause;
            retryButton.OnButtonReleased += OnRetry;
            boatCustomisationBtn.OnButtonReleased += OnBoatCustomize;
            shopBtn.OnButtonReleased += OnShop;
            noAdsBtn.OnButtonReleased += OnNoAds;
            //Powerups
            //  magnetPowerupButton.Init(PowerupType.Magnet, SaveSystem.LoadInt(StringUtils.MAGNET_BALANCE 3);
            speedBoostPowerupButton.Init(PowerupType.SpeedBoost);
            //
            GameController.GetInstance.LevelController.OnPlayerTouch += HandleHidePanels;
            GameController.GetInstance.LevelController.OnDrawPathError += HandleShowPanels;
            GameController.GetInstance.LevelController.OnCompleteSpawnAnimation += HandleShowPanels;
            GameController.GetInstance.OnCoinCollect += HandleCoinCollection;
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
            // magnetPowerupButton.DeInitialize();
            speedBoostPowerupButton.DeInitialize();
            //
            GameController.GetInstance.LevelController.OnPlayerTouch -= HandleHidePanels;
            GameController.GetInstance.LevelController.OnDrawPathError -= HandleShowPanels;
            GameController.GetInstance.LevelController.OnCompleteSpawnAnimation -= HandleShowPanels;
            GameController.GetInstance.OnCoinCollect -= HandleCoinCollection;
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
                    TutorialController.GetInstance.TutorialSpeechBubble.Show(StringUtils.CONSECUTIVE_LOSE_HINT, speechBubblePosition);
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

            TweenManager.MoveAnchorOnAxis(powerupPanel, leftPanelFromX, leftPanelToX, panelSlideDuration, panelSlideEase, TransformAxis.X);
            TweenManager.MoveAnchorOnAxis(boatPanel, rightPanelFromX, rightPanelToX, panelSlideDuration, panelSlideEase, TransformAxis.X);
            TweenManager.MoveAnchorOnAxis(shopPanel, rightPanelFromX, rightPanelToX, panelSlideDuration, panelSlideEase, TransformAxis.X);
            TweenManager.MoveAnchorOnAxis(noAdsPanel, rightPanelFromX, rightPanelToX, panelSlideDuration, panelSlideEase, TransformAxis.X);
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

        #region Handle Button Listener
        private void HandleCoinCollection()
        {
            starProgressBar.transform.localScale = Vector3.one * 0.5f;
            coinCollectionTween.Cancel();
            coinCollectionTween = TweenManager.PunchScale(starProgressBar.transform, starProgressBar.localScale,
                starBarPunchScale, 2, 1, starBarPunchDuration, ease: starBarPunchEase,
                onComplete: () =>
                {
                    starProgressBar.transform.localScale = Vector3.one * 0.5f;
                });
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
            GameController.GetInstance.SetGameState(GameState.Paused);
            GameController.GetInstance.RetryLevel();
            GameController.GetInstance.StartGameplay();
            GameController.GetInstance.LevelController.PlaySpawnAnimations();
        }
        #endregion

    }
}
