using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PowerupButton : MonoBehaviour
    {
        [SerializeField] private PowerupType powerUpType; 
        [SerializeField] private Button powerUpButton;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;
        private Action<PowerupType,bool> OnPowerUpSelect;
        private bool isSelected = false;

        public void Init(PowerupType _powerupType,int powerUpCounter, Action<PowerupType, bool> action)
        {
            powerUpType = _powerupType;
            OnPowerUpSelect = action;
            SetCounterText(powerUpCounter);
            powerUpButton.image.color = normalColor;
            isSelected = false;
            powerUpButton.onClick.AddListener(OnPowerupButtonClicked);
            if (powerUpCounter <= 0)
            {
                powerUpButton.interactable = false;
            }
        }
        public void DeInitialize()
        {
            powerUpButton.onClick.RemoveListener(OnPowerupButtonClicked);
            OnPowerUpSelect = null;
        }
        public void SetCounterText(int count)
        {
            counterText.text = count.ToString();
        }
        private void OnPowerupButtonClicked()
        {
            isSelected = !isSelected;
            powerUpButton.image.color = isSelected ? selectedColor : normalColor;
            OnPowerUpSelect?.Invoke(powerUpType, isSelected);
        }
    }
}
