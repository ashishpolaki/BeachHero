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

        public void SetSkin(BoatCustomisationUIScreen boatCustomisationUIScreen, BoatSkinData newBoatSkin,bool isSelected)
        {
            this.boatCustomisationUIScreen = boatCustomisationUIScreen;
            index = newBoatSkin.Index;
            iconImage.sprite = newBoatSkin.Icon;
            boatNameText.text = newBoatSkin.Name;
            speedSlider.value = newBoatSkin.SpeedMeter;
            if(isSelected)
            {
                colorEditButton.gameObject.SetActive(true);
                OnSelectButtonClicked();
            }
        }

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

        private void OnEditColor()
        {
            // Open color selection panel
            boatCustomisationUIScreen.OpenCustomisationPanel(index);
        }

        private void OnSelectButtonClicked()
        {
            selectImage.color = selectedColor;
        }

        private void OnRealMoneyPurchaseButtonClicked()
        {
            // Handle real money purchase logic here
            Debug.Log("Real money purchase button clicked.");
        }

        private void OnGameCurrencyPurchaseButtonClicked()
        {
            // Handle game currency purchase logic here
            Debug.Log("Game currency purchase button clicked.");
        }
    }
}
