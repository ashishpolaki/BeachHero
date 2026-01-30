using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BeachHero
{
    public class PowerupUIButton : UIButton
    {
        [SerializeField] private PowerupType powerUpType;
        [SerializeField] private Button addMoreButton;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private GameObject selectedIndicator;

        private bool isSelected = false;

        public void Init(PowerupType _powerupType, int _powerUpCounter)
        {
            powerUpType = _powerupType;
            SetCountText(_powerUpCounter);
            OnButtonReleased += OnPowerupButtonClicked;
        }
        public void DeInitialize()
        {
            OnButtonReleased -= OnPowerupButtonClicked;
            selectedIndicator.SetActive(false);
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
            if(isSelected)
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
