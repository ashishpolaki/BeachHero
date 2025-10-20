using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PowerupSelectionUIScreen : BaseScreen
    {
        #region Inspector Variables
        [SerializeField] private PowerupTutorialPanel tutorialPanel;
        [SerializeField] private PowerupButton magnetButton;
        [SerializeField] private PowerupButton speedButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI levelNumberLabel;
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
                tutorialPanel.Deactivate();
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
            GameController.GetInstance.TutorialController.OnPowerupPressAction += OnPowerupPressed;
        }
        private void RemoveListeners()
        {
            playButton.ButtonDeRegister(OnPlayClicked);
            closeButton.ButtonDeRegister(ClosePanel);
            GameController.GetInstance.TutorialController.OnPowerupPressAction -= OnPowerupPressed;
        }
        private void ClosePanel()
        {
            Close();
        }
        private async void OnPlayClicked()
        {
            var gameState = GameController.GetInstance.GameState;

            if(tutorialActive)
            {
                TutorialUiUtility.RemoveTutorialCanvas(playButton.gameObject);
                tutorialPanel.Deactivate();
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
            await UIController.GetInstance.FadeUI.FadeOutASync();
        }

        private void OnPowerupPressed()
        {
            // Only proceed if the tutorial is active
            if (!tutorialActive)
            {
                return;
            }
            TutorialUiUtility.RemoveTutorialCanvas(magnetButton.gameObject);
            TutorialUiUtility.RemoveTutorialCanvas(speedButton.gameObject);
            tutorialPanel.OnPowerupButtonPressed(playButton.transform);
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
                tutorialPanel.ShowPowerupTutorial(targetButton.transform);
            }

            int balance = GameController.GetInstance.PowerupController.GetPowerupBalance(type);
            targetButton.Init(type, balance, isLocked);
        }
        #endregion

    }
}
