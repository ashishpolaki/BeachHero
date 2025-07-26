using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatSkinUI : MonoBehaviour
    {
        [SerializeField] private BoatCustomisationUIScreen boatCustomisationUIScreen;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image outerImage;
        [SerializeField] private Image innerImage;
        [SerializeField] private Button selectButton;

        [SerializeField] private Color outerImageSelectedColor;
        [SerializeField] private Color outerImageUnselectedColor;
        [SerializeField] private Color innerImageSelectedColor;
        [SerializeField] private Color innerImageUnselectedColor;

        private int index;
        private int currentColorIndex;

        #region Unity methods
        private void OnEnable()
        {
            // Add listeners for buttons
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        private void OnDisable()
        {
            // Remove listeners for buttons
            selectButton.onClick.RemoveAllListeners();
        }
        #endregion

        public void SetSkin(BoatCustomisationUIScreen _boatCustomisationUIScreen, BoatSkinSO newBoatSkin)
        {
            boatCustomisationUIScreen = _boatCustomisationUIScreen;
            index = newBoatSkin.Index;
            SetIcon(newBoatSkin.SkinColors[currentColorIndex].sprite);
        }

        private void SetIcon(Sprite sprite)
        {
            iconImage.sprite = sprite;
        }

        private void UnlockState(BoatSkinSO newBoatSkin)
        {
            bool isUnlocked = SaveSystem.LoadBool(StringUtils.BOAT_SKIN_UNLOCKED + index, false);
            selectButton.interactable = isUnlocked || newBoatSkin.IsDefaultBoat;
            if (isUnlocked || newBoatSkin.IsDefaultBoat)
            {
                //   realMoneyPurchaseButton.gameObject.SetActive(false);
                //   gameCurrencyPurchaseButton.gameObject.SetActive(false);
            }
            else
            {
                //realMoneyPurchaseButton.gameObject.SetActive(newBoatSkin.IsRealMoney);
                //gameCurrencyPurchaseButton.gameObject.SetActive(newBoatSkin.IsGameCurrency);
                //realMoneyPriceText.text = $"{newBoatSkin.RealMoneyCost}";
                //gameCurrencyPriceText.text = $"{newBoatSkin.InGameCurrencyCost}";
            }
        }

        public void SetSelected()
        {
            outerImage.color = outerImageSelectedColor;
            innerImage.color = innerImageSelectedColor;
            selectButton.interactable = false;
        }
        public void SetUnSelected()
        {
            outerImage.color = outerImageUnselectedColor;
            innerImage.color = innerImageUnselectedColor;
            selectButton.interactable = true;
        }
        private void OnSelectButtonClicked()
        {
            SaveSystem.SaveInt(StringUtils.CURRENT_BOAT_INDEX, index);
            boatCustomisationUIScreen.ChangeBoat();
        }
    }
}
