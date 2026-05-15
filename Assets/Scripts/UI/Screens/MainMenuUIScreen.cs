using Sych.ShareAssets.Runtime;
using TMPro;
using UnityEngine;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [SerializeField] private UIButton boatCustomisationButton;
        [SerializeField] private UIButton playButton;
        [SerializeField] private UIButton storeButton;
        [SerializeField] private UIButton settingsButton;
        [SerializeField] private UIButton leaderBoardButton;
        [SerializeField] private UIButton shareGameButton;
        [SerializeField] private UIButton noAdsButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private Sprite playButtonSprite;
        [Header("Tutorial Positions")]
        [SerializeField] private Vector3 tutorialCharacterPosition;
        [SerializeField] private Vector3 speechBubblePosition;

        private bool isWelcomeMessageShown = false;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            SetLevelNumber();
            AddListeners();
        }
        public override void OnScreenOpened()
        {
            isWelcomeMessageShown = SaveSystem.LoadBool(StringUtils.SHOW_WELCOME_MESSAGE, false);
            if (!isWelcomeMessageShown)
            {
                OpenAnimator.ApplyAllToStates();
                SaveSystem.SaveBool(StringUtils.SHOW_WELCOME_MESSAGE, true);
                isWelcomeMessageShown = true;

                // Highlight the play button and show tutorial.
                var tc = TutorialController.GetInstance;
                tc.HighlightButton(playButton.transform, playButton.GetComponent<RectTransform>().sizeDelta, playButtonSprite, true,
                () =>
                {
                    tc.EnsureTutorialCanvas(playButton.gameObject, StringUtils.SPRITES_ABOVE_UI_LAYER, IntUtils.TUTORIAL_CANVAS_LAYER);
                    tc.TutorialHand.ShowHandPointing(playButton.transform);
                });

                // Move the tutorial character and show welcome message.
                tc.TutorialCharacter.PlayAnimation(TutorialCharacterState.WaveHand, tutorialCharacterPosition, () =>
                {
                    tc.TutorialSpeechBubble.Show(StringUtils.TUTORIAL_WELCOME_MESSAGE, speechBubblePosition);
                });
                UIController.GetInstance.EndTransition();
            }
            else
            {
                base.OnScreenOpened();
            }
        }

        public override void Close()
        {
            base.Close();
            RemoveListeners();
        }

        private void AddListeners()
        {
            boatCustomisationButton.OnButtonReleased += (OnBoatCustomisationButtonClicked);
            playButton.OnButtonReleased += OnPlayButtonClicked;
            storeButton.OnButtonReleased += (OnStoreButtonClicked);
            settingsButton.OnButtonReleased += (OnSettingsButtonClick);
            leaderBoardButton.OnButtonReleased += OpenLeaderboards;
            shareGameButton.OnButtonReleased += ShareClicked;
            noAdsButton.OnButtonReleased += NoAdsButtonClicked;
        }

        private void RemoveListeners()
        {
            boatCustomisationButton.OnButtonReleased -= (OnBoatCustomisationButtonClicked);
            playButton.OnButtonReleased -= OnPlayButtonClicked;
            storeButton.OnButtonReleased -= (OnStoreButtonClicked);
            settingsButton.OnButtonReleased -= (OnSettingsButtonClick);
            leaderBoardButton.OnButtonReleased -= OpenLeaderboards;
            shareGameButton.OnButtonReleased -= ShareClicked;
            noAdsButton.OnButtonReleased -= NoAdsButtonClicked;
        }

        private void ShareClicked()
        {
            if (!Share.IsPlatformSupported)
            {
                DebugUtils.LogError("Share: platform not supported");
                return;
            }

            var item = "https://play.google.com/store/apps/details?id=com.hunterKirito.BeachHero";
            Share.Item(item, success =>
            {
                DebugUtils.Log($"Share: {(success ? "success" : "failed")}");
            });
        }

        private void NoAdsButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Push);
        }

        private void OpenLeaderboards()
        {
            LeaderboardController.GetInstance.ShowLeaderboardUI();
        }

        private void OnSettingsButtonClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Settings, UIScreenEvent.Push);
        }

        private void OnBoatCustomisationButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.BoatCustomisation, UIScreenEvent.Open);
        }

        private void OnPlayButtonClicked()
        {
            if (isWelcomeMessageShown)
            {
                var tc = TutorialController.GetInstance;
                tc.RemoveTutorialCanvas(playButton.gameObject);
                tc.ClearButtonHighlight();
                tc.HideBlockerOverlay();
                tc.TutorialHand.Hide();
                tc.TutorialCharacter.SkipAnimation();
                tc.TutorialSpeechBubble.Hide();
            }
            MapController.GetInstance.SyncCharacterToLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
            GameController.GetInstance.SetGameState(GameState.Map);
        }
        private void OnStoreButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Open);
        }

        private void SetLevelNumber()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            levelNumberText.text = $"{currentLevelNumber}";
        }
    }
}
