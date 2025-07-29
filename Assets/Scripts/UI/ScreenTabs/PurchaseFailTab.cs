using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PurchaseFailTab : BaseScreenTab
    {
        [SerializeField] private Button retryPurchaseBtn; 

        public override void Open()
        {
            base.Open();
            retryPurchaseBtn.ButtonRegister(OnRetryPurchaseButtonClicked);
        }

        public override void Close()
        {
            base.Close();
            retryPurchaseBtn.ButtonDeRegister();
        }

        private void OnRetryPurchaseButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Close);
            GameController.GetInstance.StoreController.RetryPurchase();
        }
    }
}
