using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private RectTransform tutorialHandRect;
        [SerializeField] private RectTransform powerUpTutorialRect;
        [SerializeField] private RectTransform playButtonTutorialRect;
        [SerializeField] private PowerupButton magnetPowerup;
        [SerializeField] private PowerupButton speedPowerup;

        // Hand animation parameters
        [SerializeField] private float handScaleDuration = 0.5f;
        [SerializeField] private float handScaleElasticity = 0.2f;
        [SerializeField] private float handScalePunch = 0.2f;

        private bool isPowerupTutorialEnabled = false;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            playButton.onClick.AddListener(OnPlayButtonClicked);
            GameController.GetInstance.TutorialController.OnPowerupPressAction += OnTutorialPowerupPressed;
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            levelNumberText.text = $"Level {currentLevelNumber}";
            DeactivateTutorialPanel();
            InitMagnetPowerup();
            InitSpeedBoostPowerup();
        }
        public override void Close()
        {
            base.Close();
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
            GameController.GetInstance.TutorialController.OnPowerupPressAction -= OnTutorialPowerupPressed;
            magnetPowerup.DeInitialize();
            speedPowerup.DeInitialize();
        }
        private void OnPlayButtonClicked()
        {
            GameController.GetInstance.Play();
            Close();
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
                    ShowMagnetPowerupTutorial();
                }
            }
            int magnetPowerupCount = GameController.GetInstance.PowerupController.GetPowerupCount(PowerupType.Magnet);
            magnetPowerup.Init(PowerupType.Magnet, magnetPowerupCount, isMagnetPowerupLocked);
        }
        private void InitSpeedBoostPowerup()
        {
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            bool isSpeedPowerupLocked = !GameController.GetInstance.TutorialController.IsSpeedBoostPowerupUnlocked();
            if(isSpeedPowerupLocked)
            {
                bool isSpeedBoostUnlockLevel = GameController.GetInstance.TutorialController.IsSpeedBoostUnlockLevel(currentLevelNumber);
                if (isSpeedBoostUnlockLevel)
                {
                    SaveController.SaveBool(StringUtils.SPEEDBOOST_POWERUP, true);
                    isSpeedPowerupLocked = false;
                    isPowerupTutorialEnabled = true;
                    ShowSpeedBoostPowerupTutorial();
                }
            }
            int speedPowerupCount = GameController.GetInstance.PowerupController.GetPowerupCount(PowerupType.SpeedBoost);
            speedPowerup.Init(PowerupType.SpeedBoost, speedPowerupCount, isSpeedPowerupLocked);
        }
        private void ShowMagnetPowerupTutorial()
        {
            powerUpTutorialRect.gameObject.SetActive(true);
            powerUpTutorialRect.position = magnetPowerup.GetComponent<RectTransform>().position;
            tutorialHandRect.gameObject.SetActive(true);
            tutorialHandRect.position = powerUpTutorialRect.position;
        }
        private void ShowSpeedBoostPowerupTutorial()
        {
            powerUpTutorialRect.gameObject.SetActive(true);
            powerUpTutorialRect.position = speedPowerup.GetComponent<RectTransform>().position;
            tutorialHandRect.gameObject.SetActive(true);
            tutorialHandRect.position = powerUpTutorialRect.position;
        }
        private void DeactivateTutorialPanel()
        {
            powerUpTutorialRect.gameObject.SetActive(false);
            playButtonTutorialRect.gameObject.SetActive(false);
            tutorialHandRect.gameObject.SetActive(false);
        }
        private void OnTutorialPowerupPressed()
        {
            if (!isPowerupTutorialEnabled)
                return;
            powerUpTutorialRect.gameObject.SetActive(false);
            playButtonTutorialRect.gameObject.SetActive(true);
            tutorialHandRect.position = playButtonTutorialRect.position;
        }
    }
}
