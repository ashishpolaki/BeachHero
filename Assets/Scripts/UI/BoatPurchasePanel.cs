using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class BoatPurchasePanel : MonoBehaviour
    {
        [SerializeField] private GameObject realMoneyGameobject;
        [SerializeField] private GameObject gameCurrencyGameobject;
        [SerializeField] private GameObject adGameobject;
        [SerializeField] private Button realMoneyPurchaseButton;
        [SerializeField] private Button gameCurrencyPurchaseButton;
        [SerializeField] private Button adPurchaseButton;
        [SerializeField] private TextMeshProUGUI realMoneyPriceText;
        [SerializeField] private TextMeshProUGUI gameCurrencyPriceText;
        [SerializeField] private TextMeshProUGUI adPriceText;

        private int currentBoatIndex = 0;
        private int currentBoatColorIndex = 0;

        public void AddListeners()
        {
            realMoneyPurchaseButton.ButtonRegister(OnRealMoneyPurchaseButtonClicked);
            gameCurrencyPurchaseButton.ButtonRegister(OnGameCurrencyPurchaseButtonClicked);
            adPurchaseButton.ButtonRegister(OnAdPurchaseButtonClicked);
        }
        public void RemoveListeners()
        {
            realMoneyPurchaseButton.ButtonDeRegister();
            gameCurrencyPurchaseButton.ButtonDeRegister();
            adPurchaseButton.ButtonDeRegister();
        }
        public void Open(int boatIndex,int boatColorIndex,Sprite sprite)
        {

        }
        private void OnRealMoneyPurchaseButtonClicked()
        {
            if (currentBoatIndex != -1)
            {
                GameController.GetInstance.StoreController.PurchaseWithRealMoney(currentBoatIndex, PurchaseItemType.BoatSkin);
            }
        }
        private void OnGameCurrencyPurchaseButtonClicked()
        {
            if (currentBoatIndex != -1)
            {
                GameController.GetInstance.SkinController.TryPurchaseWithGameCurrency(currentBoatIndex);
            }
        }
        private void OnAdPurchaseButtonClicked()
        {
           
        }
    }
}
