using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class InSufficientGameCurrencyTab : BaseScreenTab
    {
        [SerializeField] private Button goToStoreBtn;
        [SerializeField] private Button backButton;

        public override void Open()
        {
            base.Open();
            goToStoreBtn.ButtonRegister(OnGoToStoreButtonClicked);
            backButton.ButtonRegister(OnBackButtonClicked);
        }

        public override void Close()
        {
            base.Close();
            goToStoreBtn.ButtonDeRegisterAll();
            backButton.ButtonDeRegisterAll();
        }

        private void OnGoToStoreButtonClicked()
        {
            Close();
            //OpenStore
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Push, ScreenTabType.None);
        }

        private void OnBackButtonClicked()
        {
            AudioController.GetInstance.PlaySound(AudioType.Swoosh);
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Close);
        }
    }
}
