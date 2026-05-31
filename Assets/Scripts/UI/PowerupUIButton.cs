using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BeachHero
{
    public class PowerupUIButton : UIButton
    {
        [SerializeField] private PowerupType powerUpType;
        [SerializeField] private Button addMoreButton;
        [SerializeField] private Image iconImg;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private GameObject lockObj;

        private PowerupController powerupController;
        private bool isSelected = false;
        private int balance = 0;
        private bool isUnlocked = false;

        public void Init(PowerupType _powerupType)
        {
            powerUpType = _powerupType;
            powerupController = GameController.GetInstance.PowerupController;
            if (powerupController.IsUnlockLevelForPowerup(powerUpType, GameController.GetInstance.CurrentLevelIndex + 1))
            {
                powerupController.UnlockPowerup(powerUpType);
            }
            isUnlocked = powerupController.IsPowerupUnlocked(powerUpType);
            if (isUnlocked)
            {
                iconImg.gameObject.SetActive(true);
                counterText.gameObject.SetActive(true);
                balance = powerupController.GetPowerupBalance(powerUpType);
                SetCountText(balance);
                OnButtonReleased += OnPowerupButtonClicked;
            }
            else
            {
                lockObj.SetActive(true);
            }
        }
        public void DeInitialize()
        {
            if (isUnlocked)
            {
                OnButtonReleased -= OnPowerupButtonClicked;
            }
            selectedIndicator.SetActive(false);
            lockObj.SetActive(false);
            iconImg.gameObject.SetActive(false);
            counterText.gameObject.SetActive(false);
            isSelected = false;
        }
        private void SetCountText(int count)
        {
            counterText.text = count.ToString();
        }
        private void OnPowerupButtonClicked()
        {
            isSelected = !isSelected;
            selectedIndicator.SetActive(isSelected);
            if (isSelected)
            {
                GameController.GetInstance.PowerupController.AddPowerupInList(powerUpType);
            }
            else
            {
                GameController.GetInstance.PowerupController.RemovePowerupFromList(powerUpType);
            }
        }
    }
}
