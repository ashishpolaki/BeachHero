using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class HeaderUI : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private UiScreenTextStyler uiScreenTextStyler;
        [SerializeField] private GameObject shieldBalanceObject;
        [SerializeField] private TextMeshProUGUI shieldBalanceText;
        [SerializeField] private Button addShieldButton;

        [SerializeField] private GameObject gameCurrencyBalanceObject;
        [SerializeField] private TextMeshProUGUI gameCurrencyBalanceText;
        [SerializeField] private Button addGameCurrencyButton;

        [SerializeField] private GameObject speedBoostBalanceObject;
        [SerializeField] private TextMeshProUGUI speedBoostBalanceText;
        [SerializeField] private Button addSpeedBoostButton;
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            GameController.GetInstance.PowerupController.OnBalanceChange += OnPowerupBalanceChange;
            GameController.GetInstance.StoreController.OnGameCurrencyBalanceChange += OnGameCurrencyBalanceChange;
            UpdateBalances();
            uiScreenTextStyler.ApplyStyle();
        }

        private void OnDisable()
        {
            if (GameController.GetInstance != null)
            {
                GameController.GetInstance.PowerupController.OnBalanceChange -= OnPowerupBalanceChange;
                GameController.GetInstance.StoreController.OnGameCurrencyBalanceChange -= OnGameCurrencyBalanceChange;
            }
            UnSetupAddButton(addShieldButton);
            UnSetupAddButton(addGameCurrencyButton);
            UnSetupAddButton(addSpeedBoostButton);
        }
        #endregion

        private void UpdateBalances()
        {
            var store = GameController.GetInstance.StoreController;

            if (gameCurrencyBalanceObject != null)
            {
                // Game Currency: Always visible
                gameCurrencyBalanceObject.SetActive(true);
                SetupAddButton(addGameCurrencyButton);
                UpdateText(gameCurrencyBalanceText, store.CoinsBalance);
            }

             if (shieldBalanceObject != null)
             {
                 // Magnet
                 bool isMagnetUnlocked = GameController.GetInstance.PowerupController.IsPowerupUnlocked(PowerupType.Shield);
                shieldBalanceObject.SetActive(isMagnetUnlocked);
                 if (isMagnetUnlocked)
                 {
                     SetupAddButton(addShieldButton);
                     UpdateText(shieldBalanceText, GameController.GetInstance.PowerupController.ShieldBalance);
                 }
             }

             if (speedBoostBalanceObject != null)
             {
                 //Speed Boost
                 bool isSpeedBoostUnlocked = GameController.GetInstance.PowerupController.IsPowerupUnlocked(PowerupType.SpeedBoost);
                 speedBoostBalanceObject.SetActive(isSpeedBoostUnlocked);
                 if (isSpeedBoostUnlocked)
                 {
                     SetupAddButton(addSpeedBoostButton);
                     UpdateText(speedBoostBalanceText, GameController.GetInstance.PowerupController.SpeedBoostBalance);
                 }
             }
        }

        private void UpdateText(TextMeshProUGUI text, int _balance)
        {
            text.text = _balance.ToString();
        }

        private void SetupAddButton(Button button)
        {
            if (button != null)
            {
                button.ButtonRegister(() =>
                {
                    UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Push);
                });
            }
        }
        private void UnSetupAddButton(Button button)
        {
            if (button != null)
            {
                button.ButtonDeRegisterAll();
            }
        }

        private void OnPowerupBalanceChange(PowerupType powerupType)
        {
            switch (powerupType)
            {
                case PowerupType.SpeedBoost:
                    UpdateText(speedBoostBalanceText, GameController.GetInstance.PowerupController.SpeedBoostBalance);
                    break;

                case PowerupType.Shield:
                    UpdateText(shieldBalanceText, GameController.GetInstance.PowerupController.ShieldBalance);
                    break;

                default:
                    DebugUtils.LogError($"Powerup {powerupType} not recognized or balance is zero.");
                    break;
            }
        }
        private void OnGameCurrencyBalanceChange()
        {
            UpdateText(gameCurrencyBalanceText, GameController.GetInstance.StoreController.CoinsBalance);
        }
    }
}
