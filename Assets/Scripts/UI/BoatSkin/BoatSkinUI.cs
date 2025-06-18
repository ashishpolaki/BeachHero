using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatSkinUI : MonoBehaviour
    {
        [SerializeField] private BoatCustomisationUIScreen boatCustomisationUIScreen;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image selectImage;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button realMoneyPurchaseButton;
        [SerializeField] private Button gameCurrencyPurchaseButton;
        [SerializeField] private Button colorEditButton;
        [SerializeField] private TextMeshProUGUI boatNameText;
        [SerializeField] private TextMeshProUGUI realMoneyPriceText;
        [SerializeField] private TextMeshProUGUI gameCurrencyPriceText;

        [SerializeField] private Color selectedColor;
        [SerializeField] private Color unselectedColor;
        private int index;
        private int currentColorIndex;

        #region Unity methods
        private void OnEnable()
        {
            // Add listeners for buttons
            colorEditButton.onClick.AddListener(OnEditColor);
            selectButton.onClick.AddListener(OnSelectButtonClicked);
            realMoneyPurchaseButton.onClick.AddListener(OnRealMoneyPurchaseButtonClicked);
            gameCurrencyPurchaseButton.onClick.AddListener(OnGameCurrencyPurchaseButtonClicked);
        }

        private void OnDisable()
        {
            // Remove listeners for buttons
            colorEditButton.onClick.RemoveAllListeners();
            selectButton.onClick.RemoveAllListeners();
            realMoneyPurchaseButton.onClick.RemoveAllListeners();
            gameCurrencyPurchaseButton.onClick.RemoveAllListeners();
        }
        #endregion

        public void SetSkin(BoatCustomisationUIScreen _boatCustomisationUIScreen, BoatSkinSO newBoatSkin)
        {
            boatCustomisationUIScreen = _boatCustomisationUIScreen;
            index = newBoatSkin.Index;
            DisableButtons();
            UnlockState(newBoatSkin);
            SetIcon(newBoatSkin.SkinColors[currentColorIndex].sprite);
            boatNameText.text = newBoatSkin.Name;
            speedSlider.value = newBoatSkin.SpeedMeter;
        }

        private void SetIcon(Sprite sprite)
        {
            currentColorIndex = SaveController.LoadInt(StringUtils.CURRENT_BOAT_COLOR_INDEX + index, 0);
            iconImage.sprite = sprite;
        }

        private void UnlockState(BoatSkinSO newBoatSkin)
        {
            bool isUnlocked = SaveController.LoadBool(StringUtils.BOAT_SKIN_UNLOCKED + index, false);
            selectButton.interactable = isUnlocked || newBoatSkin.IsDefaultBoat;
            if (isUnlocked || newBoatSkin.IsDefaultBoat)
            {
                realMoneyPurchaseButton.gameObject.SetActive(false);
                gameCurrencyPurchaseButton.gameObject.SetActive(false);
             
            }
            else
            {
                realMoneyPurchaseButton.gameObject.SetActive(newBoatSkin.IsPurchasableWithRealMoney);
                gameCurrencyPurchaseButton.gameObject.SetActive(newBoatSkin.IsPurchasableWithGameCurrency);
                realMoneyPriceText.text = $"Rs.{newBoatSkin.RealMoneyCost}";
                gameCurrencyPriceText.text = $"{newBoatSkin.InGameCurrencyCost}";
            }
        }

        public void SetSelected()
        {
            colorEditButton.gameObject.SetActive(true);
            selectImage.color = selectedColor;
            selectButton.interactable = false;
        }
        public void SetUnSelected()
        {
            colorEditButton.gameObject.SetActive(false);
            selectImage.color = unselectedColor;
            selectButton.interactable = true;
        }
        private void OnEditColor()
        {
            // Open color selection panel
            boatCustomisationUIScreen.OpenCustomisationPanel(index, currentColorIndex);
        }
        private void OnSelectButtonClicked()
        {
            SaveController.SaveInt(StringUtils.CURRENT_BOAT_INDEX, index);
            boatCustomisationUIScreen.SetCurrentBoatSelection(index);
        }
        private void DisableButtons()
        {
            realMoneyPurchaseButton.gameObject.SetActive(false);
            gameCurrencyPurchaseButton.gameObject.SetActive(false);
            colorEditButton.gameObject.SetActive(false);
        }
        private void OnRealMoneyPurchaseButtonClicked()
        {
            realMoneyPurchaseButton.gameObject.SetActive(false);
            gameCurrencyPurchaseButton.gameObject.SetActive(false);
            SaveController.SaveBool(StringUtils.BOAT_SKIN_UNLOCKED + index, true);
            OnSelectButtonClicked();
        }
        private void OnGameCurrencyPurchaseButtonClicked()
        {
            realMoneyPurchaseButton.gameObject.SetActive(false);
            gameCurrencyPurchaseButton.gameObject.SetActive(false);
            SaveController.SaveBool(StringUtils.BOAT_SKIN_UNLOCKED + index, true);
            OnSelectButtonClicked();
        }
    }
}
