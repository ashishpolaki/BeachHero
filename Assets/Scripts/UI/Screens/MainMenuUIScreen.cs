using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [SerializeField] private Button boatCustomisationButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button storeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private TweenSequencer panelOpenAnimation;
        [SerializeField] private Sprite playButtonSprite;
        [Header("Tutorial Positions")]
        [SerializeField] private Vector3 tutorialCharacterPosition;
        [SerializeField] private Vector3 speechBubblePosition;

        private bool isWelcomeMessageShown = false;

        public override void Open(ScreenTabType screenTabType)
        {
            panelOpenAnimation.BuildSequence();
            base.Open(screenTabType);
            SetLevelNumber();
            AddListeners();
            OnOpenPanel();
        }

        public void OnOpenPanel()
        {
            isWelcomeMessageShown = SaveSystem.LoadBool(StringUtils.SHOW_WELCOME_MESSAGE, false);
            if (!isWelcomeMessageShown)
            {
                panelOpenAnimation.ApplyAllToStates();
                SaveSystem.SaveBool(StringUtils.SHOW_WELCOME_MESSAGE, true);
                isWelcomeMessageShown = true;

                // Highlight the play button and show tutorial.
                var tc = TutorialController.GetInstance;
                Tween buttonTween = tc.HighlightButton(playButton.transform, playButton.GetComponent<RectTransform>().sizeDelta, playButtonSprite, true);
                buttonTween.OnComplete(() =>
                {
                    tc.EnsureTutorialCanvas(playButton.gameObject, StringUtils.SPRITES_ABOVE_UI_LAYER, IntUtils.TUTORIAL_CANVAS_LAYER);
                    tc.TutorialHand.ShowHandPointing(playButton.transform);
                });

                // Move the tutorial character and show welcome message.
                Tween characterMoveTween = tc.TutorialCharacter.PlayAnimation(TutorialCharacterType.WaveHand, tutorialCharacterPosition);
                characterMoveTween.OnComplete(() =>
                {
                    tc.TutorialSpeechBubble.Show(StringUtils.TUTORIAL_WELCOME_MESSAGE, speechBubblePosition);
                });
            }
            else
            {
                panelOpenAnimation.Play();
            }
        }

        public override void Close()
        {
            base.Close();
            RemoveListeners();
            panelOpenAnimation.Kill();
        }

        private void AddListeners()
        {
            boatCustomisationButton.ButtonRegister(OnBoatCustomisationButtonClicked);
            playButton.ButtonRegister(OnPlayButtonClicked);
            storeButton.ButtonRegister(OnStoreButtonClicked);
            settingsButton.ButtonRegister(OnSettingsButtonClick);
        }

        private void RemoveListeners()
        {
            boatCustomisationButton.ButtonDeRegister(OnBoatCustomisationButtonClicked);
            playButton.ButtonDeRegister(OnPlayButtonClicked);
            storeButton.ButtonDeRegister(OnStoreButtonClicked);
            settingsButton.ButtonDeRegister(OnSettingsButtonClick);
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

            MapController.GetInstance.CheckForMapUpdate();
            UIController.GetInstance.ScreenEvent(ScreenType.Map, UIScreenEvent.Open);
            GameController.GetInstance.SetGameState(GameState.Map);
            MapController.GetInstance.PlaceBoatAtCurrentLevel();
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
