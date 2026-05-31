using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PowerupSelectionUIScreen : BaseScreen
    {
        #region Inspector Variables
        [Header("Powerup Buttons")]
        [SerializeField] private PowerupButton magnetButton;
        [SerializeField] private PowerupButton speedButton;
        [Header("Controls")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI levelNumberLabel;
        [SerializeField] private Sprite powerupButtonSprite;
        [SerializeField] private Sprite playButtonSprite;
        [Header("Tutorial Positions")]
        [SerializeField] private Vector3 tutorialCharacterPosition;
        [SerializeField] private Vector3 speechBubblePosition;
        #endregion

        #region Private Variables
        private bool tutorialActive = false;
        #endregion

        #region Lifecycle
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            AddListeners();
            if (tutorialActive)
            {
                // Reset the tutorial state.
                tutorialActive = false;
            }
            UpdateLevelNumber();
           // SetupPowerup(PowerupType.Magnet, magnetButton);
            SetupPowerup(PowerupType.SpeedBoost, speedButton);
        }
        public override void Close()
        {
            base.Close();
            RemoveListeners();
           // magnetButton.DeInitialize();
            speedButton.DeInitialize();
        }
        #endregion

        #region Event Handling
        private void AddListeners()
        {
            playButton.ButtonRegister(OnPlayClicked);
            closeButton.ButtonRegister(ClosePanel);
            TutorialController.GetInstance.OnPowerupPressAction += OnPowerupPressed;
        }
        private void RemoveListeners()
        {
            playButton.ButtonDeRegister(OnPlayClicked);
            closeButton.ButtonDeRegister(ClosePanel);
            TutorialController.GetInstance.OnPowerupPressAction -= OnPowerupPressed;
        }
        private void ClosePanel()
        {
            Close();
        }
        private async void OnPlayClicked()
        {
            var gameState = GameController.GetInstance.GameState;

            if (tutorialActive)
            {
                var tc = TutorialController.GetInstance;
                tc.RemoveTutorialCanvas(playButton.gameObject);
                tc.ClearButtonHighlight();
                tc.TutorialHand.Hide();
                tc.HideBlockerOverlay();
                tutorialActive = false;
            }

            // Fade in before handling play logic
            await UIController.GetInstance.FadeUI.FadeInASync();
            if (gameState == GameState.LevelFail || gameState == GameState.Paused)
            {
                GameController.GetInstance.RetryLevel();
            }
            UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Close);
          //  GameController.GetInstance.Play();
            await UIController.GetInstance.FadeUI.FadeOutASync();
        }

        private void OnPowerupPressed()
        {
            // Only proceed if the tutorial is active
            if (!tutorialActive)
            {
                return;
            }
            var tc = TutorialController.GetInstance;
            if (tc.TutorialType == TutorialType.MagnetPowerup)
            {
                tc.RemoveTutorialCanvas(magnetButton.gameObject);
                tc.TutorialCharacter.SkipAnimation();
                tc.TutorialSpeechBubble.Hide();
                tc.TutorialHand.Hide();
                tc.ClearButtonHighlight();

                //Tween buttonTween = tc.HighlightButton(playButton.transform, playButton.GetComponent<RectTransform>().sizeDelta, playButtonSprite, true);
                //buttonTween.OnComplete(() =>
                //{
                //    tc.EnsureTutorialCanvas(playButton.gameObject, StringUtils.SPRITES_ABOVE_UI_LAYER , IntUtils.TUTORIAL_CANVAS_LAYER);
                //    tc.TutorialHand.ShowHandPointing(playButton.transform);
                //});
            }
            else if (tc.TutorialType == TutorialType.SpeedBoostPowerup)
            {
                tc.RemoveTutorialCanvas(speedButton.gameObject);
                tc.TutorialCharacter.SkipAnimation();
                tc.TutorialSpeechBubble.Hide();
                tc.TutorialHand.Hide();
                tc.ClearButtonHighlight();

                //Tween buttonTween = tc.HighlightButton(playButton.transform, playButton.GetComponent<RectTransform>().sizeDelta, playButtonSprite, true);
                //buttonTween.OnComplete(() =>
                //{
                //    tc.EnsureTutorialCanvas(playButton.gameObject, StringUtils.SPRITES_ABOVE_UI_LAYER, IntUtils.TUTORIAL_CANVAS_LAYER);
                //    tc.TutorialHand.ShowHandPointing(playButton.transform);
                //});
            }
        }
        #endregion

        #region UI Setup
        private void UpdateLevelNumber()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            levelNumberLabel.text = $"LEVEL {currentLevelNumber}";
        }

        private void SetupPowerup(PowerupType type, PowerupButton targetButton)
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            bool isLocked = !GameController.GetInstance.PowerupController.IsPowerupUnlocked(type);

            if (isLocked && GameController.GetInstance.PowerupController.IsUnlockLevelForPowerup(type, currentLevelNumber))
            {
                GameController.GetInstance.PowerupController.UnlockPowerup(type);
                isLocked = false;
                tutorialActive = true;
                var tutorialType = type == PowerupType.Magnet ? TutorialType.MagnetPowerup : TutorialType.SpeedBoostPowerup;
                TutorialController.GetInstance.SetCurrentTutorialType(tutorialType);

                //Tween buttonTween = TutorialController.GetInstance.HighlightButton(targetButton.transform, targetButton.GetComponent<RectTransform>().sizeDelta, powerupButtonSprite);
                //buttonTween.OnComplete(() =>
                //{
                //    TutorialController.GetInstance.EnsureTutorialCanvas(targetButton.gameObject, StringUtils.SPRITES_ABOVE_UI_LAYER, IntUtils.TUTORIAL_CANVAS_LAYER);
                //    TutorialController.GetInstance.TutorialHand.ShowHandPointing(targetButton.transform);
                //});

                //Tween characterMoveTween = TutorialController.GetInstance.TutorialCharacter.PlayAnimation(TutorialCharacterType.Talk, tutorialCharacterPosition);
                //characterMoveTween.OnComplete(() =>
                //{
                //    TutorialController.GetInstance.TutorialSpeechBubble.Show(type == PowerupType.Magnet ?
                //        StringUtils.MAGNET_POWERUP_TUTORIAL_MESSAGE : StringUtils.SPEEDBOOST_POWERUP_TUTORIAL_MESSAGE, speechBubblePosition);
                //});
            }

            int balance = GameController.GetInstance.PowerupController.GetPowerupBalance(type);
            targetButton.Init(type, balance, isLocked);
        }
        #endregion

    }
}
