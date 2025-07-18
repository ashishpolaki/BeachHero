using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PowerupSelectionUIScreen : BaseScreen
    {
        [SerializeField] private PowerupTutorialPanel powerupTutorialPanel;
        [SerializeField] private PowerupButton magnetPowerup;
        [SerializeField] private PowerupButton speedPowerup;
        [SerializeField] private Button playButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;

        private bool isPowerupTutorialEnabled = false;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            AddListeners();
            isPowerupTutorialEnabled = false; // Reset the tutorial state
            powerupTutorialPanel.Deactivate();
            SetLevelNumber();
            InitMagnetPowerup();
            InitSpeedBoostPowerup();
        }
        public override void Close()
        {
            base.Close();
            RemoveListeners();
            magnetPowerup.DeInitialize();
            speedPowerup.DeInitialize();
        }
        private void AddListeners()
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            closeButton.ButtonRegister(ClosePanel);
            GameController.GetInstance.TutorialController.OnPowerupPressAction += OnPowerupButtonPressed;
        }
        private void RemoveListeners()
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
            closeButton.ButtonDeRegister();
            GameController.GetInstance.TutorialController.OnPowerupPressAction -= OnPowerupButtonPressed;
        }
        private void ClosePanel()
        {
            Close();
        }
        private async void OnPlayButtonClicked()
        {
            var gameState = GameController.GetInstance.GameState;

            // Fade in before handling play logic
            await UIController.GetInstance.FadeInASync();

            if (gameState == GameState.LevelFailed || gameState == GameState.Paused)
            {
                GameController.GetInstance.RetryLevel();
            }
            else
            {
                GameController.GetInstance.CameraController.EnableCameras();
                await SceneLoader.GetInstance.UnloadScene(StringUtils.MAP_SCENE, IntUtils.MAP_SCENE_LOAD_DELAY);
            }
            UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Close);
            GameController.GetInstance.Play();
            await UIController.GetInstance.FadeOutASync();
        }
        private void SetLevelNumber()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            levelNumberText.text = $"Level {currentLevelNumber}";
        }
        private void OnPowerupButtonPressed()
        {
            if (!isPowerupTutorialEnabled)
                return;
            powerupTutorialPanel.OnPowerupButtonPressed();
        }
        private void InitMagnetPowerup()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            bool isMagnetPowerupLocked = !GameController.GetInstance.TutorialController.IsMagnetPowerupUnlocked();
            if (isMagnetPowerupLocked)
            {
                bool isMagnetUnlockLevel = GameController.GetInstance.TutorialController.IsMagnetUnlockLevel(currentLevelNumber);
                if (isMagnetUnlockLevel)
                {
                    SaveSystem.SaveBool(StringUtils.MAGNET_UNLOCKED, true);
                    isMagnetPowerupLocked = false;
                    isPowerupTutorialEnabled = true;
                    powerupTutorialPanel.ShowMagnetPowerupTutorial(magnetPowerup.transform.position);
                }
            }
            int magnetPowerupCount = GameController.GetInstance.PowerupController.MagnetBalance;
            magnetPowerup.Init(PowerupType.Magnet, magnetPowerupCount, isMagnetPowerupLocked);
        }
        private void InitSpeedBoostPowerup()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            bool isSpeedPowerupLocked = !GameController.GetInstance.TutorialController.IsSpeedBoostPowerupUnlocked();
            if (isSpeedPowerupLocked)
            {
                bool isSpeedBoostUnlockLevel = GameController.GetInstance.TutorialController.IsSpeedBoostUnlockLevel(currentLevelNumber);
                if (isSpeedBoostUnlockLevel)
                {
                    SaveSystem.SaveBool(StringUtils.SPEEDBOOST_UNLOCKED, true);
                    isSpeedPowerupLocked = false;
                    isPowerupTutorialEnabled = true;
                    powerupTutorialPanel.ShowSpeedBoostPowerupTutorial(speedPowerup.transform.position);
                }
            }
            int speedPowerupCount = GameController.GetInstance.PowerupController.SpeedBoostBalance;
            speedPowerup.Init(PowerupType.SpeedBoost, speedPowerupCount, isSpeedPowerupLocked);
        }
    }
}
