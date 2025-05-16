using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class MainMenuUIScreen : BaseScreen
    {
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private Button magnetPowerupBtn;
        [SerializeField] private Button speedPowerupBtn;

        private void OnEnable()
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            levelNumberText.text = $"Level {(GameController.GetInstance.CurrentLevelIndex + 1).ToString()}";
            magnetPowerupBtn.onClick.AddListener(OnMagnetPowerup);
            speedPowerupBtn.onClick.AddListener(OnSpeedPowerup);
        }
        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
            magnetPowerupBtn.onClick.RemoveListener(OnMagnetPowerup);
            speedPowerupBtn.onClick.RemoveListener(OnSpeedPowerup);
        }

        private void OnMagnetPowerup()
        {
            GameController.GetInstance.OnMagnetPowerUpActivate();
        }
        private void OnSpeedPowerup()
        {
            GameController.GetInstance.OnSpeedPowerUpActivate();
        }
        private void OnPlayButtonClicked()
        {
            Close();
            GameController.GetInstance.Play();
        }
    }
}
