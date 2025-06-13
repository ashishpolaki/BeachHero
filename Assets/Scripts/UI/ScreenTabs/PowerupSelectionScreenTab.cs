using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PowerupSelectionScreenTab : BaseScreenTab
    {
        [SerializeField] private PowerupTutorialPanel powerupTutorialPanel;
        [SerializeField] private PowerupButton magnetPowerup;
        [SerializeField] private PowerupButton speedPowerup;
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;

        private bool isPowerupTutorialEnabled = false;

        public override void Open()
        {
            base.Open();
            AddListeners();
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
            GameController.GetInstance.TutorialController.OnPowerupPressAction += OnPowerupButtonPressed;
        }
        private void SetLevelNumber()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            levelNumberText.text = $"Level {currentLevelNumber}";
        }
        private void RemoveListeners()
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
            GameController.GetInstance.TutorialController.OnPowerupPressAction -= OnPowerupButtonPressed;
        }
        private void OnPlayButtonClicked()
        {
            GameController.GetInstance.Play();
            Close();
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
                    SaveController.SaveBool(StringUtils.MAGNET_POWERUP, true);
                    isMagnetPowerupLocked = false;
                    isPowerupTutorialEnabled = true;
                    powerupTutorialPanel.ShowMagnetPowerupTutorial(magnetPowerup.transform.position);
                }
            }
            int magnetPowerupCount = GameController.GetInstance.PowerupController.GetPowerupCount(PowerupType.Magnet);
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
                    SaveController.SaveBool(StringUtils.SPEEDBOOST_POWERUP, true);
                    isSpeedPowerupLocked = false;
                    isPowerupTutorialEnabled = true;
                    powerupTutorialPanel.ShowSpeedBoostPowerupTutorial(speedPowerup.transform.position);
                }
            }
            int speedPowerupCount = GameController.GetInstance.PowerupController.GetPowerupCount(PowerupType.SpeedBoost);
            speedPowerup.Init(PowerupType.SpeedBoost, speedPowerupCount, isSpeedPowerupLocked);
        }

    }
}
