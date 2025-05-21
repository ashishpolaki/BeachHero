using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private PowerupButton magnetPowerup;
        [SerializeField] private PowerupButton speedPowerup;

        private List<PowerupType> activatePowerupList = new List<PowerupType>();

        private void OnEnable()
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            levelNumberText.text = $"Level {(GameController.GetInstance.CurrentLevelIndex + 1)}";
            int magnetPowerupCount = GameController.GetInstance.PowerupController.GetPowerupCount(PowerupType.Magnet);
            int speedPowerupCount = GameController.GetInstance.PowerupController.GetPowerupCount(PowerupType.Speed);
            magnetPowerup.Init(PowerupType.Magnet, magnetPowerupCount, OnPowerupSelect);
            speedPowerup.Init(PowerupType.Speed, speedPowerupCount, OnPowerupSelect);
        }
        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
            magnetPowerup.DeInitialize();
            speedPowerup.DeInitialize();
        }
        private void OnPowerupSelect(PowerupType powerupType, bool isSelected)
        {
            if (isSelected)
            {
                activatePowerupList.Add(powerupType);
            }
            else
            {
                activatePowerupList.Remove(powerupType);
            }
        }
        private void OnPlayButtonClicked()
        {
            GameController.GetInstance.Play(activatePowerupList);
            activatePowerupList.Clear();
            Close();
        }
    }
}
