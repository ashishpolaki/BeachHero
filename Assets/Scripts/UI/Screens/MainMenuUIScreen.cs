using Sych.ShareAssets.Runtime;
using TMPro;
using UnityEngine;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [Header("References")]
        [SerializeField] private Sprite playButtonSprite;
        [SerializeField] private ShineEffect[] buttonsShineEffect;
        [SerializeField] private HandPointAnimation playButtonTutorialHandAnimation;

        [Header("UI References")]
        [SerializeField] private UIButton boatCustomisationButton;
        [SerializeField] private UIButton playButton;
        [SerializeField] private UIButton storeButton;
        [SerializeField] private UIButton settingsButton;
        [SerializeField] private UIButton leaderBoardButton;
        [SerializeField] private UIButton gpgsSignInButton;
        [SerializeField] private UIButton shareGameButton;
        [SerializeField] private UIButton noAdsButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;

        [Header("Tutorial Positions")]
        [SerializeField] private Vector3 tutorialCharacterPosition;
        [SerializeField] private Vector3 speechBubblePosition;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            SetLevelNumber();
            AddListeners();
            for (int i = 0; i < buttonsShineEffect.Length; i++)
            {
                int index = i;
                buttonsShineEffect[index].Play();
            }
        }
        public override void Close()
        {
            base.Close();
            RemoveListeners();
            for (int i = 0; i < buttonsShineEffect.Length; i++)
            {
                buttonsShineEffect[i].Stop();
            }
        }

        public override void OnScreenOpened()
        {
            if (!SaveSystem.CurrentData.isWelcomeMessageShown)
            {
                OpenAnimator.ApplyAllToStates();
                SaveSystem.CurrentData.isWelcomeMessageShown = true;
                SaveSystem.SaveGameData();
                PlayGamesController.GetInstance.SaveDataInCloud();

                // Highlight the play button and show tutorial.
                var tc = TutorialController.GetInstance;
                playButtonTutorialHandAnimation.SetTarget(playButton.transform);
                tc.TutorialHand.PlayAnimation(playButtonTutorialHandAnimation);
                tc.HighlightButton(playButton.transform, playButton.GetComponent<RectTransform>().sizeDelta, playButtonSprite, true,
                () =>
                {
                    tc.EnsureTutorialCanvas(playButton.gameObject, StringUtils.SPRITES_ABOVE_UI_LAYER, IntUtils.TUTORIAL_CANVAS_LAYER);
                });

                // Move the tutorial character and show welcome message.
                tc.TutorialCharacter.PlayAnimation(TutorialCharacterState.WaveHand, tutorialCharacterPosition);
                //, () =>
                //{
                //    tc.TutorialSpeechBubble.Show(StringUtils.TUTORIAL_WELCOME_MESSAGE, speechBubblePosition);
                //});
                UIController.GetInstance.EndTransition();
            }
            else
            {
                base.OnScreenOpened();
            }
        }

        private void AddListeners()
        {
            boatCustomisationButton.OnButtonReleased += (OnBoatCustomisationButtonClicked);
            playButton.OnButtonReleased += OnPlayButtonClicked;
            storeButton.OnButtonReleased += (OnStoreButtonClicked);
            settingsButton.OnButtonReleased += (OnSettingsButtonClick);
            leaderBoardButton.OnButtonReleased += OpenLeaderboards;
            if (gpgsSignInButton != null) gpgsSignInButton.OnButtonReleased += OnGPGSSignInClicked;
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
            if (gpgsSignInButton != null) gpgsSignInButton.OnButtonReleased -= OnGPGSSignInClicked;
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
            if (!PlayGamesController.GetInstance.IsAuthenticated)
            {
                return;
            }
            PlayGamesController.GetInstance.ShowLeaderboardUI();
        }

        private void OnGPGSSignInClicked()
        {
            //  PlayGamesController.GetInstance.si();
        }

        private void OnSettingsButtonClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Settings, UIScreenEvent.Push);
        }

        private void OnBoatCustomisationButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.BoatCustomisation, UIScreenEvent.Push);
        }

        private void OnPlayButtonClicked()
        {
            if (SaveSystem.CurrentData.isWelcomeMessageShown)
            {
                var tc = TutorialController.GetInstance;
                tc.RemoveTutorialCanvas(playButton.gameObject);
                tc.ClearButtonHighlight();
                tc.HideBlockerOverlay();
                tc.TutorialHand.Hide();
                tc.TutorialCharacter.SkipAnimation();
                //  tc.TutorialSpeechBubble.Hide();
            }
            MapController.GetInstance.SyncCharacterToLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
            GameController.GetInstance.SetGameState(GameState.Map);
        }
        private void OnStoreButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Push);
        }

        private void SetLevelNumber()
        {
            //int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            //levelNumberText.text = $"{currentLevelNumber}";
        }
    }
}
