using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatPurchasePanel : MonoBehaviour
    {
        [SerializeField] private GameObject realMoneyContainer;
        [SerializeField] private GameObject gameCurrencyContainer;
        [SerializeField] private GameObject adContainer;

        [SerializeField] private Button realMoneyButton;
        [SerializeField] private Button gameCurrencyButton;
        [SerializeField] private Button adButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private TextMeshProUGUI realMoneyPriceLabel;
        [SerializeField] private TextMeshProUGUI gameCurrencyPriceLabel;

        [SerializeField] private Image selectedBoatImg;

        private BoatSelectionAction selectionAction;
        private int selectedBoatIndex = 0;
        private int selectedColorIndex = 0;

        #region Public Methods
        public void AddListeners()
        {
            realMoneyButton.ButtonRegister(OnRealMoneyPurchaseButtonClicked);
            gameCurrencyButton.ButtonRegister(OnGameCurrencyPurchaseButtonClicked);
            adButton.ButtonRegister(OnAdPurchaseButtonClicked);
            closeButton.ButtonRegister(Close);
        }
        public void RemoveListeners()
        {
            realMoneyButton.ButtonDeRegister();
            gameCurrencyButton.ButtonDeRegister();
            adButton.ButtonDeRegister();
            closeButton.ButtonDeRegister();
        }
        public void Close()
        {
            gameObject.SetActive(false);
        }
        #endregion

        private void HideAllPurchaseOptions()
        {
            realMoneyContainer.SetActive(false);
            gameCurrencyContainer.SetActive(false);
            adContainer.SetActive(false);
        }
        private void OnRealMoneyPurchaseButtonClicked()
        {
            GameController.GetInstance.StoreController.PurchaseWithRealMoney(selectedBoatIndex, PurchaseItemType.BoatSkin);
        }
        private void OnGameCurrencyPurchaseButtonClicked()
        {
            if (selectionAction == BoatSelectionAction.PurchaseSkin)
            {
                GameController.GetInstance.StoreController.BuyBoatWithGameCurrency(selectedBoatIndex);
            }
            else if (selectionAction == BoatSelectionAction.PurchaseSkinColor)
            {
                GameController.GetInstance.StoreController.BuyBoatColorWithGameCurrency(selectedBoatIndex, selectedColorIndex);
            }
        }
        private void OnAdPurchaseButtonClicked()
        {
            if (selectionAction == BoatSelectionAction.PurchaseSkin)
            {
                AdController.GetInstance.ShowRewardedAd((reward) =>
                {
                    GameController.GetInstance.SkinController.UnlockBoatSkin(selectedBoatIndex);
                });
            }
            else if (selectionAction == BoatSelectionAction.PurchaseSkinColor)
            {
                AdController.GetInstance.ShowRewardedAd((reward) =>
                {
                    GameController.GetInstance.SkinController.UnlockBoatSkinColor(selectedBoatIndex, selectedColorIndex);
                });
            }
        }

        public void InitPurchase(int selectedBoatIndex, int selectedColorIndex, BoatSelectionAction selectionAction)
        {
            HideAllPurchaseOptions();
            BoatSkinSO boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByIndex(selectedBoatIndex);
            selectedBoatImg.sprite = boatSkin.SkinColors[selectedColorIndex].sprite;
            this.selectedBoatIndex = selectedBoatIndex;
            this.selectedColorIndex = selectedColorIndex;
            this.selectionAction = selectionAction;

            switch (selectionAction)
            {
                case BoatSelectionAction.PurchaseSkin:
                    ShowSkinPurchaseOptions(boatSkin);
                    break;

                case BoatSelectionAction.PurchaseSkinColor:
                    ShowSkinColorPurchaseOptions(boatSkin);
                    break;
            }
            gameObject.SetActive(true);
        }

        private void ShowSkinPurchaseOptions(BoatSkinSO skin)
        {
            if (skin.IsGameCurrency)
            {
                gameCurrencyContainer.SetActive(true);
                gameCurrencyPriceLabel.text = skin.InGameCurrencyCost.ToString();
            }
            if (skin.IsRealMoney)
            {
                realMoneyContainer.SetActive(true);
                realMoneyPriceLabel.text = skin.RealMoneyCost;
            }
        }

        private void ShowSkinColorPurchaseOptions(BoatSkinSO skin)
        {
            var colorData = skin.SkinColors[selectedColorIndex];
            if (colorData.isGameCurrency)
            {
                gameCurrencyContainer.SetActive(true);
                gameCurrencyPriceLabel.text = colorData.inGameCurrencyCost.ToString();
            }
            if (colorData.isAds)
            {
                adContainer.SetActive(true);
            }
        }
    }
}
