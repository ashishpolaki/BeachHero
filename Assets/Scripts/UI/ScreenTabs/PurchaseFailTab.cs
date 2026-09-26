using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PurchaseFailTab : BaseScreenTab
    {
        [SerializeField] private Button retryPurchaseBtn;
        [SerializeField] private Button backButton;

        public override void Open()
        {
            base.Open();
            retryPurchaseBtn.ButtonRegister(OnRetryPurchaseButtonClicked);
            backButton.ButtonRegister(OnBackButtonClicked);
        }

        public override void Close()
        {
            base.Close();
            retryPurchaseBtn.ButtonDeRegisterAll();
            backButton.ButtonDeRegisterAll();
        }

        private void OnRetryPurchaseButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Close);
            if (NetworkController.IsInternetAvailable)
            {
                GameController.GetInstance.StoreController.RetryPurchase();
            }
        }

        private void OnBackButtonClicked()
        {
            AudioController.GetInstance.PlaySound(AudioType.Swoosh);
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Close);
        }
    }
}
