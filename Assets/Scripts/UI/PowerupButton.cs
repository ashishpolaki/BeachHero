using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PowerupButton : MonoBehaviour
    {
        [SerializeField] private PowerupType powerUpType;
        [SerializeField] private Button activateButton;
        [SerializeField] private Button addMoreButton;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private GameObject lockImageObject;
        [SerializeField] private GameObject graphicsImageObject;
        [SerializeField] private GameObject selectedImageObject;

        private bool isSelected = false;
        private bool isLocked = true;

        public void Init(PowerupType _powerupType, int _powerUpCounter, bool _isLocked)
        {
            powerUpType = _powerupType;
            isLocked = _isLocked;
            isSelected = false;
            lockImageObject.gameObject.SetActive(_isLocked);
            selectedImageObject.SetActive(false);
            graphicsImageObject.SetActive(!_isLocked);
            SetCountText(_powerUpCounter);
            if (isLocked)
            {
                activateButton.interactable = false;
                addMoreButton.gameObject.SetActive(false);
                counterText.gameObject.SetActive(false);
            }
            else
            {
                activateButton.interactable = _powerUpCounter > 0;
                counterText.gameObject.SetActive(_powerUpCounter > 0);
                addMoreButton.gameObject.SetActive(_powerUpCounter <= 0);
                activateButton.onClick.AddListener(OnPowerupButtonClicked);
                addMoreButton.onClick.AddListener(AddMorePowerup);
            }
        }
        public void DeInitialize()
        {
            if (!isLocked)
            {
                activateButton.onClick.RemoveListener(OnPowerupButtonClicked);
                addMoreButton.onClick.RemoveListener(AddMorePowerup);
            }
        }
        private void SetCountText(int count)
        {
            counterText.text = count.ToString();
        }
        private void OnPowerupButtonClicked()
        {
            isSelected = !isSelected;
            selectedImageObject.SetActive(isSelected);
            if (isSelected)
            {
                GameController.GetInstance.PowerupController.AddPowerupInList(powerUpType);
            }
            else
            {
                GameController.GetInstance.PowerupController.RemovePowerupFromList(powerUpType);
            }

            if (!isLocked)
            {
                GameController.GetInstance.TutorialController.OnPowerupPressed();
            }
        }
        private void AddMorePowerup()
        {

        }
    }
}
