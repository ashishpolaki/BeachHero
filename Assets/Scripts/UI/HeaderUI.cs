using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class HeaderUI : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private GameObject magnetBalanceObject;
        [SerializeField] private TextMeshProUGUI magnetBalanceText;
        [SerializeField] private Button addMagnetButton;

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
            GameController.GetInstance.PowerupController.OnMagnetBalanceChange += OnMagnetBalanceChange;
            GameController.GetInstance.StoreController.OnGameCurrencyBalanceChange += OnGameCurrencyBalanceChange;
            GameController.GetInstance.PowerupController.OnSpeedBoostBalanceChange += OnSpeedBoostBalanceChange;
            UpdateBalances();
        }

        private void OnDisable()
        {
            if (GameController.GetInstance != null)
            {
                GameController.GetInstance.PowerupController.OnMagnetBalanceChange -= OnMagnetBalanceChange;
                GameController.GetInstance.StoreController.OnGameCurrencyBalanceChange -= OnGameCurrencyBalanceChange;
                GameController.GetInstance.PowerupController.OnSpeedBoostBalanceChange -= OnSpeedBoostBalanceChange;
            }
            UnSetupAddButton(addMagnetButton);
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
                UpdateText(gameCurrencyBalanceText, store.GameCurrencyBalance);
            }

            if (magnetBalanceObject != null)
            {
                // Magnet
                bool isMagnetUnlocked = GameController.GetInstance.PowerupController.IsPowerupUnlocked(PowerupType.Magnet);
                magnetBalanceObject.SetActive(isMagnetUnlocked);
                if (isMagnetUnlocked)
                {
                    SetupAddButton(addMagnetButton);
                    UpdateText(magnetBalanceText, GameController.GetInstance.PowerupController.MagnetBalance);
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
                    UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Open);
                });
            }
        }
        private void UnSetupAddButton(Button button)
        {
            if (button != null)
            {
                button.ButtonDeRegister(() =>
                {
                    UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Open);
                });
            }
        }

        private void OnMagnetBalanceChange()
        {
            UpdateText(magnetBalanceText, GameController.GetInstance.PowerupController.MagnetBalance);
        }

        private void OnGameCurrencyBalanceChange()
        {
            UpdateText(gameCurrencyBalanceText, GameController.GetInstance.StoreController.GameCurrencyBalance);
        }

        private void OnSpeedBoostBalanceChange()
        {
            UpdateText(speedBoostBalanceText, GameController.GetInstance.PowerupController.SpeedBoostBalance);
        }
    }
}
