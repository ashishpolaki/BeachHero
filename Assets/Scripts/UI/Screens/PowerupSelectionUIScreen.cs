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
        [SerializeField] private Transform buttonsContainer;
        #endregion

        #region Private Variables
        private bool tutorialActive = false;
        #endregion

        #region Lifecycle
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            AddListeners();
            tutorialActive = false; // Reset the tutorial state.
            tutorialPanel.Deactivate();
            UpdateLevelNumber();
            SetupPowerup(PowerupType.Magnet, magnetButton, speedButton);
            SetupPowerup(PowerupType.SpeedBoost, speedButton, magnetButton);
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
            playButton.onClick.AddListener(OnPlayClicked);
            closeButton.ButtonRegister(ClosePanel);
            GameController.GetInstance.TutorialController.OnPowerupPressAction += OnPowerupPressed;
        }
        private void RemoveListeners()
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
            closeButton.ButtonDeRegister();
            GameController.GetInstance.TutorialController.OnPowerupPressAction -= OnPowerupPressed;
        }
        private void ClosePanel()
        {
            Close();
        }
        private async void OnPlayClicked()
        {
            var gameState = GameController.GetInstance.GameState;

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
            if (!tutorialActive)
                return;
            tutorialPanel.OnPowerupButtonPressed(buttonsContainer);
        }
        #endregion

        #region UI Setup
        private void UpdateLevelNumber()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            levelNumberLabel.text = $"LEVEL {currentLevelNumber}";
        }

        private void SetupPowerup(PowerupType type, PowerupButton targetButton, PowerupButton otherButton)
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            bool isLocked = !GameController.GetInstance.PowerupController.IsPowerupUnlocked(type);

            if (isLocked && GameController.GetInstance.PowerupController.IsUnlockLevelForPowerup(type, currentLevelNumber))
            {
                GameController.GetInstance.PowerupController.UnlockPowerup(type);
                isLocked = false;

                tutorialActive = true;
                otherButton.transform.SetParent(buttonsContainer);
                playButton.transform.SetParent(buttonsContainer);

                tutorialPanel.ShowPowerupTutorial(targetButton.transform, playButton.transform);
            }

            int balance = GameController.GetInstance.PowerupController.GetPowerupBalance(type);
            targetButton.Init(type, balance, isLocked);
        }
        #endregion

    }
}
