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
        [SerializeField] private NotificationBadgeUI boatCustomisationNotificationBadgeUI;
        [SerializeField] private UIButton playButton;
        [SerializeField] private UIButton storeButton;
        [SerializeField] private UIButton settingsButton;
        [SerializeField] private UIButton leaderBoardButton;
        [SerializeField] private UIButton gpgsSignInButton;
        [SerializeField] private UIButton rateUsButton;
        [SerializeField] private UIButton shareGameButton;
        [SerializeField] private UIButton noAdsButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;

        [Header("Tutorial Positions")]
        [SerializeField] private Vector3 tutorialCharacterPosition;
        [SerializeField] private Vector3 speechBubblePosition;

        [Header("Loading Spinner")]
        [SerializeField] private SimpleSpinner simpleSpinner;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            AddListeners();
            EnableGPGSButton();
            EnableNoAdsButton();
            ShowBoatCustomisationNotificationBadge();
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
            HideBoatCustomisationNotificationBadge();
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
                UIController.GetInstance.EndTransition();
            }
            else
            {
                base.OnScreenOpened();
            }
        }

        private void AddListeners()
        {
            if (boatCustomisationButton != null) boatCustomisationButton.OnButtonReleased += (OnBoatCustomisationButtonClicked);
            if (playButton != null) playButton.OnButtonReleased += OnPlayButtonClicked;
            if (storeButton != null) storeButton.OnButtonReleased += (OnStoreButtonClicked);
            if (settingsButton != null) settingsButton.OnButtonReleased += (OnSettingsButtonClick);
            if (leaderBoardButton != null) leaderBoardButton.OnButtonReleased += OpenLeaderboards;
            if (rateUsButton != null) rateUsButton.OnButtonReleased += OnRateUsClick;
            if (gpgsSignInButton != null) gpgsSignInButton.OnButtonReleased += OnGPGSSignInClicked;
            if (shareGameButton != null) shareGameButton.OnButtonReleased += ShareClicked;
            if (noAdsButton != null) noAdsButton.OnButtonReleased += NoAdsButtonClicked;
        }

        private void RemoveListeners()
        {
            if (boatCustomisationButton != null) boatCustomisationButton.OnButtonReleased -= (OnBoatCustomisationButtonClicked);
            if (playButton != null) playButton.OnButtonReleased -= OnPlayButtonClicked;
            if (storeButton != null) storeButton.OnButtonReleased -= (OnStoreButtonClicked);
            if (settingsButton != null) settingsButton.OnButtonReleased -= (OnSettingsButtonClick);
            if (leaderBoardButton != null) leaderBoardButton.OnButtonReleased -= OpenLeaderboards;
            if (rateUsButton != null) rateUsButton.OnButtonReleased -= OnRateUsClick;
            if (gpgsSignInButton != null) gpgsSignInButton.OnButtonReleased -= OnGPGSSignInClicked;
            if (shareGameButton != null) shareGameButton.OnButtonReleased -= ShareClicked;
            if (noAdsButton != null) noAdsButton.OnButtonReleased -= NoAdsButtonClicked;
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

        private void EnableGPGSButton()
        {
            bool isSignedIn = PlayGamesController.GetInstance.IsAuthenticated;
            if (gpgsSignInButton != null)
            {
                gpgsSignInButton.SetInteractable(!isSignedIn);
            }
        }

        private void EnableNoAdsButton()
        {
            bool isNoAdsPurchased = AdController.GetInstance.NoAdsPurchased();
            if (noAdsButton != null)
            {
                noAdsButton.SetInteractable(!isNoAdsPurchased);
            }
        }

        private void ShowBoatCustomisationNotificationBadge()
        {
            if (boatCustomisationNotificationBadgeUI != null && SaveSystem.CurrentData != null &&
                !SaveSystem.CurrentData.isBoatNotificationShown && GameController.GetInstance.HighestCompletedLevelIndex >= IntUtils.BOAT_NOTIFICATION_SHOWN_LEVEL)
            {
                boatCustomisationNotificationBadgeUI.Show();
            }
        }

        private void HideBoatCustomisationNotificationBadge()
        {
            if (boatCustomisationNotificationBadgeUI != null)
            {
                boatCustomisationNotificationBadgeUI.Hide();
            }
        }

        private void OnGPGSSignInClicked()
        {
            if (!NetworkController.IsInternetAvailable)
            {
                return;
            }

            if (simpleSpinner != null) simpleSpinner.StartSpinning();
            PlayGamesController.GetInstance.SignInASync(success =>
            {
                if (simpleSpinner != null) simpleSpinner.StopSpinning();
                if (success)
                {
                    EnableGPGSButton();
                }
            });
        }

        private void OnRateUsClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.RateUs, UIScreenEvent.Push);
        }

        private void OnSettingsButtonClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Settings, UIScreenEvent.Push);
        }

        private void OnBoatCustomisationButtonClicked()
        {
            HideBoatCustomisationNotificationBadge();
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
    }
}
