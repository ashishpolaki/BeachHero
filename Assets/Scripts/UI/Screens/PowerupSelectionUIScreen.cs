using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PowerupSelectionUIScreen : BaseScreen
    {
        #region Inspector Variables
        [SerializeField] private PowerupButton magnetButton;
        [SerializeField] private PowerupButton speedButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI levelNumberLabel;
        [SerializeField] private Sprite powerupButtonSprite;
        [SerializeField] private Sprite playButtonSprite;
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
            SetupPowerup(PowerupType.Magnet, magnetButton);
            SetupPowerup(PowerupType.SpeedBoost, speedButton);
        }
        public override void Close()
        {
            base.Close();
            RemoveListeners();
            magnetButton.DeInitialize();
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
                TutorialController.GetInstance.RemoveTutorialCanvas(playButton.gameObject);
                TutorialController.GetInstance.ClearButtonHighlight();
                TutorialController.GetInstance.TutorialHand.Hide();
                TutorialController.GetInstance.HideBlockerOverlay();
                tutorialActive = false;
            }

            // Fade in before handling play logic
            await UIController.GetInstance.FadeUI.FadeInASync();
            if (gameState == GameState.LevelFail || gameState == GameState.Paused)
            {
                GameController.GetInstance.RetryLevel();
            }
            UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Close);
            GameController.GetInstance.Play();
            MapController.GetInstance.SetMapActive(true);
            await UIController.GetInstance.FadeUI.FadeOutASync();
        }

        private void OnPowerupPressed()
        {
            // Only proceed if the tutorial is active
            if (!tutorialActive)
            {
                return;
            }
            if (TutorialController.GetInstance.TutorialType == TutorialType.MagnetPowerup)
            {
                TutorialController.GetInstance.RemoveTutorialCanvas(magnetButton.gameObject);
                TutorialController.GetInstance.TutorialCharacter.SkipAnimation();
                TutorialController.GetInstance.TutorialSpeechBubble.Hide();
            }
            else if (TutorialController.GetInstance.TutorialType == TutorialType.SpeedBoostPowerup)
            {
                TutorialController.GetInstance.RemoveTutorialCanvas(speedButton.gameObject);
                TutorialController.GetInstance.TutorialCharacter.SkipAnimation();
                TutorialController.GetInstance.TutorialSpeechBubble.Hide();
            }
            TutorialController.GetInstance.ClearButtonHighlight();
            TutorialController.GetInstance.TutorialHand.Hide();
            TutorialController.GetInstance.HighlightButton(playButton.transform, playButton.GetComponent<RectTransform>().sizeDelta, playButtonSprite, true);
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
                TutorialController.GetInstance.TutorialCharacter.PlayAnimation(TutorialCharacterType.Talk);
                TutorialController.GetInstance.TutorialSpeechBubble.Show(type == PowerupType.Magnet ?
                    StringUtils.MAGNET_POWERUP_TUTORIAL_MESSAGE : StringUtils.SPEEDBOOST_POWERUP_TUTORIAL_MESSAGE);
                TutorialController.GetInstance.SetCurrentTutorialType(tutorialType);
                TutorialController.GetInstance.HighlightButton(targetButton.transform, targetButton.GetComponent<RectTransform>().sizeDelta, powerupButtonSprite);
            }

            int balance = GameController.GetInstance.PowerupController.GetPowerupBalance(type);
            targetButton.Init(type, balance, isLocked);
        }
        #endregion

    }
}
